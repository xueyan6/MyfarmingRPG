using System.Collections.Generic;

[System.Serializable]

public class SceneSave
{
    // string key is an identifier name we choose for this list字符串键是我们为该列表选择的标识符名称
    public Dictionary<string, List<SceneItem>> listSceneItemDictionary;
}

