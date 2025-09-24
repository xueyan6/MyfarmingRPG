using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SaveLoadManager : SingletonMonobehaviour<SaveLoadManager>
{
    public List<ISaveable> iSaveableObjectList;

    protected override void Awake()
    {
        base.Awake();

        iSaveableObjectList = new List<ISaveable>();
    }

    public void StoreCurrentSceneData()
    {
        // loop through all ISaveable objects and trigger store scene data for each�������пɱ�����󣬲�Ϊÿ�����󴥷��洢��������
        foreach (ISaveable iSaveableObject in iSaveableObjectList)
        {
            // �����е����ݶ��洢�ڵ�ǰ��������
            iSaveableObject.ISaveableStoreScene(SceneManager.GetActiveScene().name);
        }
    }

    public void RestoreCurrentSceneData()
    {
        // loop through all ISaveble objects and trigger restore scene data for each�������пɱ�����󣬲�Ϊÿ�����󴥷��������ݻָ�
        foreach (ISaveable iSaveableObject in iSaveableObjectList)
        {
            // ���ݵ�ǰ�������ָ�����
            iSaveableObject.ISaveableRestoreScene(SceneManager.GetActiveScene().name);
        }
    }


}