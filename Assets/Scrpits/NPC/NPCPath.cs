using System;
using System.Collections.Generic;
using UnityEngine;

// 确保该脚本所在的游戏对象必须拥有NPCMovement组件，若没有则会在编辑器中报错
[RequireComponent(typeof(NPCMovement))]
public class NPCPath : MonoBehaviour
{
    // 用于存储NPC移动步骤的栈，每个步骤可能包含移动到的网格坐标等信息
    public Stack<NPCMovementStep> npcMovementStepStack;

    // 引用NPCMovement组件，用于获取NPC当前的移动相关信息
    private NPCMovement npcMovement;

    private void Awake()
    {
        npcMovement = GetComponent<NPCMovement>();
       
        npcMovementStepStack = new Stack<NPCMovementStep>();
    }

    // 清空移动步骤栈
    public void ClearPath()
    {
        npcMovementStepStack.Clear();
    }

    // 根据NPC的日程事件构建路径
    public void BuildPath(NPCScheduleEvent npcScheduleEvent)
    {
        // 先清空之前的路径
        ClearPath();

        // 如果日程事件的目标场景和当前NPC所在场景相同
        // 检查NPC日程事件的目标场景是否与NPC当前所在场景相同
        if (npcScheduleEvent.toSceneName == npcMovement.npcCurrentScene)
        {
            // 获取NPC当前的网格位置并转换为Vector2Int类型
            Vector2Int npcCurrentGridPosition = (Vector2Int)npcMovement.npcCurrentGridPosition;

            // 获取日程事件中目标的网格坐标并转换为Vector2Int类型
            Vector2Int npcTargetGridPosition = (Vector2Int)npcScheduleEvent.toGridCoordinate;

            // 调用NPCManager单例的BuildPath方法构建路径，并将移动步骤添加到移动步骤栈中
            // 单例模式确保系统中只有一个NPCManager实例来全局管理路径逻辑，但该实例可以同时处理多个NPC的路径计算
            NPCManager.Instance.BuildPath(npcScheduleEvent.toSceneName, npcCurrentGridPosition, npcTargetGridPosition, npcMovementStepStack);

        }
        // 检查NPC日程事件的目标场景是否与NPC当前所在场景不同（跨场景移动）
        else if (npcScheduleEvent.toSceneName != npcMovement.npcCurrentScene)
        {
            // 声明场景路由变量，用于存储跨场景的路径信息
            SceneRoute sceneRoute;

            // 获取匹配当前场景到目标场景的场景路由
            // 将枚举类型转换为字符串作为参数传入
            sceneRoute = NPCManager.Instance.GetSceneRoute(npcMovement.npcCurrentScene.ToString(), npcScheduleEvent.toSceneName.ToString());

            // 检查是否找到了有效的场景路由
            if (sceneRoute != null)
            {
                // 倒序遍历场景路径列表（因为npcMovementStepStack是栈结构，后进先出）
                for (int i = sceneRoute.scenePathList.Count - 1; i >= 0; i--)
                {
                    // 声明目标网格坐标和起始网格坐标的变量
                    int toGridX, toGridY, fromGridX, fromGridY;

                    // 获取当前遍历的场景路径
                    ScenePath scenePath = sceneRoute.scenePathList[i];

                    // 检查当前路径是否是最终目的地（通过判断坐标是否超出网格最大范围）
                    if (scenePath.toGridCell.x >= Settings.maxGridWidth || scenePath.toGridCell.y >= Settings.maxGridHeight)
                    {
                        // 如果是最终目的地，则使用日程事件中指定的目标坐标
                        toGridX = npcScheduleEvent.toGridCoordinate.x;
                        toGridY = npcScheduleEvent.toGridCoordinate.y;
                    }
                    else
                    {
                        // 否则使用场景路径中定义的目标坐标
                        toGridX = scenePath.toGridCell.x;
                        toGridY = scenePath.toGridCell.y;
                    }

                    // 检查当前路径是否是起始位置（通过判断坐标是否超出网格最大范围）
                    if (scenePath.fromGridCell.x >= Settings.maxGridWidth || scenePath.fromGridCell.y >= Settings.maxGridHeight)
                    {
                        // 如果是起始位置，则使用NPC当前的实际位置
                        fromGridX = npcMovement.npcCurrentGridPosition.x;
                        fromGridY = npcMovement.npcCurrentGridPosition.y;
                    }
                    else
                    {
                        // 否则使用场景路径中定义的起始坐标
                        fromGridX = scenePath.fromGridCell.x;
                        fromGridY = scenePath.fromGridCell.y;
                    }

                    // 创建起始网格位置的Vector2Int对象
                    Vector2Int fromGridPosition = new Vector2Int(fromGridX, fromGridY);
                    // 创建目标网格位置的Vector2Int对象
                    Vector2Int toGridPosition = new Vector2Int(toGridX, toGridY);

                    // 构建路径并将移动步骤添加到移动步骤栈中
                    // 使用场景路径中定义的场景名称、起始位置和目标位置
                    NPCManager.Instance.BuildPath(scenePath.SceneName, fromGridPosition, toGridPosition, npcMovementStepStack);
                }
            }
        }



        // 如果栈中的元素数量大于1（即路径存在有效步骤，因为起始位置也算一个元素）
        if (npcMovementStepStack.Count > 1)
        {
            // 更新路径上各步骤的时间信息
            UpdateTimesOnPath();
            // 弹出栈顶元素（即起始位置步骤，因为不需要移动到自身）
            npcMovementStepStack.Pop();

            // 在NPCMovement组件中设置日程事件的详细信息
            npcMovement.SetScheduleEventDetails(npcScheduleEvent);
        }

    }

    // 使用预期的游戏时间更新路径移动步骤
    public void UpdateTimesOnPath()
    {
        // 获取当前的游戏时间
        TimeSpan currentGameTime = TimeManager.Instance.GetGameTime();

        NPCMovementStep previousNPCMovementStep = null;

        // 遍历移动步骤栈中的每个步骤
        foreach (NPCMovementStep npcMovementStep in npcMovementStepStack)
        {
            if (previousNPCMovementStep == null)
            {
                previousNPCMovementStep = npcMovementStep;
            }

            // 设置当前步骤的小时、分钟、秒为当前游戏时间
            npcMovementStep.hour = currentGameTime.Hours;
            npcMovementStep.minute = currentGameTime.Minutes;
            npcMovementStep.second = currentGameTime.Seconds;

            TimeSpan movementTimeStep;

            // 判断当前步骤和前一个步骤的移动是否为对角线移动
            if (MovementIsDiagonal(npcMovementStep, previousNPCMovementStep))
            {
                // 根据设置的网格单元格对角线大小、每秒游戏时间和NPC正常速度计算对角线移动所需时间
                movementTimeStep = new TimeSpan(0, 0, (int)(Settings.gridCellDiagonalSize / Settings.secondsPerGameSecond / npcMovement.npcNormalSpeed));
            }
            else
            {
                // 根据设置的网格单元格大小、每秒游戏时间和NPC正常速度计算普通移动所需时间
                movementTimeStep = new TimeSpan(0, 0, (int)(Settings.gridCellSize / Settings.secondsPerGameSecond / npcMovement.npcNormalSpeed));
            }

            // 更新当前游戏时间，加上本次移动所需时间
            currentGameTime = currentGameTime.Add(movementTimeStep);

            previousNPCMovementStep = npcMovementStep;
        }
    }

    // 判断两个移动步骤之间的移动是否为对角线移动
    private bool MovementIsDiagonal(NPCMovementStep npcMovementStep, NPCMovementStep previoudNPCMovementStep)
    {
        // 如果当前步骤和前一个步骤的x坐标和y坐标都不同，则为对角线移动
        if ((npcMovementStep.gridCoordinate.x != previoudNPCMovementStep.gridCoordinate.x)
            && (npcMovementStep.gridCoordinate.y != previoudNPCMovementStep.gridCoordinate.y))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
