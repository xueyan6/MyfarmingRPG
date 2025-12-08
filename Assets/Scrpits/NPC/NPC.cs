using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NPCMovement))]
[RequireComponent(typeof(GenerateGUID))]
public class NPC : MonoBehaviour, ISaveable
{
    // 实现ISaveable接口
    private string _iSaveableUniqueID;
    // 私有字段存储唯一ID
    public string ISaveableUniqueID { get { return _iSaveableUniqueID; } set { _iSaveableUniqueID = value; } }
    // 公共属性访问唯一ID
    private GameObjectSave _gameObjectSave;
    // 私有字段存储游戏对象保存数据
    public GameObjectSave GameObjectSave { get { return _gameObjectSave; } set { _gameObjectSave = value; } }
    // 公共属性访问游戏对象保存数据
    private NPCMovement npcMovement;
    // 私有字段存储NPC移动组件

    private void OnEnable()
    {
        ISaveableRegister();
        // 启用时注册保存对象
    }

    private void OnDisable()
    {
        ISaveableDeregister();
        // 禁用时注销保存对象
    }

    private void Awake()
    {
        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        // 初始化唯一ID
        GameObjectSave = new GameObjectSave();
        // 初始化游戏对象保存数据
    }

    private void Start()
    {
        npcMovement = GetComponent<NPCMovement>();
        // 获取NPC移动组件
    }

    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
        // 从保存管理器中移除对象
    }

    public void ISaveableLoad(GameSave gameSave)
    {
        // 尝试从gameSave的gameObjectData字典中获取当前NPC的GameObjectSave数据
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameObjectSave gameObjectSave))
        {
            GameObjectSave = gameObjectSave; // 更新GameObjectSave字段
            // 确保目标场景数据存在，后续操作基于有效数据进行
            if (GameObjectSave.sceneData.TryGetValue(Settings.PersistentScene, out SceneSave sceneSave))
            {
                // 检查sceneSave对象中的两个字典是否已初始化
                if (sceneSave.vector3Dictionary != null && sceneSave.stringDictionary != null)
                {
                    // 尝试从vector3Dictionary字典中获取目标网格位置数据
                    if (sceneSave.vector3Dictionary.TryGetValue("npcTargetGridPosition", out Vector3Serializable savedNPCTargetGridPosition))
                    {
                        // 将保存的网格位置转换为Vector3Int并更新NPC移动组件
                        npcMovement.npcTargetGridPosition = new Vector3Int((int)savedNPCTargetGridPosition.x, (int)savedNPCTargetGridPosition.y, (int)savedNPCTargetGridPosition.z);
                        npcMovement.npcCurrentGridPosition = npcMovement.npcTargetGridPosition; // 同步当前网格位置
                                                                                                // 加载目标网格位置
                    }

                    // 尝试从vector3Dictionary字典中获取目标世界位置数据
                    if (sceneSave.vector3Dictionary.TryGetValue("npcTargetWorldPosition", out Vector3Serializable savedNPCTargetWorldPosition))
                    {
                        // 将保存的世界位置转换为Vector3并更新NPC的Transform位置
                        npcMovement.npcTargetWorldPosition = new Vector3(savedNPCTargetWorldPosition.x, savedNPCTargetWorldPosition.y, savedNPCTargetWorldPosition.z);
                        transform.position = npcMovement.npcTargetWorldPosition; // 同步Transform位置
                                                                                 // 加载目标世界位置
                    }

                    // 尝试从stringDictionary字典中获取目标场景名称
                    if (sceneSave.stringDictionary.TryGetValue("npcTargetScene", out string savedTargetScene))
                    {
                        // 将字符串转换为枚举类型SceneName
                        if (Enum.TryParse<SceneName>(savedTargetScene, out SceneName sceneName))
                        {
                            // 更新NPC移动组件的目标场景
                            npcMovement.npcTargetScene = sceneName;
                            npcMovement.npcCurrentScene = npcMovement.npcTargetScene; // 同步当前场景
                                                                                      // 加载目标场景
                        }
                    }

                    // 取消当前移动操作，确保加载后状态一致
                    npcMovement.CancelNPCMovement();
                    // 取消当前移动
                }
            }
        }
    }


    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
        // 注册保存对象
    }

    public void ISaveableRestoreScene(string sceneName)
    {
        // 持久化场景无需恢复
    }

    public GameObjectSave ISaveableSave()
    {
        GameObjectSave.sceneData.Remove(Settings.PersistentScene);
        // 移除当前场景数据
        SceneSave sceneSave = new SceneSave();
        sceneSave.vector3Dictionary = new Dictionary<string, Vector3Serializable>();
        sceneSave.stringDictionary = new Dictionary<string, string>();
        // 初始化场景保存数据
        sceneSave.vector3Dictionary.Add("npcTargetGridPosition", new Vector3Serializable(npcMovement.npcTargetGridPosition.x, npcMovement.npcTargetGridPosition.y, npcMovement.npcTargetGridPosition.z));
        sceneSave.vector3Dictionary.Add("npcTargetWorldPosition", new Vector3Serializable(npcMovement.npcTargetWorldPosition.x, npcMovement.npcTargetWorldPosition.y, npcMovement.npcTargetWorldPosition.z));
        sceneSave.stringDictionary.Add("npcTargetScene", npcMovement.npcTargetScene.ToString());
        // 保存目标位置和场景
        GameObjectSave.sceneData.Add(Settings.PersistentScene, sceneSave);
        return GameObjectSave;
    }

    public void ISaveableStoreScene(string sceneName)
    {
        // 持久化场景无需存储
    }
}
