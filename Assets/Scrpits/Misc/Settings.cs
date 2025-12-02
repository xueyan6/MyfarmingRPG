
using UnityEngine;

public static class Settings 
{
    //Obscuring Item Fading-ObscuringItemFader遮蔽项淡出效果-遮蔽项淡出器
    public const float fadeInSeconds = 0.25f;
    public const float fadeOutSeconds = 0.35f;
    public const float tagetAlpha = 0.45f;

    // Tilemap
    public const float gridCellSize = 1f; // grid cell size in unity units网格单元尺寸in unity units
    public const float gridCellDiagonalSize = 1.41f;//diagonal distance between unity cell centres unity网格单元对角线距离
    public const int maxGridWidth = 99999;//检测到坐标超出边界 → 识别为"最终目的地标记"
    public const int maxGridHeight = 99999;//而使用日程事件中定义的真实目标坐标（对于非最终目的地，直接使用场景路径中预设的坐标）
    public static Vector2 cursorSize = Vector2.one;

    //Player
    public static float playerCentreYOffset = 0.875f;

    //Player Movement
    public const float runingSpeed = 5.333f;
    public const float walkingSpeed = 2.666f;
    public static float useToolAnimationPause = 0.25f;
    public static float liftToolAnimationPause = 0.4f;
    public static float pickAnimationPause = 1f;
    public static float afterUseToolAnimationPause = 0.2f;
    public static float afterLiftToolAnimationPause = 0.4f;
    public static float afterPickAnimationPause = 0.2f;

    //NPC Movement 
    public static float pixelSize = 0.0625f;//用于测试NPC移动时是否在距离目标位置一个像素的距离内

    //Inventory
    public static int playerInitialInventoryCapacity = 24;//玩家初始背包容量
    public static int playerMaximumInventoryCapacity = 48;//玩家最大背包容量

    // NPC Animation ParametersNPC动画参数
    public static int walkUp;
    public static int walkDown;
    public static int walkLeft;
    public static int walkRight;
    public static int eventAnimation;

    //Player Animation Parameters玩家动画参数
    public static int xInput;
    public static int yInput;
    public static int isWalking;
    public static int isRunning;
    public static int toolEffect; 
    public static int isUsingToolRight;
    public static int isUsingToolLeft;
    public static int isUsingToolUp;
    public static int isUsingToolDown;
    public static int isLiftingToolRight;
    public static int isLiftingToolLeft;
    public static int isLiftingToolUp;
    public static int isLiftingToolDown;
    public static int isSwingingToolRight;
    public static int isSwingingToolLeft;   
    public static int isSwingingToolUp;
    public static int isSwingingToolDown;
    public static int isPickingRight;
    public static int isPickingLeft;
    public static int isPickingUp;
    public static int isPickingDown;


    //Shared Animation Parameters共享动画参数
    public static int idleUp;
    public static int idleDown;
    public static int idleLeft;
    public static int idleRight;


    //Tool
    public const string HoeingTool = "Hoe"; 
    public const string ChoppingTool = "Axe"; 
    public const string BreakingTool = "Pickaxe"; 
    public const string ReapingTool = "Scythe";
    public const string WateringTool = "Watering Can";
    public const string CollectingTool = "Basket";

    // Reaping（收割）
    public const int maxCollidersToTestPerReapSwing = 15; // 每次收割检测的最多的碰撞体数量
    public const int maxTargetComponentsToDestroyPerReapSwing = 2; // 每次收割销毁的最多的组件数量

    // Time System
    public const float secondsPerGameSecond = 0.012f;

    //static constructor静态构造函数
    static Settings()
    {
        // NPC Animation parameters NPC动画参数
        walkUp = Animator.StringToHash("walkUp");
        walkDown = Animator.StringToHash("walkDown");
        walkLeft = Animator.StringToHash("walkLeft");
        walkRight = Animator.StringToHash("walkRight");
        eventAnimation = Animator.StringToHash("eventAnimation");

        //Player Animation Parameters玩家动画参数
        xInput = Animator.StringToHash("xInput");
        yInput = Animator.StringToHash("yInput");
        isWalking = Animator.StringToHash("isWalking");
        isRunning = Animator.StringToHash("isRunning");
        toolEffect = Animator.StringToHash("toolEffect");
        isUsingToolRight = Animator.StringToHash("isUsingToolRight");
        isUsingToolLeft = Animator.StringToHash("isUsingToolLeft");
        isUsingToolUp = Animator.StringToHash("isUsingToolUp");
        isUsingToolDown = Animator.StringToHash("isUsingToolDown");
        isLiftingToolRight = Animator.StringToHash("isLiftingToolRight");
        isLiftingToolLeft = Animator.StringToHash("isLiftingToolLeft");
        isLiftingToolUp = Animator.StringToHash("isLiftingToolUp");
        isLiftingToolDown = Animator.StringToHash("isLiftingToolDown");
        isSwingingToolRight = Animator.StringToHash("isSwingingToolRight");
        isSwingingToolLeft = Animator.StringToHash("isSwingingToolLeft");
        isSwingingToolUp = Animator.StringToHash("isSwingingToolUp");
        isSwingingToolDown = Animator.StringToHash("isSwingingToolDown");
        isPickingRight = Animator.StringToHash("isPickingRight");
        isPickingLeft = Animator.StringToHash("isPickingLeft");
        isPickingUp = Animator.StringToHash("isPickingUp");
        isPickingDown = Animator.StringToHash("isPickingDown");

        //Shared Animation Parameters共享动画参数
        idleUp = Animator.StringToHash("idleUp");
        idleDown = Animator.StringToHash("idleDown");
        idleLeft = Animator.StringToHash("idleLeft");
        idleRight = Animator.StringToHash("idleRight");




    }

}
