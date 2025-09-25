using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "so_GridProperties", menuName = "Scriptable Objects/Grid Properties")]
public class SO_GridProperties : ScriptableObject
{
    public SceneName sceneName;//标识关联的场景（需自定义SceneName枚举类型）
    public int gridWidth;
    public int gridHeight;//一个场景的Tilemap的长宽
    public int originX;
    public int originY;//是tilemap左下角的坐标。

    [SerializeField]
    public List<GridProperty> gridPropertyList;//存储网格属性的动态列表（需预定义GridProperty类）


}
