using UnityEngine;

[System.Serializable]
public class GridCoordinate
{
    public int x;
    public int y;

    public GridCoordinate(int p1, int p2)
    {
        x = p1;
        y = p2;
    }

    //以下四个操作符实现 GridCoordinate 与 Unity 常用向量类型的显式转换
    public static explicit operator Vector2(GridCoordinate gridCoordinate)
    {
        return new Vector2((float)gridCoordinate.x, (float)gridCoordinate.y);
    }

    public static explicit operator Vector2Int(GridCoordinate gridCoordinate)
    {
        return new Vector2Int(gridCoordinate.x, gridCoordinate.y);
    }

    public static explicit operator Vector3(GridCoordinate gridCoordinate)
    {
        return new Vector3((float)gridCoordinate.x, (float)gridCoordinate.y, 0f);
    }

    public static explicit operator Vector3Int(GridCoordinate gridCoordinate)
    {
        return new Vector3Int(gridCoordinate.x, gridCoordinate.y, 0);
    }

}
