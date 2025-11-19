using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoadManager : SingletonMonobehaviour<SaveLoadManager>
{

    public GameSave gameSave;
    public List<ISaveable> iSaveableObjectList;

    protected override void Awake()
    {
        base.Awake();

        iSaveableObjectList = new List<ISaveable>();
    }

    public void LoadDataFromFile()
    {
        BinaryFormatter bf = new BinaryFormatter(); // 创建二进制格式化器，用于将文件中的二进制数据反序列化为对象

        // 检查存档文件是否存在于设备的持久化数据路径中
        if (File.Exists(Application.persistentDataPath + "/WildHopeCreek.data"))
        {
            gameSave = new GameSave(); // 创建一个新的GameSave实例作为数据容器

            // 以只读模式打开存档文件，准备读取数据
            FileStream file = File.Open(Application.persistentDataPath + "/WildHopeCreek.data", FileMode.Open);

            // 关键步骤：将文件中的二进制数据反序列化为GameSave对象
            // 这个过程会重建整个存档数据结构，包括字典中的所有GameObjectSave
            gameSave = (GameSave)bf.Deserialize(file);

            // 逆向遍历所有已注册的可存档对象列表（从后往前遍历）
            // 这样处理可以在删除对象时不影响遍历过程
            for (int i = iSaveableObjectList.Count - 1; i > -1; i--)
            {
                // 检查当前存档数据中是否包含这个对象的唯一标识符
                if (gameSave.gameObjectData.ContainsKey(iSaveableObjectList[i].ISaveableUniqueID))
                {
                    // 如果找到匹配的存档数据，调用对象的加载方法
                    // 对象将根据其唯一ID从gameSave中提取自己的数据并恢复状态
                    iSaveableObjectList[i].ISaveableLoad(gameSave);
                }
                else
                {
                    // 如果在存档数据中找不到该对象的唯一ID，说明这个对象在存档中不存在
                    // 可能是新游戏中的对象，或者存档创建后添加的对象
                    Component component = (Component)iSaveableObjectList[i]; // 将接口转换为Unity组件
                    Destroy(component.gameObject); // 销毁该游戏对象，保持场景与存档一致
                }
            }

            file.Close(); // 关闭文件流，释放系统资源
        }

        // 存档加载完成后，禁用游戏中的暂停菜单界面
        UIManager.Instance.DisablePauseMenu();
    }

    public void SaveDataToFile()
    {
        gameSave = new GameSave(); // 创建一个新的空GameSave实例，准备收集所有对象的存档数据

        // 遍历所有已注册的可存档对象，逐个收集它们的存档数据
        foreach (ISaveable iSaveableObject in iSaveableObjectList)
        {
            // 将每个对象的唯一ID和其返回的存档数据添加到字典中
            // iSaveableObject.ISaveableSave() 调用每个对象自己的数据打包方法
            gameSave.gameObjectData.Add(iSaveableObject.ISaveableUniqueID, iSaveableObject.ISaveableSave());
        }

        BinaryFormatter bf = new BinaryFormatter(); // 创建二进制格式化器，用于序列化数据

        // 创建或覆盖存档文件，如果文件已存在则清空内容
        FileStream file = File.Open(Application.persistentDataPath + "/WildHopeCreek.data", FileMode.Create);

        // 关键步骤：将整个gameSave对象及其包含的所有数据序列化为二进制格式
        bf.Serialize(file, gameSave);

        file.Close(); // 关闭文件流，确保所有数据写入完成

        // 保存完成后，禁用游戏中的暂停菜单界面
        UIManager.Instance.DisablePauseMenu();
        Debug.Log("dataPath:" + Application.persistentDataPath);
    }
    //LoadDataFromFile 和 SaveDataToFile与ISaveableLoad 和 ISaveableSave   是不同层级但紧密配合的关系：
    //层级关系
    //ISaveableLoad/ISaveableSave（个体级别）
    //每个游戏对象自己实现的接口方法
    //负责：我的数据如何打包？我的数据如何恢复？
    //比如玩家对象知道要保存位置、血量、装备；宝箱对象知道要保存是否开启、里面物品

    //LoadDataFromFile/SaveDataToFile（系统级别）
    //存档管理器实现的全局方法
    //负责：所有对象数据如何存储到文件？如何从文件读取？

    //工作流程配合
    //保存时（SaveDataToFile调用ISaveableSave）
    //SaveDataToFile() 遍历所有对象 → 调用每个对象的 ISaveableSave() → 收集返回的 GameObjectSave → 序列化写入文件
    //加载时（LoadDataFromFile调用ISaveableLoad）
    //LoadDataFromFile() 读取文件 → 反序列化数据 → 找到匹配对象 → 调用对象的 ISaveableLoad()

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