using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(GenerateGUID))]
public class SceneItemsManager : SingletonMonobehaviour<SceneItemsManager>, ISaveable
{
    private Transform parentItem;
    [SerializeField] private GameObject itemPrefab = null;

    private string _iSaveableUniqueID;
    public string ISaveableUniqueID { get { return _iSaveableUniqueID; } set { _iSaveableUniqueID = value; } }// 实现ISaveable接口的唯一ID属性

    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get { return _gameObjectSave; } set { _gameObjectSave = value; } }// 实现ISaveable接口的存档数据属性

    private void AfterSceneLoad()
    {
        parentItem = GameObject.FindGameObjectWithTag(Tags.ItemsParentTransform).transform;// 通过标签查找物品父节点并缓存引用
    }

    protected override void Awake()
    {
        base.Awake();

        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;// 从GUID组件获取唯一标识符
        GameObjectSave = new GameObjectSave();
    }

    // Destroy items currently in the scene销毁当前场景中的物品
    private void DestroySceneItems()
    {
        // Get all items in the scene获取场景中所有Item组件
        Item[] itemsInScene = GameObject.FindObjectsOfType<Item>();

        // Loop through all scene items and destroy them遍历所有场景物品并销毁它们
        for (int i = itemsInScene.Length - 1; i > -1; i--)
        {
            Destroy(itemsInScene[i].gameObject);
        }
    }

    public void InstantiateSceneItem(int itemCode, Vector3 itemPosition)//实例化单个场景物品
    {
        GameObject itemGameObject = Instantiate(itemPrefab, itemPosition, Quaternion.identity, parentItem);// 实例化预制体并设置父对象
        Item item = itemGameObject.GetComponent<Item>();
        item.Init(itemCode);// 初始化物品数据
    }

    private void InstantiateSceneItems(List<SceneItem> sceneItemList)// 批量实例化场景物品
    {
        GameObject itemGameObject;

        foreach (SceneItem sceneItem in sceneItemList)// 遍历存档物品列表
        {
            itemGameObject = Instantiate(itemPrefab, new Vector3(sceneItem.position.x, sceneItem.position.y, sceneItem.position.z), Quaternion.identity, parentItem);// 根据存档数据实例化物品

            Item item = itemGameObject.GetComponent<Item>();
            item.ItemCode = sceneItem.itemCode;
            item.name = sceneItem.itemName;
        }
    }

    private void OnDisable()
    {
        ISaveableDeregister();// 从存档系统注销
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
    }

    private void OnEnable()
    {
        ISaveableRegister();// 向存档系统注册
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
    }

    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);// 从存档管理器移除当前对象
    }

    public void ISaveableLoad(GameSave gameSave)
    {
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameObjectSave gameObjectSave))
        {
            GameObjectSave = gameObjectSave;

            // Restore data for current scene恢复当前场景的数据
            ISaveableRestoreScene(SceneManager.GetActiveScene().name);
        }
    }



    public void ISaveableRestoreScene(string sceneName)
    {
        if (GameObjectSave.sceneData.TryGetValue(sceneName, out SceneSave sceneSave))// 尝试获取指定场景存档
        {
            if (sceneSave.listSceneItem != null )// 检查并获取物品列表数据
            {
                // scene list items found - destroy existing items in scene清理现有物品
                DestroySceneItems();

                // new instantiate the list of scene items根据存档恢复物品
                InstantiateSceneItems(sceneSave.listSceneItem);
            }
        }
    }

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);// 向存档管理器注册当前对象
    }

    public GameObjectSave ISaveableSave()
    {
        // Store current scene data存储当前场景数据
        ISaveableStoreScene(SceneManager.GetActiveScene().name);

        return GameObjectSave;
    }

    public void ISaveableStoreScene(string sceneName)
    {
        // Remove old scene save for gameObject if exists清除旧场景数据
        GameObjectSave.sceneData.Remove(sceneName);

        // Get all items in the scene获取场景中的所有物品
        List<SceneItem> sceneItemList = new List<SceneItem>(); //创建新物品列表
        Item[] itemsInScene = FindObjectsOfType<Item>();// 获取场景所有物品

        // Loop through all scene items遍历场景物品
        foreach (Item item in itemsInScene)
        {
            SceneItem sceneItem = new SceneItem();// 创建存档物品数据
            sceneItem.itemCode = item.ItemCode;// 记录物品代码
            sceneItem.position = new Vector3Serializable(item.transform.position.x,item.transform.position.y,item.transform.position.z);// 序列化物品位置
            sceneItem.itemName = item.name;// 记录物品名称

            // Add scene item to list添加到存档列表
            sceneItemList.Add(sceneItem);
        }

        // Create list scene items in scene save and set to scene item list 在场景保存中创建场景列表项并设置为场景项列表
        SceneSave sceneSave = new SceneSave();// 创建场景存档数据
        sceneSave.listSceneItem= sceneItemList;

        // Add scene save to gameobject为游戏对象添加场景保存功能
        GameObjectSave.sceneData.Add(sceneName, sceneSave);// 保存到全局存档

    }
    //核心功能概述
    //该脚本实现了Unity场景物品的三种核心功能：
    //1.物品实例化管理：通过InstantiateSceneItem和InstantiateSceneItems方法动态生成物品
    //2.场景状态持久化：实现ISaveable接口实现场景物品的存档/读档
    //3.生命周期控制：通过OnEnable/OnDisable管理对象激活状态

    //关键逻辑分层解析
    //1. 初始化阶段
    //Awake()：生成唯一GUID标识，初始化存档容器
    //AfterSceneLoad()：定位场景物品父节点
    //2. 物品管理
    //动态生成：
    //单物品：InstantiateSceneItem(itemCode, position)
    //批量生成：InstantiateSceneItems(List)
    //清理机制：DestroySceneItems()销毁当前场景所有物品
    //3. 存档系统实现
    //注册机制：ISaveableRegister()将对象加入存档管理器
    //场景保存：
    //序列化所有Item的位置和属性
    //存储为SceneItem对象字典
    //按sceneName分组存储
    //场景恢复：
    //读取存档数据
    //先销毁现有物品
    //按存档数据重建场景
    //4. 生命周期管理
    //激活管理：
    //OnEnable()：注册存档系统+订阅场景加载事件
    //OnDisable()：反注册+取消事件订阅
    //线程安全：采用倒序销毁物品的循环模式
}