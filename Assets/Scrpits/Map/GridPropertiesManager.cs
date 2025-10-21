using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(GenerateGUID))]
public class GridPropertiesManager : SingletonMonobehaviour<GridPropertiesManager>, ISaveable
{
    private Transform cropParentTransform;
    private Tilemap groundDecoration1;
    private Tilemap groundDecoration2;

    private Grid grid;

    private Dictionary<string, GridPropertyDetails> gridPropertyDictionary;

    [SerializeField] private SO_GridProperties[] so_gridPropertiesArray = null;

    [SerializeField] private SO_CropDetailsList so_CropDetailList = null;

    [SerializeField] private Tile[] dugGround = null;
    [SerializeField] private Tile[] wateredGround = null;

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
        // 水不会永远留在地上，每天需要重置Tile
        EventHandler.AdvanceGameDayEvent += AdvanceDay;
    }

    private void OnDisable()
    {
        ISaveableDeregister();// 从存档系统注销

        EventHandler.AfterSceneLoadEvent -= AfterSceneLoaded;

        EventHandler.AdvanceGameDayEvent -= AdvanceDay;
    }

    private void Start()
    {
        InitialiseGridProperties();
    }

    private void ClearDisplayGroundDecorations()
    {
        // Remove ground decorations移除地面装饰物
        groundDecoration1.ClearAllTiles();
        groundDecoration2.ClearAllTiles();
    }


    private void ClearDisplayAllPlantedCrops()
    {
        // Destory all crops in scene摧毁场景中的所有作物
        Crop[] cropArray;
        cropArray = FindObjectsOfType<Crop>();

        foreach (Crop crop in cropArray)
        {
            Destroy(crop.gameObject);
        }

    }

    private void ClearDisplayGridPropertyDetails()
    {
        ClearDisplayGroundDecorations();

        ClearDisplayAllPlantedCrops();
    }

    public void DisplayDugGround(GridPropertyDetails gridPropertyDetails)
    {
        //Dug挖
        if (gridPropertyDetails.daysSinceDug > -1)//如果当前地块的挖掘天数大于-1（表示已挖掘）
        {
            ConnectDugGround(gridPropertyDetails);
        }
    }

    public void DisplayWateredGround(GridPropertyDetails gridPropertyDetails)
    {
        // Watered
        if (gridPropertyDetails.daysSinceWatered > -1)
        {
            ConnectWateredGround(gridPropertyDetails);
        }
    }



    private void ConnectDugGround(GridPropertyDetails gridPropertyDetails)
    {
        // Select tile based on surrounding dug tiles检查该地块周围4个方向的相邻地块是否也被挖掘，从而决定使用哪种连接样式的瓦片（共16种可能组合）
        Tile dugTile0 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
        groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0), dugTile0);//将上一步获取的瓦片对象应用到指定坐标位置


        // Set 4 tiles if dug surrounding current tile - up, down, left, right now that this central tile has been dug
        //若当前中央瓷砖已被挖掘，则在其周围（上、下、左、右方向）挖掘4块瓷砖。
        GridPropertyDetails adjacentGridPropertyDetails;

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);//获取上方地块属性
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)//检查是否有效且已挖掘
        {
            Tile dugTile1 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);//设置对应瓦片
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1, 0), dugTile1);//应用瓦片 → groundDecoration1.SetTile(坐标, 瓦片)
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);//下
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile2 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1, 0), dugTile2);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);//左
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile3 = SetDugTile(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY, 0), dugTile3);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);//右
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile4 = SetDugTile(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY, 0), dugTile4);
        }
    }

    private void ConnectWateredGround(GridPropertyDetails gridPropertyDetails)
    {
        // Select tile based on surrounding watered tiles
        Tile wateredTile0 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
        groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0), wateredTile0);

        // Set 4 tiles if watered surrounding current tile - up, down, left, right now that this central tile has been watered
        GridPropertyDetails adjacentGridPropertyDetails;

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile1 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1, 0), wateredTile1);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile2 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1, 0), wateredTile2);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile3 = SetWateredTile(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY, 0), wateredTile3);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile4 = SetWateredTile(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY, 0), wateredTile4);
        }

    }

    private Tile SetDugTile(int xGrid, int yGrid)
    {
        // Get whether surrounding tiles(up,down,left,right) are dug or not检查四个方向相邻地块是否已挖掘
        bool upDug = IsGridSquareDug(xGrid, yGrid + 1);
        bool downDug = IsGridSquareDug(xGrid, yGrid - 1);
        bool leftDug = IsGridSquareDug(xGrid - 1, yGrid);
        bool rightDug = IsGridSquareDug(xGrid + 1, yGrid);

        // 根据周围地块的挖掘状态选择相应的瓦片
        #region Set appropriate tile based on whether surrounding tiles are dug or not
        // 所有方向都未挖掘的情况
        if (!upDug && !downDug && !rightDug && !leftDug)
        {
            return dugGround[0];
        }
        // 下右方向已挖掘，上左方向未挖掘的情况
        else if (!upDug && downDug && rightDug && !leftDug)
        {
            return dugGround[1];
        }
        // 下右方向已挖掘，上方向未挖掘的情况
        else if (!upDug && downDug && rightDug && leftDug)
        {
            return dugGround[2];
        }
        // 下左方向已挖掘，上右方向未挖掘的情况
        else if (!upDug && downDug && !rightDug && leftDug)
        {
            return dugGround[3];
        }
        // 下方向已挖掘，其他方向未挖掘的情况
        else if (!upDug && downDug && !rightDug && !leftDug)
        {
            return dugGround[4];
        }
        // 上下右方向已挖掘，左方向未挖掘的情况
        else if (upDug && downDug && rightDug && !leftDug)
        {
            return dugGround[5];
        }
        // 上下右方向已挖掘，左方向已挖掘的情况
        else if (upDug && downDug && rightDug && leftDug)
        {
            return dugGround[6];
        }
        // 上下左方向已挖掘，右方向未挖掘的情况
        else if (upDug && downDug && !rightDug && leftDug)
        {
            return dugGround[7];
        }
        // 上下方向已挖掘，左右方向未挖掘的情况
        else if (upDug && downDug && !rightDug && !leftDug)
        {
            return dugGround[8];
        }
        // 上右方向已挖掘，下左方向未挖掘的情况
        else if (upDug && !downDug && rightDug && !leftDug)
        {
            return dugGround[9];
        }
        // 上右方向已挖掘，下方向未挖掘的情况
        else if (upDug && !downDug && rightDug && leftDug)
        {
            return dugGround[10];
        }
        // 上左方向已挖掘，下右方向未挖掘的情况
        else if (upDug && !downDug && !rightDug && leftDug)
        {
            return dugGround[11];
        }
        // 上方向已挖掘，其他方向未挖掘的情况
        else if (upDug && !downDug && !rightDug && !leftDug)
        {
            return dugGround[12];
        }
        // 右方向已挖掘，其他方向未挖掘的情况
        else if (!upDug && !downDug && rightDug && !leftDug)
        {
            return dugGround[13];
        }
        // 右方向已挖掘，左方向未挖掘的情况
        else if (!upDug && !downDug && rightDug && leftDug)
        {
            return dugGround[14];
        }
        // 左方向已挖掘，其他方向未挖掘的情况
        else if (!upDug && !downDug && !rightDug && leftDug)
        {
            return dugGround[15];
        }
        // 默认情况（理论上不会执行到这里）
        return null;

        #endregion Set appropriate tile based on whether surrounding tiles are dug or not

    }

    private bool IsGridSquareDug(int xGrid, int yGrid)
    {
        GridPropertyDetails gridPropertyDetails = GetGridPropertyDetails(xGrid, yGrid);//获取地块属性

        if (gridPropertyDetails == null)
        {
            return false;
        }
        else if (gridPropertyDetails.daysSinceDug > -1)//有效地块返回daysSinceDug > -1
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private Tile SetWateredTile(int xGrid, int yGrid)
    {
        // Get whether surrounding tiles(up,down,left,right) are watered or not获取周围方块（上方、下方、左侧、右侧）是否被浇灌
        bool upWatered = IsGridSquareWatered(xGrid, yGrid + 1);
        bool downWatered = IsGridSquareWatered(xGrid, yGrid - 1);
        bool leftWatered = IsGridSquareWatered(xGrid - 1, yGrid);
        bool rightWatered = IsGridSquareWatered(xGrid + 1, yGrid);

        #region Set appropriate tile based on whether surrounding tiles are watered or not根据周围瓷砖是否浇水设置相应的瓷砖
        if (!upWatered && !downWatered && !rightWatered && !leftWatered)
        {
            return wateredGround[0];
        }
        else if (!upWatered && downWatered && rightWatered && !leftWatered)
        {
            return wateredGround[1];
        }
        else if (!upWatered && downWatered && rightWatered && leftWatered)
        {
            return wateredGround[2];
        }
        else if (!upWatered && downWatered && !rightWatered && leftWatered)
        {
            return wateredGround[3];
        }
        else if (!upWatered && downWatered && !rightWatered && !leftWatered)
        {
            return wateredGround[4];
        }
        else if (upWatered && downWatered && rightWatered && !leftWatered)
        {
            return wateredGround[5];
        }
        else if (upWatered && downWatered && rightWatered && leftWatered)
        {
            return wateredGround[6];
        }
        else if (upWatered && downWatered && !rightWatered && leftWatered)
        {
            return wateredGround[7];
        }
        else if (upWatered && downWatered && !rightWatered && !leftWatered)
        {
            return wateredGround[8];
        }
        else if (upWatered && !downWatered && rightWatered && !leftWatered)
        {
            return wateredGround[9];
        }
        else if (upWatered && !downWatered && rightWatered && leftWatered)
        {
            return wateredGround[10];
        }
        else if (upWatered && !downWatered && !rightWatered && leftWatered)
        {
            return wateredGround[11];
        }
        else if (upWatered && !downWatered && !rightWatered && !leftWatered)
        {
            return wateredGround[12];
        }
        else if (!upWatered && !downWatered && rightWatered && !leftWatered)
        {
            return wateredGround[13];
        }
        else if (!upWatered && !downWatered && rightWatered && leftWatered)
        {
            return wateredGround[14];
        }
        else if (!upWatered && !downWatered && !rightWatered && leftWatered)
        {
            return wateredGround[15];
        }

        return null;

        #endregion Set appropriate tile based on whether surrounding tiles are watered or not

    }
    private bool IsGridSquareWatered(int xGrid, int yGrid)
    {
        GridPropertyDetails gridPropertyDetails = GetGridPropertyDetails(xGrid, yGrid);

        if (gridPropertyDetails == null)
        {
            return false;
        }
        else if (gridPropertyDetails.daysSinceWatered > -1)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    private void DisplayGridPropertyDetails()
    {
        // Loop throught all grid items遍历所有网格项
        foreach (KeyValuePair<string, GridPropertyDetails> item in gridPropertyDictionary)// 遍历字典中所有网格项
        {
            GridPropertyDetails gridPropertyDetails = item.Value;// 获取当前网格的属性详情对象

            DisplayDugGround(gridPropertyDetails);// 调用显示方法处理当前网格

            DisplayWateredGround(gridPropertyDetails);

            DisplayPlantedCrop(gridPropertyDetails);
        }
    }

    public void DisplayPlantedCrop(GridPropertyDetails gridPropertyDetails)
    {
        //// 检查该网格是否种植了作物（seedItemCode > -1表示已种植）
        if (gridPropertyDetails.seedItemCode > -1)
        {
            // get crop details根据种子物品代码获取作物详细信息
            CropDetails cropDetails = so_CropDetailList.GetCropDetails(gridPropertyDetails.seedItemCode);

            if (cropDetails != null)
            {

                // prefab to use声明作物预制体变量
                GameObject cropPrefab;

                // 获取作物的生长阶段总数（根据growthDays数组长度）
                int growthStages = cropDetails.growthDays.Length;
                // 初始化当前生长阶段和天数计数器
                int currentGrowthStage = 0;
                int daysCounter = cropDetails.totalGrowthDays;

                //从最高生长阶段开始倒序遍历，确定当前所处的生长阶段
                for (int i = growthStages - 1; i >= 0; i--)
                {
                    // 如果当前生长天数大于等于累计天数阈值
                    if (gridPropertyDetails.growthDays >= daysCounter)
                    {
                        currentGrowthStage = i;// 设置当前生长阶段
                        break;// 找到后跳出循环
                    }

                    // 减少天数计数器，继续检查前一个生长阶段
                    daysCounter = daysCounter - cropDetails.growthDays[i];
                }

                //示例场景
                //假设作物有3个生长阶段，天数分布为[2, 3, 5]（总天数=10）：
                //Case 1: growthDays = 4
                //i=2: 4 >= 10? → 否 → daysCounter = 10 - 5 = 5
                //i=1: 4 >= 5? → 否 → daysCounter = 5 - 3 = 2
                //i=0: 4 >= 2? → 是 → 阶段0（未完全进入阶段1）。
                //Case 2: growthDays = 7
                //i=2: 7 >= 10? → 否 → daysCounter = 5
                //i=1: 7 >= 5? → 是 → 阶段1。

                // 获取当前生长阶段对应的预制体
                cropPrefab = cropDetails.growthPrefab[currentGrowthStage];

                // 获取当前生长阶段对应的精灵图片
                Sprite growthSprite = cropDetails.growthSprite[currentGrowthStage];

                // 将网格坐标转换为世界坐标
                Vector3 worldPosition = groundDecoration2.CellToWorld(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0));

                // 调整世界坐标位置（向右偏移半个单元格大小）
                worldPosition = new Vector3(worldPosition.x + Settings.gridCellSize / 2, worldPosition.y, worldPosition.z);

                //instantiate crop prefab at grid location在调整后的位置实例化作物预制体
                GameObject cropInstance = Instantiate(cropPrefab, worldPosition, Quaternion.identity);

                // 设置作物实例的精灵图片
                cropInstance.GetComponentInChildren<SpriteRenderer>().sprite = growthSprite;
                // 设置作物实例的父物体
                cropInstance.transform.SetParent(cropParentTransform);
                // 设置作物实例的网格位置
                cropInstance.GetComponent<Crop>().cropGridPosition = new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY);

            }

        }

    }

        //核心功能流程：
        //1.DisplayDugGround（入口方法）
        //├─ 检查地块是否被挖掘（daysSinceDug > -1）
        //└─ 触发 ConnectDugGround 连接逻辑

        //2.地块连接处理（ConnectDugGround方法）
        //├─ 当前地块处理
        //│ ├─ 通过SetDugTile获取适配瓦片
        //│ └─ 应用瓦片到Tilemap
        //└─ 四向邻接检查（上/下/左/右）
        //├─ 获取相邻地块属性（GetGridPropertyDetails）
        //├─ 验证有效性及挖掘状态（IsGridSquareDug）
        //└─ 动态设置连接瓦片（SetDugTile）

        //3.瓦片决策逻辑（SetDugTile内部）
        //├─ 四向邻接检测（IsGridSquareDug）
        //│ ├─ 空地块返回false
        //│ └─ 有效地块返回daysSinceDug > -1
        //└─ 16种连接组合判断
        //├─ 根据邻接状态选择对应瓦片
        //└─ 返回dugGround数组中的预设瓦片

        //4.数据流向
        //属性检测 → daysSinceDug状态 → 瓦片选择 → 视觉渲染


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
        if (GameObject.FindGameObjectWithTag(Tags.CropsParentTransform) != null)
        {
            cropParentTransform = GameObject.FindGameObjectWithTag(Tags.CropsParentTransform).transform;
        }
        else
        {
            cropParentTransform = null;

        }

            // Get Grid获取网格
            grid = GameObject.FindObjectOfType<Grid>();

        // Get tilemaps获取瓦片地图
        groundDecoration1 = GameObject.FindGameObjectWithTag(Tags.GroundDecoration1).GetComponent<Tilemap>();
        groundDecoration2 = GameObject.FindGameObjectWithTag(Tags.GroundDecoration2).GetComponent<Tilemap>();

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

    // 通过网格属性详情中的网格坐标，计算对应网格单元中心在世界空间中的三维坐标
    // gridPropertyDetails.gridX 和 gridPropertyDetails.gridY 表示网格的横纵坐标
    public Crop GetCropObjectAtGridLocation(GridPropertyDetails gridPropertyDetails)
    {
        // 在计算出的世界坐标点检测所有2D碰撞器，返回该位置的所有碰撞体数组
        // OverlapPointAll会检测所有与指定点重叠的碰撞器
        Vector3 worldPosition = grid.GetCellCenterWorld(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0));
        Collider2D[] collider2DArray = Physics2D.OverlapPointAll(worldPosition);

        // Loop through colliders to get crop game object遍历碰撞器以获取作物游戏对象
        Crop crop = null;

        for (int i = 0; i < collider2DArray.Length; i++)
        {
            // 首先尝试从碰撞器所属游戏对象的父级组件中获取Crop组件
            crop = collider2DArray[i].gameObject.GetComponentInParent<Crop>();
            // 如果成功获取到Crop组件，并且该作物的网格位置与目标网格位置匹配，则结束循环
            if (crop != null && crop.cropGridPosition == new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY))
                break;
            // 如果父级未找到匹配的作物，则尝试从子级组件中获取Crop组件
            crop = collider2DArray[i].gameObject.GetComponentInChildren<Crop>();
            if (crop != null && crop.cropGridPosition == new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY))
                break;
        }
        // 返回找到的作物对象（如果未找到则返回null）
        return crop;
    }

    // Returns Crop Details for the provided seedItemCode返回指定种子项目代码的作物详情
    public CropDetails GetCropDetails(int seedItemCode)
    {
        return so_CropDetailList.GetCropDetails(seedItemCode);
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

            // If grid properties exist如果网格属性存在
            if (gridPropertyDictionary.Count > 0)
            {
                // grid property details found for the current scene destroy existing ground decoration当前场景中找到的网格属性详情销毁现有的地面装饰
                ClearDisplayGridPropertyDetails();

                // Instantiate grid property details for current scene为当前场景实例化网格属性详情
                DisplayGridPropertyDetails();
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

    private void AdvanceDay(int gameYear, Season gameSeason, int gameDay, string gameDayyOfWeek, int gameHour, int gameMinute, int gameSecond)
    {
        // Clear Display All Grid Property Details清除显示所有网格属性详情
        // 清除当前显示的网格属性信息，为更新做准备
        ClearDisplayGridPropertyDetails();

        // loop through all scenes - by looping through all gridproperty in the array遍历所有场景 - 通过循环遍历数组中的所有网格属性
        foreach (SO_GridProperties so_GridProperties in so_gridPropertiesArray)
        {
            // Get gridpropertydetails dictionary for scene获取场景的网格属性详细信息字典
            // 尝试从场景数据中获取当前场景的保存信息
            if (GameObjectSave.sceneData.TryGetValue(so_GridProperties.sceneName.ToString(), out SceneSave sceneSave))
            {
                // 如果场景中存在网格属性数据
                if (sceneSave.gridPropertyDetailsDictionary != null)
                {
                    // 从后向前遍历网格属性字典（可能是为了安全删除或特定顺序处理）
                    for (int i = sceneSave.gridPropertyDetailsDictionary.Count - 1; i >= 0; i--)
                    {
                        // 获取当前索引（i）的键值对
                        KeyValuePair<string, GridPropertyDetails> item = sceneSave.gridPropertyDetailsDictionary.ElementAt(i);//ElementAt：来自LINQ的扩展方法，可同时访问字典项的键和值，可满足按索引遍历字典的特殊情况。

                        GridPropertyDetails gridPropertyDetails = item.Value;

                        #region Update all grid properties to reflect the advance in the day更新所有网格属性以反映当日的进展

                        // if a crop is planted如果种植了作物，则随着天数+1生长天数
                        if (gridPropertyDetails.growthDays > -1)
                        {
                            gridPropertyDetails.growthDays += 1;
                        }


                        // if ground is watered, then clear water如果地面浇过水，则为清除水
                        // 如果该网格属性有浇水间隔记录（大于-1）
                        if (gridPropertyDetails.daysSinceWatered > -1)
                        {
                            // 减少浇水间隔天数（模拟时间推进）
                            gridPropertyDetails.daysSinceWatered -= 1;
                        }

                        // Set gridpropertydetails设置网格属性详细信息
                        SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails, sceneSave.gridPropertyDetailsDictionary);

                        #endregion Update all grid properties to reflect the advance in the day
                    }
                }
            }
        }

        // Display grid property details to reflect changed values显示网格属性详细信息以反映更改后的值
        // 刷新显示以反映更新后的网格属性状态
        DisplayGridPropertyDetails();

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
