using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(AStar))]
public class NPCManager : SingletonMonobehaviour<NPCManager>
{
    [SerializeField] private SO_SceneRouteList so_SceneRoutelist = null;
    private Dictionary <string, SceneRoute> sceneRouteDictionary;

    [HideInInspector]
    public NPC[] npcArray;

    private AStar aStar;

    protected override void Awake()
    {
        base.Awake();

        // Create sceneRoute dictionary 创建场景路线字典 
        sceneRouteDictionary = new Dictionary<string, SceneRoute>();

        //字典初始化与去重逻辑
        if (so_SceneRoutelist.sceneRouteList.Count > 0)
        {
            foreach (SceneRoute so_sceneRoute in so_SceneRoutelist.sceneRouteList)

            {
                // Check for duplicate routes in dictionary 检查字典中是否存在重复路径 
                if (sceneRouteDictionary.ContainsKey(so_sceneRoute.fromSceneName.ToString() + so_sceneRoute.toSceneName.ToString()))
                {
                    Debug.Log(" 发现重复场景路线键 ,请检查脚本对象场景路线列表中是否存在重复路线"); 
                continue;
                }
                // Add route to dictionary 将路线添加到字典中
                sceneRouteDictionary.Add(so_sceneRoute.fromSceneName.ToString() + so_sceneRoute.toSceneName.ToString(), so_sceneRoute);
            }
        }


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

    public SceneRoute GetSceneRoute(string fromSceneName, string toScceneName)//字典中按场景对查询路线
    {
        SceneRoute sceneRoute;

        // Get scene route from dictionary从字典中获取场景路线
        if (sceneRouteDictionary.TryGetValue(fromSceneName + toScceneName, out sceneRoute))
        {
            return sceneRoute;
        }
        else
        {
            return null;
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