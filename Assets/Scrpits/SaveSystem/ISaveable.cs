
public interface ISaveable
{
    string ISaveableUniqueID { get; set; }//每个可保存对象的唯一标识符

    GameObjectSave GameObjectSave { get; set; }//保存对象数据的容器

    void ISaveableRegister();//向保存系统注册当前对象

    void ISaveableDeregister();//从保存系统注销当前对象

    void ISaveableStoreScene(string sceneName);//将当前对象的状态数据序列化到指定场景的存档中

    void ISaveableRestoreScene(string sceneName);//从指定场景存档中读取并恢复对象状态

}
