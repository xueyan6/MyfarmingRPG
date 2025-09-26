
[System.Serializable]
public class GridProperty
{
    public GridCoordinate gridCoordinate;     // 坐标
    public GridBoolProperty gridBoolProperty; // 地面属性
    public bool gridBoolValue = false;        // 是否Bool属性

    public GridProperty(GridCoordinate gridCoordinate, GridBoolProperty gridBoolProperty, bool gridBoolValue)
    {
        this.gridCoordinate = gridCoordinate;
        this.gridBoolProperty = gridBoolProperty;
        this.gridBoolValue = gridBoolValue;
    }


}
