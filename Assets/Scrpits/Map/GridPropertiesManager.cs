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
        ISaveableRegister();// ע�ᵽ�浵ϵͳ

        EventHandler.AfterSceneLoadEvent += AfterSceneLoaded;
        // ˮ������Զ���ڵ��ϣ�ÿ����Ҫ����Tile
        EventHandler.AdvanceGameDayEvent += AdvanceDay;
    }

    private void OnDisable()
    {
        ISaveableDeregister();// �Ӵ浵ϵͳע��

        EventHandler.AfterSceneLoadEvent -= AfterSceneLoaded;

        EventHandler.AdvanceGameDayEvent -= AdvanceDay;
    }

    private void Start()
    {
        InitialiseGridProperties();
    }

    private void ClearDisplayGroundDecorations()
    {
        // Remove ground decorations�Ƴ�����װ����
        groundDecoration1.ClearAllTiles();
        groundDecoration2.ClearAllTiles();
    }


    private void ClearDisplayAllPlantedCrops()
    {
        // Destory all crops in scene�ݻٳ����е���������
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
        //Dug��
        if (gridPropertyDetails.daysSinceDug > -1)//�����ǰ�ؿ���ھ���������-1����ʾ���ھ�
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
        // Select tile based on surrounding dug tiles���õؿ���Χ4����������ڵؿ��Ƿ�Ҳ���ھ򣬴Ӷ�����ʹ������������ʽ����Ƭ����16�ֿ�����ϣ�
        Tile dugTile0 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
        groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0), dugTile0);//����һ����ȡ����Ƭ����Ӧ�õ�ָ������λ��


        // Set 4 tiles if dug surrounding current tile - up, down, left, right now that this central tile has been dug
        //����ǰ�����ש�ѱ��ھ���������Χ���ϡ��¡����ҷ����ھ�4���ש��
        GridPropertyDetails adjacentGridPropertyDetails;

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);//��ȡ�Ϸ��ؿ�����
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)//����Ƿ���Ч�����ھ�
        {
            Tile dugTile1 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);//���ö�Ӧ��Ƭ
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1, 0), dugTile1);//Ӧ����Ƭ �� groundDecoration1.SetTile(����, ��Ƭ)
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);//��
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile2 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1, 0), dugTile2);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);//��
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile3 = SetDugTile(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY, 0), dugTile3);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);//��
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
        // Get whether surrounding tiles(up,down,left,right) are dug or not����ĸ��������ڵؿ��Ƿ����ھ�
        bool upDug = IsGridSquareDug(xGrid, yGrid + 1);
        bool downDug = IsGridSquareDug(xGrid, yGrid - 1);
        bool leftDug = IsGridSquareDug(xGrid - 1, yGrid);
        bool rightDug = IsGridSquareDug(xGrid + 1, yGrid);

        // ������Χ�ؿ���ھ�״̬ѡ����Ӧ����Ƭ
        #region Set appropriate tile based on whether surrounding tiles are dug or not
        // ���з���δ�ھ�����
        if (!upDug && !downDug && !rightDug && !leftDug)
        {
            return dugGround[0];
        }
        // ���ҷ������ھ�������δ�ھ�����
        else if (!upDug && downDug && rightDug && !leftDug)
        {
            return dugGround[1];
        }
        // ���ҷ������ھ��Ϸ���δ�ھ�����
        else if (!upDug && downDug && rightDug && leftDug)
        {
            return dugGround[2];
        }
        // ���������ھ����ҷ���δ�ھ�����
        else if (!upDug && downDug && !rightDug && leftDug)
        {
            return dugGround[3];
        }
        // �·������ھ���������δ�ھ�����
        else if (!upDug && downDug && !rightDug && !leftDug)
        {
            return dugGround[4];
        }
        // �����ҷ������ھ�����δ�ھ�����
        else if (upDug && downDug && rightDug && !leftDug)
        {
            return dugGround[5];
        }
        // �����ҷ������ھ��������ھ�����
        else if (upDug && downDug && rightDug && leftDug)
        {
            return dugGround[6];
        }
        // �����������ھ��ҷ���δ�ھ�����
        else if (upDug && downDug && !rightDug && leftDug)
        {
            return dugGround[7];
        }
        // ���·������ھ����ҷ���δ�ھ�����
        else if (upDug && downDug && !rightDug && !leftDug)
        {
            return dugGround[8];
        }
        // ���ҷ������ھ�������δ�ھ�����
        else if (upDug && !downDug && rightDug && !leftDug)
        {
            return dugGround[9];
        }
        // ���ҷ������ھ��·���δ�ھ�����
        else if (upDug && !downDug && rightDug && leftDug)
        {
            return dugGround[10];
        }
        // ���������ھ����ҷ���δ�ھ�����
        else if (upDug && !downDug && !rightDug && leftDug)
        {
            return dugGround[11];
        }
        // �Ϸ������ھ���������δ�ھ�����
        else if (upDug && !downDug && !rightDug && !leftDug)
        {
            return dugGround[12];
        }
        // �ҷ������ھ���������δ�ھ�����
        else if (!upDug && !downDug && rightDug && !leftDug)
        {
            return dugGround[13];
        }
        // �ҷ������ھ�����δ�ھ�����
        else if (!upDug && !downDug && rightDug && leftDug)
        {
            return dugGround[14];
        }
        // �������ھ���������δ�ھ�����
        else if (!upDug && !downDug && !rightDug && leftDug)
        {
            return dugGround[15];
        }
        // Ĭ������������ϲ���ִ�е����
        return null;

        #endregion Set appropriate tile based on whether surrounding tiles are dug or not

    }

    private bool IsGridSquareDug(int xGrid, int yGrid)
    {
        GridPropertyDetails gridPropertyDetails = GetGridPropertyDetails(xGrid, yGrid);//��ȡ�ؿ�����

        if (gridPropertyDetails == null)
        {
            return false;
        }
        else if (gridPropertyDetails.daysSinceDug > -1)//��Ч�ؿ鷵��daysSinceDug > -1
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
        // Get whether surrounding tiles(up,down,left,right) are watered or not��ȡ��Χ���飨�Ϸ����·�����ࡢ�Ҳࣩ�Ƿ񱻽���
        bool upWatered = IsGridSquareWatered(xGrid, yGrid + 1);
        bool downWatered = IsGridSquareWatered(xGrid, yGrid - 1);
        bool leftWatered = IsGridSquareWatered(xGrid - 1, yGrid);
        bool rightWatered = IsGridSquareWatered(xGrid + 1, yGrid);

        #region Set appropriate tile based on whether surrounding tiles are watered or not������Χ��ש�Ƿ�ˮ������Ӧ�Ĵ�ש
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
        // Loop throught all grid items��������������
        foreach (KeyValuePair<string, GridPropertyDetails> item in gridPropertyDictionary)// �����ֵ�������������
        {
            GridPropertyDetails gridPropertyDetails = item.Value;// ��ȡ��ǰ����������������

            DisplayDugGround(gridPropertyDetails);// ������ʾ��������ǰ����

            DisplayWateredGround(gridPropertyDetails);

            DisplayPlantedCrop(gridPropertyDetails);
        }
    }

    public void DisplayPlantedCrop(GridPropertyDetails gridPropertyDetails)
    {
        //// ���������Ƿ���ֲ�����seedItemCode > -1��ʾ����ֲ��
        if (gridPropertyDetails.seedItemCode > -1)
        {
            // get crop details����������Ʒ�����ȡ������ϸ��Ϣ
            CropDetails cropDetails = so_CropDetailList.GetCropDetails(gridPropertyDetails.seedItemCode);

            if (cropDetails != null)
            {

                // prefab to use��������Ԥ�������
                GameObject cropPrefab;

                // ��ȡ����������׶�����������growthDays���鳤�ȣ�
                int growthStages = cropDetails.growthDays.Length;
                // ��ʼ����ǰ�����׶κ�����������
                int currentGrowthStage = 0;
                int daysCounter = cropDetails.totalGrowthDays;

                //����������׶ο�ʼ���������ȷ����ǰ�����������׶�
                for (int i = growthStages - 1; i >= 0; i--)
                {
                    // �����ǰ�����������ڵ����ۼ�������ֵ
                    if (gridPropertyDetails.growthDays >= daysCounter)
                    {
                        currentGrowthStage = i;// ���õ�ǰ�����׶�
                        break;// �ҵ�������ѭ��
                    }

                    // �����������������������ǰһ�������׶�
                    daysCounter = daysCounter - cropDetails.growthDays[i];
                }

                //ʾ������
                //����������3�������׶Σ������ֲ�Ϊ[2, 3, 5]��������=10����
                //Case 1: growthDays = 4
                //i=2: 4 >= 10? �� �� �� daysCounter = 10 - 5 = 5
                //i=1: 4 >= 5? �� �� �� daysCounter = 5 - 3 = 2
                //i=0: 4 >= 2? �� �� �� �׶�0��δ��ȫ����׶�1����
                //Case 2: growthDays = 7
                //i=2: 7 >= 10? �� �� �� daysCounter = 5
                //i=1: 7 >= 5? �� �� �� �׶�1��

                // ��ȡ��ǰ�����׶ζ�Ӧ��Ԥ����
                cropPrefab = cropDetails.growthPrefab[currentGrowthStage];

                // ��ȡ��ǰ�����׶ζ�Ӧ�ľ���ͼƬ
                Sprite growthSprite = cropDetails.growthSprite[currentGrowthStage];

                // ����������ת��Ϊ��������
                Vector3 worldPosition = groundDecoration2.CellToWorld(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0));

                // ������������λ�ã�����ƫ�ư����Ԫ���С��
                worldPosition = new Vector3(worldPosition.x + Settings.gridCellSize / 2, worldPosition.y, worldPosition.z);

                //instantiate crop prefab at grid location�ڵ������λ��ʵ��������Ԥ����
                GameObject cropInstance = Instantiate(cropPrefab, worldPosition, Quaternion.identity);

                // ��������ʵ���ľ���ͼƬ
                cropInstance.GetComponentInChildren<SpriteRenderer>().sprite = growthSprite;
                // ��������ʵ���ĸ�����
                cropInstance.transform.SetParent(cropParentTransform);
                // ��������ʵ��������λ��
                cropInstance.GetComponent<Crop>().cropGridPosition = new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY);

            }

        }

    }

        //���Ĺ������̣�
        //1.DisplayDugGround����ڷ�����
        //���� ���ؿ��Ƿ��ھ�daysSinceDug > -1��
        //���� ���� ConnectDugGround �����߼�

        //2.�ؿ����Ӵ���ConnectDugGround������
        //���� ��ǰ�ؿ鴦��
        //�� ���� ͨ��SetDugTile��ȡ������Ƭ
        //�� ���� Ӧ����Ƭ��Tilemap
        //���� �����ڽӼ�飨��/��/��/�ң�
        //���� ��ȡ���ڵؿ����ԣ�GetGridPropertyDetails��
        //���� ��֤��Ч�Լ��ھ�״̬��IsGridSquareDug��
        //���� ��̬����������Ƭ��SetDugTile��

        //3.��Ƭ�����߼���SetDugTile�ڲ���
        //���� �����ڽӼ�⣨IsGridSquareDug��
        //�� ���� �յؿ鷵��false
        //�� ���� ��Ч�ؿ鷵��daysSinceDug > -1
        //���� 16����������ж�
        //���� �����ڽ�״̬ѡ���Ӧ��Ƭ
        //���� ����dugGround�����е�Ԥ����Ƭ

        //4.��������
        //���Լ�� �� daysSinceDug״̬ �� ��Ƭѡ�� �� �Ӿ���Ⱦ


        // This initialises the grid property dictionary with the values from the SO_GridProperties assets and stores the values for each scene in GameObjectSave sceneData
        //�˲��������������ֵ��ʼ��ΪSO_GridProperties��Դ�е�ֵ������ÿ��������ֵ�洢��GameObjectSave���������С�
        private void InitialiseGridProperties()
    {
        // Loop through all gridproperties in the array����������������
        foreach (SO_GridProperties so_GridProperties in so_gridPropertiesArray)
        {
            // Create dictionary of grid property details��������������ϸ��Ϣ�ֵ�
            Dictionary<string, GridPropertyDetails> gridPropertyDictionary = new Dictionary<string, GridPropertyDetails>();

            // Populate grid property dictionary - Iterate through all the grid properties in the so gridproperties list
            //������������ֵ� - ����so gridproperties�б��е�������������
            foreach (GridProperty gridProperty in so_GridProperties.gridPropertyList)
            {
                GridPropertyDetails gridPropertyDetails;

                gridPropertyDetails = GetGridPropertyDetails(gridProperty.gridCoordinate.x, gridProperty.gridCoordinate.y, gridPropertyDictionary);

                if (gridPropertyDetails == null)
                {
                    gridPropertyDetails = new GridPropertyDetails();
                }
                // �������ֲ�������
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
                // �������ݱ���
                SetGridPropertyDetails(gridProperty.gridCoordinate.x, gridProperty.gridCoordinate.y, gridPropertyDetails, gridPropertyDictionary);

            }

            // Create scene save for this gameobjectΪ����Ϸ���󴴽���������
            SceneSave sceneSave = new SceneSave();

            // Add grid property dictionary to scene save data�����������ֵ���ӵ���������������
            sceneSave.gridPropertyDetailsDictionary = gridPropertyDictionary;

            // If starting scene set the griProertyDictionary member variable to the current iteration
            //// �������ʼ��������Ϊȫ���ֵ�
            if (so_GridProperties.sceneName.ToString() == SceneControllerManager.Instance.startingSceneName.ToString())
            {
                this.gridPropertyDictionary = gridPropertyDictionary;
            }

            // Add scene save to game object scene data������������ӵ���Ϸ���󳡾�������
            GameObjectSave.sceneData.Add(so_GridProperties.sceneName.ToString(), sceneSave);
        }
    }


    private void AfterSceneLoaded() // ��ȡ�������
    {
        if (GameObject.FindGameObjectWithTag(Tags.CropsParentTransform) != null)
        {
            cropParentTransform = GameObject.FindGameObjectWithTag(Tags.CropsParentTransform).transform;
        }
        else
        {
            cropParentTransform = null;

        }

            // Get Grid��ȡ����
            grid = GameObject.FindObjectOfType<Grid>();

        // Get tilemaps��ȡ��Ƭ��ͼ
        groundDecoration1 = GameObject.FindGameObjectWithTag(Tags.GroundDecoration1).GetComponent<Tilemap>();
        groundDecoration2 = GameObject.FindGameObjectWithTag(Tags.GroundDecoration2).GetComponent<Tilemap>();

    }


    //���Բ�ѯ����

    // Returns the gridPropertyDetails at the gridlocation fro the supplied dictionary,or null if no properties exist at that location
    //����ָ���ֵ�������λ�õ������������飬����λ�ò����������򷵻�null��
    public GridPropertyDetails GetGridPropertyDetails(int gridX, int gridY,Dictionary<string, GridPropertyDetails> gridPropertyDictionary)
    {
        // Construct key from coordinate�������깹���ֵ
        string key = "x" + gridX + "y" + gridY;

        GridPropertyDetails gridPropertyDetails;

        // Check if grid property details exist for coordinate and retrieve��������Ƿ���������������鲢��ȡ
        if (!gridPropertyDictionary.TryGetValue(key, out gridPropertyDetails))
        {
            // if not found���û��
            return null;
        }
        else
        {
            return gridPropertyDetails;
        }
    }

    // ͨ���������������е��������꣬�����Ӧ����Ԫ����������ռ��е���ά����
    // gridPropertyDetails.gridX �� gridPropertyDetails.gridY ��ʾ����ĺ�������
    public Crop GetCropObjectAtGridLocation(GridPropertyDetails gridPropertyDetails)
    {
        // �ڼ���������������������2D��ײ�������ظ�λ�õ�������ײ������
        // OverlapPointAll����������ָ�����ص�����ײ��
        Vector3 worldPosition = grid.GetCellCenterWorld(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0));
        Collider2D[] collider2DArray = Physics2D.OverlapPointAll(worldPosition);

        // Loop through colliders to get crop game object������ײ���Ի�ȡ������Ϸ����
        Crop crop = null;

        for (int i = 0; i < collider2DArray.Length; i++)
        {
            // ���ȳ��Դ���ײ��������Ϸ����ĸ�������л�ȡCrop���
            crop = collider2DArray[i].gameObject.GetComponentInParent<Crop>();
            // ����ɹ���ȡ��Crop��������Ҹ����������λ����Ŀ������λ��ƥ�䣬�����ѭ��
            if (crop != null && crop.cropGridPosition == new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY))
                break;
            // �������δ�ҵ�ƥ���������Դ��Ӽ�����л�ȡCrop���
            crop = collider2DArray[i].gameObject.GetComponentInChildren<Crop>();
            if (crop != null && crop.cropGridPosition == new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY))
                break;
        }
        // �����ҵ�������������δ�ҵ��򷵻�null��
        return crop;
    }

    // Returns Crop Details for the provided seedItemCode����ָ��������Ŀ�������������
    public CropDetails GetCropDetails(int seedItemCode)
    {
        return so_CropDetailList.GetCropDetails(seedItemCode);
    }

    //// ��ѯȫ���ֵ�
    public GridPropertyDetails GetGridPropertyDetails(int gridX, int gridY)
    {
        return GetGridPropertyDetails(gridX, gridY, gridPropertyDictionary);
    }


    //����ϵͳ�ӿ�ʵ��
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
        //��ȡ������������ - �������Գ�ʼ����������һֱ����
        if (GameObjectSave.sceneData.TryGetValue(sceneName, out SceneSave sceneSave))
        {
            // get grid property details dictionary - it exists since we created it in initialise
            //��ȡ����������ϸ��Ϣ�ֵ䡪���������ڳ�ʼ���׶δ���֮ʱ
            if (sceneSave.gridPropertyDetailsDictionary != null)
            {
                gridPropertyDictionary = sceneSave.gridPropertyDetailsDictionary;
            }

            // If grid properties exist����������Դ���
            if (gridPropertyDictionary.Count > 0)
            {
                // grid property details found for the current scene destroy existing ground decoration��ǰ�������ҵ����������������������еĵ���װ��
                ClearDisplayGridPropertyDetails();

                // Instantiate grid property details for current sceneΪ��ǰ����ʵ����������������
                DisplayGridPropertyDetails();
            }


        }
    }

    public void ISaveableStoreScene(string sceneName)
    {
        // Remove sceneSave for sceneɾ������Ϊ�������棨�Ƴ��ó����ľɴ浵��
        GameObjectSave.sceneData.Remove(sceneName);

        // Create sceneSave for scene��������Ϊ��������
        SceneSave sceneSave = new SceneSave();

        // create & add dict grid property details dictionary����������ֵ�����������ϸ��Ϣ�ֵ�
        sceneSave.gridPropertyDetailsDictionary = gridPropertyDictionary;

        // Add scene save to game object scene data������������ӵ���Ϸ���󳡾�������
        GameObjectSave.sceneData.Add(sceneName, sceneSave);
    }

    // Set the grid property details to gridPropertyDetails fro the tile at (gridX, gridY) for current scene
    //����ǰ����������Ϊ(gridX, gridY)��ͼ�������������������ΪgridPropertyDetails
    public void SetGridPropertyDetails(int gridX, int gridY, GridPropertyDetails gridPropertyDetails)
    {
        SetGridPropertyDetails(gridX, gridY, gridPropertyDetails, gridPropertyDictionary);
    }


    // Set the grid property details to gridPropertyDetails for the title at (gridX, gridY) for the gridPropertyDictionary.
    //������������ϸ��Ϣ����ΪgridPropertyDetails���������������ֵ�������Ϊ(gridX, gridY)���ı��⡣
    public void SetGridPropertyDetails(int gridX, int gridY, GridPropertyDetails gridPropertyDetails, Dictionary<string, GridPropertyDetails> gridPropertyDictionary)
    {
        // Construct key from coordinate�������깹����
        string key = "x" + gridX + "y" + gridY;

        gridPropertyDetails.gridX = gridX;
        gridPropertyDetails.gridY = gridY;

        // Set value 
        gridPropertyDictionary[key] = gridPropertyDetails;
    }

    private void AdvanceDay(int gameYear, Season gameSeason, int gameDay, string gameDayyOfWeek, int gameHour, int gameMinute, int gameSecond)
    {
        // Clear Display All Grid Property Details�����ʾ����������������
        // �����ǰ��ʾ������������Ϣ��Ϊ������׼��
        ClearDisplayGridPropertyDetails();

        // loop through all scenes - by looping through all gridproperty in the array�������г��� - ͨ��ѭ�����������е�������������
        foreach (SO_GridProperties so_GridProperties in so_gridPropertiesArray)
        {
            // Get gridpropertydetails dictionary for scene��ȡ����������������ϸ��Ϣ�ֵ�
            // ���Դӳ��������л�ȡ��ǰ�����ı�����Ϣ
            if (GameObjectSave.sceneData.TryGetValue(so_GridProperties.sceneName.ToString(), out SceneSave sceneSave))
            {
                // ��������д���������������
                if (sceneSave.gridPropertyDetailsDictionary != null)
                {
                    // �Ӻ���ǰ�������������ֵ䣨������Ϊ�˰�ȫɾ�����ض�˳����
                    for (int i = sceneSave.gridPropertyDetailsDictionary.Count - 1; i >= 0; i--)
                    {
                        // ��ȡ��ǰ������i���ļ�ֵ��
                        KeyValuePair<string, GridPropertyDetails> item = sceneSave.gridPropertyDetailsDictionary.ElementAt(i);//ElementAt������LINQ����չ��������ͬʱ�����ֵ���ļ���ֵ�������㰴���������ֵ�����������

                        GridPropertyDetails gridPropertyDetails = item.Value;

                        #region Update all grid properties to reflect the advance in the day�����������������Է�ӳ���յĽ�չ

                        // if a crop is planted�����ֲ���������������+1��������
                        if (gridPropertyDetails.growthDays > -1)
                        {
                            gridPropertyDetails.growthDays += 1;
                        }


                        // if ground is watered, then clear water������潽��ˮ����Ϊ���ˮ
                        // ��������������н�ˮ�����¼������-1��
                        if (gridPropertyDetails.daysSinceWatered > -1)
                        {
                            // ���ٽ�ˮ���������ģ��ʱ���ƽ���
                            gridPropertyDetails.daysSinceWatered -= 1;
                        }

                        // Set gridpropertydetails��������������ϸ��Ϣ
                        SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails, sceneSave.gridPropertyDetailsDictionary);

                        #endregion Update all grid properties to reflect the advance in the day
                    }
                }
            }
        }

        // Display grid property details to reflect changed values��ʾ����������ϸ��Ϣ�Է�ӳ���ĺ��ֵ
        // ˢ����ʾ�Է�ӳ���º����������״̬
        DisplayGridPropertyDetails();

    }

    //1. ���Ĺ���
    //�������Թ���ͨ��Dictionary<string, GridPropertyDetails>�洢����������������ԣ�����ھ��ԡ���Ʒ����Ȩ�޵ȣ���
    //���ݳ־û���ʵ�� ISaveable �ӿڣ�֧�ֳ������ԵĴ浵��ISaveableStoreScene���������ISaveableRestoreScene����
    //��̬�����޸ģ��ṩSetGridPropertyDetails����ʵʱ��������״̬������ҽ���������ھ򣩡�
    //2. �ؼ�ʵ���߼�
    //��ʼ�����̣�
    //Awake������ΨһID����ʼ���浵������GameObjectSave����
    //Start�����ؽű�������SO_GridProperties�����õĳ�ʼ�������ԡ�
    //����ӳ�䣺
    //ͨ��switch��GridBoolPropertyö��ֵ����diggable��canPlaceFurniture��ת��ΪGridPropertyDetails����Ĳ������ԡ�
    //�������䣺
    //���ݳ������ƣ�sceneName�����벻ͬ�������������ݣ������ͻ��
    //3. ��Ҫ����
    //GetGridPropertyDetails	ͨ�����꣨��"x3y5"����ѯ��������
    //SetGridPropertyDetails	�޸�ָ��������������ԣ�֧���Զ����ֵ䣩
    //ISaveableRegister/Deregister	ע��/ע����ȫ�ִ浵ϵͳ
}
