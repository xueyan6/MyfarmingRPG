using UnityEngine;

public enum AnimationName
{
    idleDown,
    idleUp,
    idleLeft,
    idleRight,
    walkUp,
    walkDown,
    walkLeft,
    walkRight,
    runUp,
    runDown,
    runLeft,
    runRight,
    useToolUp,
    useToolDown,
    useToolLeft,
    useToolRight,
    swingToolUp,
    swingToolDown,
    swingToolLeft,
    swingToolRight,
    liftToolUp,
    liftToolDown,
    liftToolLeft,
    liftToolRight,
    holdToolUp,
    holdToolDown,
    holdToolLeft,
    holdToolRight,
    pickUp,
    pickDown,
    pickLeft,
    pickRight,
    count
}


public enum CharacterPartAnimator
{
    body,
    arms,
    hair,
    tool,
    hat,
    count
}

public enum PartVariantColour
{
    none,
    count
}

public enum PartVariantType
{
    none,
    carry,
    hoe,
    pickaxe,
    axe,
    scythe,
    wateringCan,
    count

}

public enum GridBoolProperty
{
    diggable,
    canDropItem,
    canPlaceFurniture,
    isPath,
    isNPCObstacle
}

public enum InventoryLocation
{
    player,//物品在角色手里
    chest,//物品在箱子里
    count//枚举位置的计数，此处值为2，只要把count放在最后一位，自动进行计数
}

public enum SceneName
{
    Scene1_Farm,
    Scene2_Field,
    Scene3_Cabin
}

public enum Season
{
    Spring,
    Summer,
    Autumn,
    Winter,
    none,
    count
}


public enum ToolEffect
{
    None,
    watering,
}

public enum HarvestActionEffect
{
    deciduousLeavesFalling,//落叶飘零
    pineConesFalling,//松果坠落
    choppingTreeTrunk,//砍伐树干
    breakingStone,
    reaping,
    none
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
