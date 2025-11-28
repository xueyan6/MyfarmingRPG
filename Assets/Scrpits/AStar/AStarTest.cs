using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(AStar))]  // Unity特性：强制要求该游戏对象必须附加AStar组件，如果不存在会自动添加
public class AStarTest : MonoBehaviour  
{
    private AStar aStar;  // 引用A*寻路算法组件实例
    [SerializeField] private Vector2Int startPosition;  // 序列化字段：在Inspector面板显示起始位置
    [SerializeField] private Vector2Int finishPosition;  // 序列化字段：在Inspector面板显示目标位置
    [SerializeField] private Tilemap tileMapToDisplayPathOn = null;  // 序列化字段：用于显示路径的Tilemap引用
    [SerializeField] private TileBase tileToUseToDisplayPath = null;  // 序列化字段：用于显示路径的Tile资源引用
    [SerializeField] private bool displayStartAndFinish = false;  // 序列化字段：控制是否显示起点和终点
    [SerializeField] private bool displayPath = false;  // 序列化字段：控制是否显示完整路径

    private Stack<NPCMovementStep> npcMovementSteps;  // 存储NPC移动步骤的栈数据结构

    private void Awake()  
    {
        aStar = GetComponent<AStar>();  // 获取附加在同一游戏对象上的AStar组件实例
        npcMovementSteps = new Stack<NPCMovementStep>();  // 初始化NPC移动步骤栈
    }

    private void Update()  
    {
        if (startPosition != null && finishPosition != null && tileMapToDisplayPathOn != null && tileToUseToDisplayPath != null)  // 检查所有必要引用是否已设置
        {
            // Display start and finish tiles显示起点和终点瓦片
            if (displayStartAndFinish)  // 如果启用了显示起点和终点功能
            {
                // Display start tile 显示起点瓦片
                tileMapToDisplayPathOn.SetTile(new Vector3Int(startPosition.x, startPosition.y, 0), tileToUseToDisplayPath);  // 在Tilemap上设置起点位置的瓦片

                // Display finish tile 显示终点瓦片
                tileMapToDisplayPathOn.SetTile(new Vector3Int(finishPosition.x, finishPosition.y, 0), tileToUseToDisplayPath);  // 在Tilemap上设置终点位置的瓦片
            }
            else  // Clear start and finish 清除起点和终点显示
            {
                // clear start tile  清除起点瓦片
                tileMapToDisplayPathOn.SetTile(new Vector3Int(startPosition.x, startPosition.y, 0), null);  // 将起点位置的瓦片设置为null（清除显示）

                // clear finish tile  清除终点瓦片
                tileMapToDisplayPathOn.SetTile(new Vector3Int(finishPosition.x, finishPosition.y, 0), null);  // 将终点位置的瓦片设置为null（清除显示）
            }

            // Display path  显示完整路径
            if (displayPath)  // 如果启用了显示完整路径功能
            {
                // Get current scene name  获取当前场景名称
                Enum.TryParse<SceneName>(SceneManager.GetActiveScene().name, out SceneName sceneName);  // 将当前场景名称转换为SceneName枚举类型

                // Build path  构建路径
                aStar.BuildPath(sceneName, startPosition, finishPosition, npcMovementSteps);  // 调用AStar组件的BuildPath方法计算路径

                // Display path on tilemap  在Tilemap上显示路径
                foreach (NPCMovementStep npcMovementStep in npcMovementSteps)  // 遍历NPC移动步骤栈中的所有步骤
                {
                    tileMapToDisplayPathOn.SetTile(new Vector3Int(npcMovementStep.gridCoordinate.x, npcMovementStep.gridCoordinate.y, 0), tileToUseToDisplayPath);  // 在Tilemap上设置每个路径位置的瓦片
                }
            }
            else  // 否则：清除路径显示
            {
                // Clear path  清除路径
                if (npcMovementSteps.Count > 0)  // 检查移动步骤栈是否包含步骤
                {
                    // Clear path on tilemap  在Tilemap上清除路径显示
                    foreach (NPCMovementStep npcMovementStep in npcMovementSteps)  // 遍历所有移动步骤
                    {
                        tileMapToDisplayPathOn.SetTile(new Vector3Int(npcMovementStep.gridCoordinate.x, npcMovementStep.gridCoordinate.y, 0), null);  // 将每个路径位置的瓦片设置为null（清除显示）
                    }

                    // Clear movement steps  清除移动步骤
                    npcMovementSteps.Clear();  // 清空NPC移动步骤栈
                }
            }

        }
    }
}
