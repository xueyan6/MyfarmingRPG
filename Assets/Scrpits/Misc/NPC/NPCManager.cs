using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(AStar))]
public class NPCManager : SingletonMonobehaviour<NPCManager>
{
    [HideInInspector]
    public NPC[] npcArray;

    private AStar aStar;

    protected override void Awake()
    {
        base.Awake();
        aStar = GetComponent<AStar>();

        // Get NPC gameobjects in scene
        npcArray = FindObjectsOfType<NPC>();
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
    }

    private void AfterSceneLoad()
    {
        SetNPCsActiveStatus();
    }

    private void SetNPCsActiveStatus()
    {
        // 遍历 npcArray 数组中的每一个 NPC 对象
        foreach (NPC npc in npcArray)
        {
            // 获取当前 NPC 对象上挂载的 NPCMovement 组件
            NPCMovement nPCMovement = npc.GetComponent<NPCMovement>();

            // 将当前 NPC 的当前场景名称转换为字符串，并与当前活动场景的名称进行比较
            if (nPCMovement.npcCurrentScene.ToString() == SceneManager.GetActiveScene().name)
            {
                // 如果当前 NPC 的当前场景与活动场景相同，调用 SetNPCActiveInScene 方法将其设置为活动状态
                nPCMovement.SetNPCActiveInScene();
            }
            else
            {
                // 如果当前 NPC 的当前场景与活动场景不同，调用 SetNPCInactiveInScene 方法将其设置为非活动状态
                nPCMovement.SetNPCInactiveInScene();
            }
        }
    }

    public bool BuildPath(SceneName sceneName, Vector2Int startGridPosition, Vector2Int endGridPosition, Stack<NPCMovementStep> npcMovementStepStack)
    {
        // 调用 aStar 对象的 BuildPath 方法来构建路径，传入场景名称、起始网格位置、结束网格位置以及存储路径步骤的栈
        if (aStar.BuildPath(sceneName, startGridPosition, endGridPosition, npcMovementStepStack))
        {
            // 如果路径构建成功，返回 true
            return true;
        }
        else
        {
            // 如果路径构建失败，返回 false
            return false;
        }
    }


}