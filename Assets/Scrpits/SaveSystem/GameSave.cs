using System.Collections.Generic;

[System.Serializable]
public class GameSave
{
    // string key - GUID gameobject ID 字符串键 - GUID 游戏对象 ID
    public Dictionary<string, GameObjectSave> gameObjectData;//// 核心数据：一个字典，用于存储场景中所有需要保存的游戏对象数据，键是对象的唯一标识符，值是对应的存档数据
    //键（Key）- 对象的唯一标识符
    //这是一个字符串，用于唯一识别游戏世界中的某个特定对象
    //就像每个人的身份证号码，确保存档系统能精确找到对应的游戏对象
    //例如："Player_001"、"Enemy_Dragon_123"、"Chest_Treasure_456"

    //值（Value）- 对应的存档数据
    //这是一个 GameObjectSave 对象，包含了该游戏对象需要保存的所有状态信息
    //就像个人的档案袋，里面装着位置坐标、生命值、装备、任务进度等具体数据

    //工作流程举例：
    //当保存游戏时，系统会：
    //遍历所有需要保存的游戏对象
    //用每个对象的唯一ID作为键
    //将对象的状态数据打包成 GameObjectSave 作为值存入字典中

    //当加载游戏时，系统会：
    //读取存档字典
    //根据唯一ID找到对应的存档数据
    //将数据还原到场景中的对应对象上

    //这种设计确保了即使游戏中有成百上千个对象，存档系统也能高效地管理和恢复每个对象的精确状态。

    public GameSave()
    {
        gameObjectData = new Dictionary<string, GameObjectSave>();
    }
}