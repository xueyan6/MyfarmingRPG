public enum InventoryLocation
{
    player,//物品在角色手里
    chest,//物品在箱子里
    count//枚举位置的计数，此处值为2，只要把count放在最后一位，自动进行计数
}


public enum ToolEffect
{
    None,
    Warting,
}

public enum Direction
{
    Left,
    Right,
    Up,
    Down,
    None
}

public enum ItemType
{
    Seed,
    Commodity,
    Watering_tool,
    Hoeing_tool,
    Chopping_tool,
    Breaking_tool,
    Reaping_tool,
    Collecting_tool,
    Reapable_scenary,
    Furniture,
    none,
    count
}