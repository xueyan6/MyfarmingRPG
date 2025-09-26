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
        // loop through all ISaveable objects and trigger store scene data for each遍历所有可保存对象，并为每个对象触发存储场景数据
        foreach (ISaveable iSaveableObject in iSaveableObjectList)
        {
            // 将所有的数据都存储在当前场景名下
            iSaveableObject.ISaveableStoreScene(SceneManager.GetActiveScene().name);
        }
    }

    public void RestoreCurrentSceneData()
    {
        // loop through all ISaveble objects and trigger restore scene data for each遍历所有可保存对象，并为每个对象触发场景数据恢复
        foreach (ISaveable iSaveableObject in iSaveableObjectList)
        {
            // 根据当前场景名恢复数据
            iSaveableObject.ISaveableRestoreScene(SceneManager.GetActiveScene().name);
        }
    }


}