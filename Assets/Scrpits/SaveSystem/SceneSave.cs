using System.Collections.Generic;

[System.Serializable]

public class SceneSave
{
    // string key is an identifier name we choose for this list�ַ�����������Ϊ���б�ѡ��ı�ʶ������
    public List<SceneItem> listSceneItem;
    public Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary; // key��������Ϣ��value�ǵ���������Ϣ
}

