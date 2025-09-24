using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GenerateGUID))]
public class SceneItemsManager : SingletonMonobehaviour<SceneItemsManager>, ISaveable
{
    private Transform parentItem;
    [SerializeField] private GameObject itemPrefab = null;

    private string _iSaveableUniqueID;
    public string ISaveableUniqueID { get { return _iSaveableUniqueID; } set { _iSaveableUniqueID = value; } }// ʵ��ISaveable�ӿڵ�ΨһID����

    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get { return _gameObjectSave; } set { _gameObjectSave = value; } }// ʵ��ISaveable�ӿڵĴ浵��������

    private void AfterSceneLoad()
    {
        parentItem = GameObject.FindGameObjectWithTag(Tags.ItemsParentTransform).transform;// ͨ����ǩ������Ʒ���ڵ㲢��������
    }

    protected override void Awake()
    {
        base.Awake();

        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;// ��GUID�����ȡΨһ��ʶ��
        GameObjectSave = new GameObjectSave();
    }

    // Destroy items currently in the scene���ٵ�ǰ�����е���Ʒ
    private void DestroySceneItems()
    {
        // Get all items in the scene��ȡ����������Item���
        Item[] itemsInScene = GameObject.FindObjectsOfType<Item>();

        // Loop through all scene items and destroy them�������г�����Ʒ����������
        for (int i = itemsInScene.Length - 1; i > -1; i--)
        {
            Destroy(itemsInScene[i].gameObject);
        }
    }

    public void InstantiateSceneItem(int itemCode, Vector3 itemPosition)//ʵ��������������Ʒ
    {
        GameObject itemGameObject = Instantiate(itemPrefab, itemPosition, Quaternion.identity, parentItem);// ʵ����Ԥ���岢���ø�����
        Item item = itemGameObject.GetComponent<Item>();
        item.Init(itemCode);// ��ʼ����Ʒ����
    }

    private void InstantiateSceneItems(List<SceneItem> sceneItemList)// ����ʵ����������Ʒ
    {
        GameObject itemGameObject;

        foreach (SceneItem sceneItem in sceneItemList)// �����浵��Ʒ�б�
        {
            itemGameObject = Instantiate(itemPrefab, new Vector3(sceneItem.position.x, sceneItem.position.y, sceneItem.position.z), Quaternion.identity, parentItem);// ���ݴ浵����ʵ������Ʒ

            Item item = itemGameObject.GetComponent<Item>();
            item.ItemCode = sceneItem.itemCode;
            item.name = sceneItem.itemName;
        }
    }

    private void OnDisable()
    {
        ISaveableDeregister();// �Ӵ浵ϵͳע��
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
    }

    private void OnEnable()
    {
        ISaveableRegister();// ��浵ϵͳע��
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
    }

    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);// �Ӵ浵�������Ƴ���ǰ����
    }

    public void ISaveableRestoreScene(string sceneName)
    {
        if (GameObjectSave.sceneData.TryGetValue(sceneName, out SceneSave sceneSave))// ���Ի�ȡָ�������浵
        {
            if (sceneSave.listSceneItemDictionary != null && sceneSave.listSceneItemDictionary.TryGetValue("sceneItemList", out List<SceneItem> sceneItemList))// ��鲢��ȡ��Ʒ�б�����
            {
                // scene list items found - destroy existing items in scene����������Ʒ
                DestroySceneItems();

                // new instantiate the list of scene items���ݴ浵�ָ���Ʒ
                InstantiateSceneItems(sceneItemList);
            }
        }
    }

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);// ��浵������ע�ᵱǰ����
    }


    public void ISaveableStoreScene(string sceneName)
    {
        // Remove old scene save for gameObject if exists����ɳ�������
        GameObjectSave.sceneData.Remove(sceneName);

        // Get all items in the scene��ȡ�����е�������Ʒ
        List<SceneItem> sceneItemList = new List<SceneItem>(); //��������Ʒ�б�
        Item[] itemsInScene = FindObjectsOfType<Item>();// ��ȡ����������Ʒ

        // Loop through all scene items����������Ʒ
        foreach (Item item in itemsInScene)
        {
            SceneItem sceneItem = new SceneItem();// �����浵��Ʒ����
            sceneItem.itemCode = item.ItemCode;// ��¼��Ʒ����
            sceneItem.position = new Vector3Serializable(item.transform.position.x,item.transform.position.y,item.transform.position.z);// ���л���Ʒλ��
            sceneItem.itemName = item.name;// ��¼��Ʒ����

            // Add scene item to list��ӵ��浵�б�
            sceneItemList.Add(sceneItem);
        }

        // Create list scene items dictionary in scene save and add to it�ڳ�������ʱ����������Ŀ�ֵ䲢�������������
        SceneSave sceneSave = new SceneSave();// ���������浵����
        sceneSave.listSceneItemDictionary = new Dictionary<string, List<SceneItem>>();// ��ʼ���ֵ�����
        sceneSave.listSceneItemDictionary.Add("sceneItemList", sceneItemList);// �����Ʒ�б�����

        // Add scene save to gameobjectΪ��Ϸ������ӳ������湦��
        GameObjectSave.sceneData.Add(sceneName, sceneSave);// ���浽ȫ�ִ浵

    }
    //���Ĺ��ܸ���
    //�ýű�ʵ����Unity������Ʒ�����ֺ��Ĺ��ܣ�
    //1.��Ʒʵ��������ͨ��InstantiateSceneItem��InstantiateSceneItems������̬������Ʒ
    //2.����״̬�־û���ʵ��ISaveable�ӿ�ʵ�ֳ�����Ʒ�Ĵ浵/����
    //3.�������ڿ��ƣ�ͨ��OnEnable/OnDisable������󼤻�״̬

    //�ؼ��߼��ֲ����
    //1. ��ʼ���׶�
    //Awake()������ΨһGUID��ʶ����ʼ���浵����
    //AfterSceneLoad()����λ������Ʒ���ڵ�
    //2. ��Ʒ����
    //��̬���ɣ�
    //����Ʒ��InstantiateSceneItem(itemCode, position)
    //�������ɣ�InstantiateSceneItems(List)
    //������ƣ�DestroySceneItems()���ٵ�ǰ����������Ʒ
    //3. �浵ϵͳʵ��
    //ע����ƣ�ISaveableRegister()���������浵������
    //�������棺
    //���л�����Item��λ�ú�����
    //�洢ΪSceneItem�����ֵ�
    //��sceneName����洢
    //�����ָ���
    //��ȡ�浵����
    //������������Ʒ
    //���浵�����ؽ�����
    //4. �������ڹ���
    //�������
    //OnEnable()��ע��浵ϵͳ+���ĳ��������¼�
    //OnDisable()����ע��+ȡ���¼�����
    //�̰߳�ȫ�����õ���������Ʒ��ѭ��ģʽ
}