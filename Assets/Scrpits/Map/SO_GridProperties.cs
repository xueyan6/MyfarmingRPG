using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "so_GridProperties", menuName = "Scriptable Objects/Grid Properties")]
public class SO_GridProperties : ScriptableObject
{
    public SceneName sceneName;//��ʶ�����ĳ��������Զ���SceneNameö�����ͣ�
    public int gridWidth;
    public int gridHeight;//һ��������Tilemap�ĳ���
    public int originX;
    public int originY;//��tilemap���½ǵ����ꡣ

    [SerializeField]
    public List<GridProperty> gridPropertyList;//�洢�������ԵĶ�̬�б���Ԥ����GridProperty�ࣩ


}
