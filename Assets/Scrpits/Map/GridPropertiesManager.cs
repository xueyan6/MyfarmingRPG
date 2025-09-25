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
        ISaveableRegister();// ע�ᵽ�浵ϵͳ

        EventHandler.AfterSceneLoadEvent += AfterSceneLoaded;
    }

    private void OnDisable()
    {
        ISaveableDeregister();// �Ӵ浵ϵͳע��

        EventHandler.AfterSceneLoadEvent -= AfterSceneLoaded;
    }

    private void Start()
    {
        InitialiseGridProperties();
    }


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
        // Get Grid
        grid = GameObject.FindObjectOfType<Grid>();
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
