using System.Collections.Generic;

[System.Serializable]
public class SceneRoute
{
    //序列化字段
    public SceneName fromSceneName;//初始场景
    public SceneName toSceneName;//目标场景
    public List<ScenePath> scenePathList;//场景路线列表
}