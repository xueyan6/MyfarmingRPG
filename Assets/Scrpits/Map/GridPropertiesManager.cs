using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GenerateGUID))]
public class GridPropertiesManager : SingletonMonobehaviour<GridPropertiesManager>, ISaveable
{
    public Grid grid;
    private Dictionary<string, GridPropertyDetails> gridPropertyDictionary;
    [SerializeField] private SO_GridProperties[] so_gridPropertiesArray = null;

    private string _iSaveableUniqueID;
    public string ISaveableUniqueID { get { return _iSaveableUniqueID; } set { _iSaveableUniqueID = value; } }

    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get { return _gameObjectSave; } set { _gameObjectSave = value; } }

    protected override void Awake()
    {
        base.Awake();

        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }

    private void OnEnable()
    {
        ISaveableRegister();// 注册到存档系统

        EventHandler.AfterSceneLoadEvent += AfterSceneLoaded;
    }

    private void OnDisable()
    {
        ISaveableDeregister();// 从存档系统注销

        EventHandler.AfterSceneLoadEvent -= AfterSceneLoaded;
    }

    private void Start()
    {
        InitialiseGridProperties();
    }


    // This initialises the grid property dictionary with the values from the SO_GridProperties assets and stores the values for each scene in GameObjectSave sceneData
    //此操作将网格属性字典初始化为SO_GridProperties资源中的值，并将每个场景的值存储在GameObjectSave场景数据中。
    private void InitialiseGridProperties()
    {
        // Loop through all gridproperties in the array遍历所有网格配置
        foreach (SO_GridProperties so_GridProperties in so_gridPropertiesArray)
        {
            // Create dictionary of grid property details创建网格属性详细信息字典
            Dictionary<string, GridPropertyDetails> gridPropertyDictionary = new Dictionary<string, GridPropertyDetails>();

            // Populate grid property dictionary - Iterate through all the grid properties in the so gridproperties list
            //填充网格属性字典 - 遍历so gridproperties列表中的所有网格属性
            foreach (GridProperty gridProperty in so_GridProperties.gridPropertyList)
            {
                GridPropertyDetails gridPropertyDetails;

                gridPropertyDetails = GetGridPropertyDetails(gridProperty.gridCoordinate.x, gridProperty.gridCoordinate.y, gridPropertyDictionary);

                if (gridPropertyDetails == null)
                {
                    gridPropertyDetails = new GridPropertyDetails();
                }
                // 设置五种布尔属性
                switch (gridProperty.gridBoolProperty)
                {
                    case GridBoolProperty.diggable:
                        gridPropertyDetails.isDiggable = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.canDropItem:
                        gridPropertyDetails.canDropItem = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.canPlaceFurniture:
                        gridPropertyDetails.canPlaceFurniture = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.isPath:
                        gridPropertyDetails.isPath = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.isNPCObstacle:
                        gridPropertyDetails.isNPCObstacle = gridProperty.gridBoolValue;
                        break;

                    default:
                        break;
                }
                // 场景数据保存
                SetGridPropertyDetails(gridProperty.gridCoordinate.x, gridProperty.gridCoordinate.y, gridPropertyDetails, gridPropertyDictionary);

            }

            // Create scene save for this gameobject为该游戏对象创建场景保存
            SceneSave sceneSave = new SceneSave();

            // Add grid property dictionary to scene save data将网格属性字典添加到场景保存数据中
            sceneSave.gridPropertyDetailsDictionary = gridPropertyDictionary;

            // If starting scene set the griProertyDictionary member variable to the current iteration
            //// 如果是起始场景则设为全局字典
            if (so_GridProperties.sceneName.ToString() == SceneControllerManager.Instance.startingSceneName.ToString())
            {
                this.gridPropertyDictionary = gridPropertyDictionary;
            }

            // Add scene save to game object scene data将场景保存添加到游戏对象场景数据中
            GameObjectSave.sceneData.Add(so_GridProperties.sceneName.ToString(), sceneSave);
        }
    }


    private void AfterSceneLoaded() // 获取网格组件
    {
        // Get Grid
        grid = GameObject.FindObjectOfType<Grid>();
    }


    //属性查询方法

    // Returns the gridPropertyDetails at the gridlocation fro the supplied dictionary,or null if no properties exist at that location
    //返回指定字典中网格位置的网格属性详情，若该位置不存在属性则返回null。
    public GridPropertyDetails GetGridPropertyDetails(int gridX, int gridY,Dictionary<string, GridPropertyDetails> gridPropertyDictionary)
    {
        // Construct key from coordinate根据坐标构造键值
        string key = "x" + gridX + "y" + gridY;

        GridPropertyDetails gridPropertyDetails;

        // Check if grid property details exist for coordinate and retrieve检查坐标是否存在网格属性详情并获取
        if (!gridPropertyDictionary.TryGetValue(key, out gridPropertyDetails))
        {
            // if not found如果没有
            return null;
        }
        else
        {
            return gridPropertyDetails;
        }
    }

    //// 查询全局字典
    public GridPropertyDetails GetGridPropertyDetails(int gridX, int gridY)
    {
        return GetGridPropertyDetails(gridX, gridY, gridPropertyDictionary);
    }


    //保存系统接口实现
    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }



    public void ISaveableRestoreScene(string sceneName)
    {
        // Get sceneSave for scene - it exists since we created it in initialise
        //获取场景保存数据 - 该数据自初始化创建以来一直存在
        if (GameObjectSave.sceneData.TryGetValue(sceneName, out SceneSave sceneSave))
        {
            // get grid property details dictionary - it exists since we created it in initialise
            //获取网格属性详细信息字典——它存在于初始化阶段创建之时
            if (sceneSave.gridPropertyDetailsDictionary != null)
            {
                gridPropertyDictionary = sceneSave.gridPropertyDetailsDictionary;
            }
        }
    }

    public void ISaveableStoreScene(string sceneName)
    {
        // Remove sceneSave for scene删除场景为场景保存（移除该场景的旧存档）
        GameObjectSave.sceneData.Remove(sceneName);

        // Create sceneSave for scene创建场景为场景保存
        SceneSave sceneSave = new SceneSave();

        // create & add dict grid property details dictionary创建并添加字典网格属性详细信息字典
        sceneSave.gridPropertyDetailsDictionary = gridPropertyDictionary;

        // Add scene save to game object scene data将场景保存添加到游戏对象场景数据中
        GameObjectSave.sceneData.Add(sceneName, sceneSave);
    }

    // Set the grid property details to gridPropertyDetails fro the tile at (gridX, gridY) for current scene
    //将当前场景中坐标为(gridX, gridY)的图块的网格属性详情设置为gridPropertyDetails
    public void SetGridPropertyDetails(int gridX, int gridY, GridPropertyDetails gridPropertyDetails)
    {
        SetGridPropertyDetails(gridX, gridY, gridPropertyDetails, gridPropertyDictionary);
    }


    // Set the grid property details to gridPropertyDetails for the title at (gridX, gridY) for the gridPropertyDictionary.
    //将网格属性详细信息设置为gridPropertyDetails，用于网格属性字典中坐标为(gridX, gridY)处的标题。
    public void SetGridPropertyDetails(int gridX, int gridY, GridPropertyDetails gridPropertyDetails, Dictionary<string, GridPropertyDetails> gridPropertyDictionary)
    {
        // Construct key from coordinate根据坐标构建键
        string key = "x" + gridX + "y" + gridY;

        gridPropertyDetails.gridX = gridX;
        gridPropertyDetails.gridY = gridY;

        // Set value 
        gridPropertyDictionary[key] = gridPropertyDetails;
    }

    //1. 核心功能
    //网格属性管理：通过Dictionary<string, GridPropertyDetails>存储场景网格的物理属性（如可挖掘性、物品放置权限等）。
    //数据持久化：实现 ISaveable 接口，支持场景属性的存档（ISaveableStoreScene）与读档（ISaveableRestoreScene）。
    //动态属性修改：提供SetGridPropertyDetails方法实时更新网格状态（如玩家交互后禁用挖掘）。
    //2. 关键实现逻辑
    //初始化流程：
    //Awake：生成唯一ID并初始化存档容器（GameObjectSave）。
    //Start：加载脚本化对象（SO_GridProperties）配置的初始网格属性。
    //属性映射：
    //通过switch将GridBoolProperty枚举值（如diggable、canPlaceFurniture）转换为GridPropertyDetails对象的布尔属性。
    //场景适配：
    //根据场景名称（sceneName）隔离不同场景的网格数据，避免冲突。
    //3. 主要方法
    //GetGridPropertyDetails	通过坐标（如"x3y5"）查询网格属性
    //SetGridPropertyDetails	修改指定坐标的网格属性（支持自定义字典）
    //ISaveableRegister/Deregister	注册/注销到全局存档系统
}
