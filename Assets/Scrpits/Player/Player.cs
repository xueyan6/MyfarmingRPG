using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Player : SingletonMonobehaviour<Player>,ISaveable
{
    private WaitForSeconds afterUseToolAnimationPause;
    private WaitForSeconds afterLiftToolAnimationPause;
    private WaitForSeconds afterPickAnimationPause;

    private AnimationOverrides animationOverrides;
    private GridCursor gridCursor;
    private Cursor cursor;

    //Movement Parameters运动参数
    private float xInput;
    private float yInput;
    private bool isCarrying = false;
    private bool isIdle;
    private bool isLiftingToolDown;
    private bool isLiftingToolLeft;
    private bool isLiftingToolRight;
    private bool isLiftingToolUp;
    private bool isRunning;
    private bool isUsingToolDown;
    private bool isUsingToolLeft;
    private bool isUsingToolRight;
    private bool isUsingToolUp;
    private bool isSwingingToolDown;
    private bool isSwingingToolLeft;
    private bool isSwingingToolRight;
    private bool isSwingingToolUp;
    private bool isWalking;
    private bool isPickingUp;
    private bool isPickingDown;
    private bool isPickingLeft;
    private bool isPickingRight;
    private WaitForSeconds liftToolAnimationPause;
    private WaitForSeconds pickAnimationPause;

    private Camera mainCamera;

    private bool playerToolUseDisabled = false;  // 如果玩家正在使用某个道具，其他道具将被禁用

    private ToolEffect toolEffect = ToolEffect.None;

    private Rigidbody2D Rigidbody2D;

    private WaitForSeconds useToolAnimationPause;

    private Direction PlayerDirection;

    private List<CharacterAttribute> characterAttributeCustomisationList;  // 目标动作列表
    private float MovementSpeed;

    [Tooltip("Should be populated in the prefab with the equippped item sprite renderer")]
    [SerializeField] private SpriteRenderer equippedItemSpriteRenderer = null; // 武器的图片


    // Player attributes that can be swapped可替换的玩家属性
    private CharacterAttribute armsCharacterAttribute;
    private CharacterAttribute toolCharacterAttribute;


    private bool _playerInputIsDisable = false;
    public bool PlayerInputIsDisabled { get => _playerInputIsDisable; set => _playerInputIsDisable = value; }

    private string _iSaveableUniqueID;
    public string ISaveableUniqueID { get { return _iSaveableUniqueID; } set { _iSaveableUniqueID = value; } }

    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get { return _gameObjectSave; } set { _gameObjectSave = value; } }


    protected override void Awake()
    {
        base.Awake();
        Rigidbody2D = GetComponent<Rigidbody2D>();

        animationOverrides = GetComponentInChildren<AnimationOverrides>();

        // Initialise swappable character attributes初始化可交换字符属性
        armsCharacterAttribute = new CharacterAttribute(CharacterPartAnimator.arms, PartVariantColour.none, PartVariantType.none);
        toolCharacterAttribute = new CharacterAttribute(CharacterPartAnimator.tool, PartVariantColour.none, PartVariantType.none);

        // Initialise character attribute list初始化字符属性列表
        characterAttributeCustomisationList = new List<CharacterAttribute>();

        // Get unique ID for gameobject and create save data object 获取游戏对象的唯一ID并创建保存数据对象
        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;

        GameObjectSave = new GameObjectSave();


        //get reference to main camera获取主摄像头的引用
        mainCamera = Camera.main;
    }

    private void OnDisable()
    {
        ISaveableDeregister();
        EventHandler.BeforeSceneUnloadFadeOutEvent -= DisablePlayerInputAndResetMovement;
        EventHandler.AfterSceneLoadFadeInEvent -= EnablePlayerInput;
    }


    private void OnEnable()
    {
        ISaveableRegister();
        EventHandler.BeforeSceneUnloadFadeOutEvent += DisablePlayerInputAndResetMovement;
        EventHandler.AfterSceneLoadFadeInEvent += EnablePlayerInput;
    }

        private void Start()
    {
        gridCursor = FindObjectOfType<GridCursor>();
        cursor = FindObjectOfType<Cursor>();

        useToolAnimationPause = new WaitForSeconds(Settings.useToolAnimationPause);
        liftToolAnimationPause = new WaitForSeconds(Settings.liftToolAnimationPause); 
        pickAnimationPause = new WaitForSeconds(Settings.pickAnimationPause);

        afterUseToolAnimationPause = new WaitForSeconds(Settings.afterUseToolAnimationPause);
        afterLiftToolAnimationPause = new WaitForSeconds(Settings.afterLiftToolAnimationPause);
        afterPickAnimationPause = new WaitForSeconds(Settings.afterPickAnimationPause);
    }

    private void Update()
    {
        #region Player Input
        if (!PlayerInputIsDisabled)
        {
        ResetAnimationTrigger();

        PlayerMovementInput();

        PlayerWalkInput();

        PlayerClickInput();

        PlayerTestInput();



        //Send event to any listeners for player movement input向任何监听器发送事件以获取玩家移动输入
        EventHandler.CallMovementEvent(xInput, yInput,
                isWalking, isRunning, isIdle, isCarrying,
                toolEffect,
                isUsingToolRight, isUsingToolLeft, isUsingToolUp, isUsingToolDown,
                isLiftingToolRight, isLiftingToolLeft, isLiftingToolUp, isLiftingToolDown,
                isPickingRight, isPickingLeft, isPickingUp, isPickingDown,
                isSwingingToolRight, isSwingingToolLeft, isSwingingToolUp, isSwingingToolDown
                , false, false, false, false);
        }
        #endregion
    }

    private void FixedUpdate()
    {
        PlayerMovement();
    }

    private void PlayerMovement()
    {
        Vector2 move = new Vector2(xInput * MovementSpeed * Time.deltaTime, yInput * MovementSpeed * Time.deltaTime);
        Rigidbody2D.MovePosition(Rigidbody2D.position + move);
    }

    private void ResetAnimationTrigger()
    {
        isPickingRight = false;
        isPickingLeft = false;
        isPickingUp = false;
        isPickingDown = false;
        isUsingToolRight = false;
        isUsingToolLeft = false;
        isUsingToolUp = false;
        isUsingToolDown = false;
        isLiftingToolRight = false;
        isLiftingToolLeft = false;
        isLiftingToolUp = false;
        isLiftingToolDown = false;
        isSwingingToolRight = false;
        isSwingingToolLeft = false;
        isSwingingToolUp = false;
        isSwingingToolDown = false;
        toolEffect = ToolEffect.None;
    }

    private void PlayerMovementInput()
    {
        yInput = Input.GetAxisRaw("Vertical");
        xInput = Input.GetAxisRaw("Horizontal");

        if (yInput != 0 && xInput != 0)
        {
            xInput = xInput * 0.71f;
            yInput = yInput * 0.71f;
        }

        if (xInput != 0 || yInput != 0)
        {
            isRunning = true;
            isWalking = false;
            isIdle = false;
            MovementSpeed = Settings.runingSpeed;

            //Capture player direction for save game记录玩家方向用于存档
            if (xInput < 0)
            {
                PlayerDirection = Direction.Left;
            }
            else if (xInput > 0)
            {
                PlayerDirection = Direction.Right;
            }
            else if (yInput < 0)
            {
                PlayerDirection = Direction.Down;
            }
            else
            {
                PlayerDirection = Direction.Up;
            }
        }
        else if (xInput == 0 && yInput == 0)
        {
            isRunning = false;
            isWalking = false;
            isIdle = true;
        }
    }

    private void PlayerWalkInput()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            isRunning = false;
            isWalking = true;
            isIdle = false;
            MovementSpeed = Settings.walkingSpeed;
        }
        else
        {
            isRunning = true;
            isWalking = false;
            isIdle = false;
            MovementSpeed = Settings.runingSpeed;
        }
    }

    private void PlayerClickInput()
    {

        if (!playerToolUseDisabled)
        {

            if (Input.GetMouseButton(0))
            {


                if (gridCursor.CursorIsEnabled|| cursor.CursorIsEnable)
                {
                    // Get Cursor Grid Position获取光标网格位置
                    Vector3Int cursorGridPosition = gridCursor.GetGridPositionForCursor();

                    // Get Player Grid Position获取玩家网格位置
                    Vector3Int playerGridPosition = gridCursor.GetGridPositionForPlayer();

                    ProcessPlayerClickInput(cursorGridPosition, playerGridPosition);
                }
            }
        }
    }

    private void ProcessPlayerClickInput(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
    {
        ResetMovement();

        //获取玩家朝向
        Vector3Int playerDirection = GetPlayerClickDirection(cursorGridPosition, playerGridPosition);

        // Get Grid property details at cursor position (the GridCursor validation routine ensures that grid property details are not null)
        //获取光标位置处的网格属性详情（GridCursor验证程序确保网格属性详情不为空）
        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cursorGridPosition.x, cursorGridPosition.y);

        // Get Selected item details获取选定项的详细信息
        ItemDetails itemDetails = InventoryManager.Instance.GetSelectedInventoryItemDetails(InventoryLocation.player);// 获取玩家当前选中的物品详情

        if (itemDetails != null)// 检查是否有选中物品
        {
            switch (itemDetails.itemType)// 根据物品类型分支处理
            {
                case ItemType.Seed:// 如果是种子类型
                    if (Input.GetMouseButtonDown(0))// 检测鼠标左键点击
                    {
                        ProcessPlayerClickInputSeed(gridPropertyDetails,itemDetails);// 调用种子点击处理方法
                    }
                    break;
                case ItemType.Commodity:
                    if (Input.GetMouseButtonDown(0))// 如果是商品类型
                    {
                        ProcessPlayerClickInputCommodity(itemDetails);
                    }
                    break;

                case ItemType.Watering_tool:
                case ItemType.Breaking_tool:
                case ItemType.Chopping_tool:
                case ItemType.Hoeing_tool:
                case ItemType.Reaping_tool:
                case ItemType.Collecting_tool:
                    ProcessPlayerClickInputTool(gridPropertyDetails, itemDetails, playerDirection);
                    break;

                case ItemType.none:// 空物品类型
                    break;
                case ItemType.count:
                    break;

                default:
                    break;
            }
        }
    }

    private Vector3Int GetPlayerClickDirection(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
    {
        if (cursorGridPosition.x > playerGridPosition.x)//如果光标在玩家右侧（x坐标更大）
        {
            return Vector3Int.right;// 返回Unity预定义的右方向向量(1,0,0)
        }
        else if (cursorGridPosition.x < playerGridPosition.x) //如果光标在玩家左侧（x坐标更小）
        {
            return Vector3Int.left;// 返回Unity预定义的左方向向量(-1,0,0)
        }
        else if (cursorGridPosition.y > playerGridPosition.y)// 如果x坐标相同且光标在玩家上方（y坐标更大）
        {
            return Vector3Int.up;// 返回Unity预定义的上方向向量(0,1,0)
        }
        else
        {
            // 默认情况（x坐标相同且光标在玩家下方或同一位置）
            return Vector3Int.down;// 返回Unity预定义的下方向向量(0,-1,0)
        }
        //返回的是Unity预定义的标准方向向量，可直接用于角色朝向控制
    }

    //输入光标位置和玩家位置，返回玩家朝向的方向向量
    private Vector3Int GetPlayerDirection(Vector3 cursorPosition, Vector3 playerPosition)
    {
        // 检查光标是否在玩家右侧且Y轴在容差范围内
        if (
            cursorPosition.x > playerPosition.x// 光标X坐标大于玩家X坐标
            &&
            cursorPosition.y < (playerPosition.y + cursor.ItemUseRadius / 2f)// Y坐标小于上限
            &&
            cursorPosition.y > (playerPosition.y - cursor.ItemUseRadius / 2f)// Y坐标大于下限
            )
        {
            return Vector3Int.right;// 返回向右的单位向量(1,0,0)
        }
        // 检查光标是否在玩家左侧且Y轴在容差范围内
        else if (
            cursorPosition.x < playerPosition.x// 光标X坐标小于玩家X坐标
            &&
            cursorPosition.y < (playerPosition.y + cursor.ItemUseRadius / 2f)// Y坐标小于上限
            &&
            cursorPosition.y > (playerPosition.y - cursor.ItemUseRadius / 2f)// Y坐标大于下限
            )
        {
            return Vector3Int.left;// 返回向左的单位向量(-1,0,0)
        }
        // 检查光标是否在玩家上方（不考虑X轴）
        else if (cursorPosition.y > playerPosition.y)// 仅比较Y坐标
        {
            return Vector3Int.up;// 返回向上的单位向量(0,1,0)
        }
        else
        {
            return Vector3Int.down;// 返回向下的单位向量(0,-1,0)
        }
    }


    private void ProcessPlayerClickInputSeed(GridPropertyDetails gridPropertyDetails,ItemDetails itemDetails)
    {
        // 检查物品可丢弃且光标位置有效，是否被挖掘以及是否种有种子
        if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid && gridPropertyDetails.daysSinceDug > -1 && gridPropertyDetails.seedItemCode == -1)
        {
            PlantSeedAtCursor(gridPropertyDetails, itemDetails);
        }
        else if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid)// 检查物品可丢弃且光标位置有效
        {
            EventHandler.CallDropSelectedItemEvent();// 触发丢弃物品事件
        }
    }


    private void PlantSeedAtCursor(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails)
    {
        //Process if we have cropdetails for seed只处理有种植详情的种子
        if (GridPropertiesManager.Instance.GetCropDetails(itemDetails.itemCode) != null) 
        { 
          // update grid properties with seed details更新网格属性并添加种子详情
          gridPropertyDetails.seedItemCode = itemDetails.itemCode;
          gridPropertyDetails.growthDays = 0;

          // Display planted crop at grid property details在网格属性详情中显示种植作物
          GridPropertiesManager.Instance.DisplayPlantedCrop(gridPropertyDetails);

          // Remove item from inventory从物品栏中移除物品
          EventHandler.CallRemoveSelectedItemFromInventoryEvent();
        }

    }

        private void ProcessPlayerClickInputCommodity(ItemDetails itemDetails)
    {
        if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid)
        {
            EventHandler.CallDropSelectedItemEvent();
        }
    }


    private void ProcessPlayerClickInputTool(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails, Vector3Int playerDirection)
    {
        // 处理工具类型物品点击逻辑
        // 参数：
        // gridPropertyDetails - 目标网格的属性信息
        // itemDetails - 工具物品的详细数据
        // playerDirection - 玩家当前朝向

        // Switch on tool开启工具
        switch (itemDetails.itemType)// 根据工具类型分支处理
        {
            case ItemType.Hoeing_tool:// 锄具类型
                if (gridCursor.CursorPositionIsValid)// 验证光标位置有效性
                {
                    HoeGroundAtCursor(gridPropertyDetails, playerDirection);// 执行锄地操作
                }
                break;

            case ItemType.Watering_tool:
                if (gridCursor.CursorPositionIsValid)
                {
                    WaterGroundAtCursor(gridPropertyDetails, playerDirection);
                }
                break;

            case ItemType.Chopping_tool:
                if (gridCursor.CursorPositionIsValid)
                {
                   ChopInPlayerDirection(gridPropertyDetails, itemDetails, playerDirection);
                }
                break;

            case ItemType.Collecting_tool:
                if (gridCursor.CursorPositionIsValid)
                {
                    CollectInPlayerDirection(gridPropertyDetails, itemDetails, playerDirection);
                }
                break;
            case ItemType.Breaking_tool:
                if (gridCursor.CursorPositionIsValid)
                {
                    BreakInPlayerDirection(gridPropertyDetails, itemDetails, playerDirection);
                }
                break;

            case ItemType.Reaping_tool:
                if (cursor.CursorPositionIsValid)
                {
                    playerDirection = GetPlayerDirection(cursor.GetWorldPositionForCursor(), GetPlayerCentrePosition());
                    ReapInPlayerDirectionAtCursor(itemDetails, playerDirection);
                }
                break;

                default:
                break;

        }
    }


    private void HoeGroundAtCursor(GridPropertyDetails gridPropertyDetails, Vector3Int playerDirection)
    {
        // Trigger animation触发动画
        StartCoroutine(HoeGroundAtCursorRoutine(playerDirection, gridPropertyDetails));
    }

    private IEnumerator HoeGroundAtCursorRoutine(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails)
    {
        // 禁用玩家输入和工具使用
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        // Set tool animation to hoe in override animation配置锄具动画参数
        toolCharacterAttribute.partVariantType = PartVariantType.hoe;//设置工具动画类型为"hoe"（锄头）
        characterAttributeCustomisationList.Clear();//清空角色属性定制列表，准备接收新的动画参数配置
        characterAttributeCustomisationList.Add(toolCharacterAttribute); //将配置好的锄头工具属性添加到定制列表，该列表包含所有需要覆盖的动画参数
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);//应用动画参数覆盖，系统会根据列表中的配置动态替换默认动画片段

        // 根据朝向设置对应的动画标志位
        if (playerDirection == Vector3Int.right)
        {
            isUsingToolRight = true;
        }
        else if (playerDirection == Vector3Int.left)
        {
            isUsingToolLeft = true;
        }
        else if (playerDirection == Vector3Int.up)
        {
            isUsingToolUp = true;
        }
        else if (playerDirection == Vector3Int.down)
        {
            isUsingToolDown = true;
        }
   
        yield return useToolAnimationPause;// 等待锄地动画完成

        // Set Grid property details for dug ground更新地块挖掘状态
        if (gridPropertyDetails.daysSinceDug == -1)
        {
            gridPropertyDetails.daysSinceDug = 0;
        }

        // Set grid property to dug保存更新后的地块属性
        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        // Display dug grid tiles 显示挖掘网格瓦片
        GridPropertiesManager.Instance.DisplayDugGround(gridPropertyDetails);

        // After animation pause等待动画后处理时间
        yield return afterUseToolAnimationPause;

        // 恢复玩家控制
        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private void WaterGroundAtCursor(GridPropertyDetails gridPropertyDetails, Vector3Int playerDirection)
    {
        // Trigger animation触发动画
        StartCoroutine(WaterGroundAtCursorRoutine(playerDirection, gridPropertyDetails));

    }

    private IEnumerator WaterGroundAtCursorRoutine(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails)
    {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        // Set tool animation to watering can in override animation将工具动画设置为浇水壶（覆盖动画）
        toolCharacterAttribute.partVariantType = PartVariantType.wateringCan;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        //TODO: If there is water in the watering can待办事项：若喷壶内有水
        toolEffect = ToolEffect.watering;

        if (playerDirection == Vector3Int.right)
        {
            isLiftingToolRight = true;
        }
        else if (playerDirection == Vector3Int.left)
        {
            isLiftingToolLeft = true;
        }
        else if (playerDirection == Vector3Int.up)
        {
            isLiftingToolUp = true;
        }
        else if (playerDirection == Vector3Int.down)
        {
            isLiftingToolDown = true;
        }

        yield return liftToolAnimationPause;

        // Set Grid property details for watered ground设置灌溉地块的网格属性详情
        if (gridPropertyDetails.daysSinceWatered == -1)
        {
            gridPropertyDetails.daysSinceWatered = 0;
        }

        // Set grid property to watered将网格属性设置为浇水状态
        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        // Display watered grid tiles显示带网格线的水渍瓷砖
        GridPropertiesManager.Instance.DisplayWateredGround(gridPropertyDetails);

        // After animation pause动画暂停后
        yield return afterLiftToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;

    }

    private void ChopInPlayerDirection(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails, Vector3Int playerDirection)
    {
        // Trigger animation触发动画
        StartCoroutine(ChopInPlayerDirectionRoutine(gridPropertyDetails, itemDetails, playerDirection));

    }

    private IEnumerator ChopInPlayerDirectionRoutine(GridPropertyDetails gridPropertyDetails, ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        // Set tool animation to axe in override animation将工具动画设置为斧头覆盖动画
        toolCharacterAttribute.partVariantType = PartVariantType.axe;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        ProcessCropWithEquippedItemInPlayerDirection(playerDirection, equippedItemDetails, gridPropertyDetails);

        yield return useToolAnimationPause;

        // After animation pause 动画暂停后 
        yield return afterUseToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

        private void CollectInPlayerDirection(GridPropertyDetails gridPropertyDetails, ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        // 启动一个协程来处理采集过程，以避免阻塞主线程并允许插入延时
        StartCoroutine(CollectInPlayerDirectionRoutine(gridPropertyDetails, equippedItemDetails, playerDirection));
    }

    private IEnumerator CollectInPlayerDirectionRoutine(GridPropertyDetails gridPropertyDetails, ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        PlayerInputIsDisabled = true;// 禁用玩家输入，防止在动画期间移动
        playerToolUseDisabled = true;// 禁用工具使用，防止连续快速触发

        ProcessCropWithEquippedItemInPlayerDirection(playerDirection, equippedItemDetails, gridPropertyDetails);// 执行核心的作物处理逻辑

        yield return pickAnimationPause; // 等待采集动画播放完成

        // After animation pause
        yield return afterPickAnimationPause;// 等待一个额外的缓冲时间

        PlayerInputIsDisabled = false;// 重新启用玩家输入
        playerToolUseDisabled = false;// 重新启用工具使用

    }

    //关于协程：（StartCoroutine、IEnumerator、yield）
    //它能够将这个方法“拉长” over time。它执行到 yield return pickAnimationPause; 时就自动暂停了，等待指定的时间（比如0.5秒）过后，Unity引擎会自动唤醒它，继续执行后面的代码。
    //所以，在您的代码中启动协程，就是为了实现这样一个符合逻辑和时间顺序的流程：开始动作 -> 等待动作完成 -> 结束动作。这确保了游戏体验的流畅性和合理性。
    //一个简单的判断法则：如果你的脚本中想要使用“延时”、“过几秒再”、“等到...的时候”这类词语来描述逻辑，那么99%的情况就是你该启动一个协程的时候。

    private void BreakInPlayerDirection(GridPropertyDetails gridPropertyDetails, ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        StartCoroutine(BreakInPlayerDirectionRoutine(gridPropertyDetails, equippedItemDetails, playerDirection));

    }


    private IEnumerator BreakInPlayerDirectionRoutine(GridPropertyDetails gridPropertyDetails, ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        // Set tool animation to pickaxe in override animation将工具动画设置为镐头（覆盖动画）
        toolCharacterAttribute.partVariantType = PartVariantType.pickaxe;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        ProcessCropWithEquippedItemInPlayerDirection(playerDirection, equippedItemDetails, gridPropertyDetails);

        yield return useToolAnimationPause;

        // After animation pause动画暂停后
        yield return afterUseToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;

    }


        private void ReapInPlayerDirectionAtCursor(ItemDetails itemDetails, Vector3Int playerDirection)
    {
        // StartCoroutine:开启协程执行收割流程（避免阻塞主线程）
        StartCoroutine(ReapInPlayerDirectionAtCursorRoutine(itemDetails, playerDirection));
    }

    private IEnumerator ReapInPlayerDirectionAtCursorRoutine(ItemDetails itemDetails, Vector3Int playerDirection)
    {
        // 禁用玩家移动输入
        PlayerInputIsDisabled = true;
        // 禁用工具使用（防止重复触发）
        playerToolUseDisabled = true;

        // Set tool animation to scythe in override animation将工具动画设置为镰刀的覆盖动画
        // 配置角色动画参数
        toolCharacterAttribute.partVariantType = PartVariantType.scythe;// 先设定为镰刀动画（如果先清空再指定目标，会导致短暂的无状态期（可能出现T-Pose闪烁））
        characterAttributeCustomisationList.Clear();// 清空原有动画参数
        characterAttributeCustomisationList.Add(toolCharacterAttribute);// 添加镰刀参数
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);// 应用动画覆盖（立即生效）

        // Reap in player direction朝玩家方向收割
        // 执行实际收割逻辑
        UseToolInPlayerDirection(itemDetails, playerDirection);

        // 等待动画完成（useToolAnimationPause是预定义的等待时间）
        yield return useToolAnimationPause;

        // 恢复玩家控制
        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    // 实际执行工具使用的核心方法
    private void UseToolInPlayerDirection(ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        // 检测鼠标左键持续按下状态
        if (Input.GetMouseButton(0))
        {
            // 根据工具类型执行不同逻辑
            switch (equippedItemDetails.itemType)
            {
                case ItemType.Reaping_tool:// 收割工具处理分支
                    // 根据方向设置对应的动画标志位
                    if (playerDirection == Vector3Int.right)
                    {
                        isSwingingToolRight = true;// 触发右挥动画
                    }
                    else if (playerDirection == Vector3Int.left)
                    {
                        isSwingingToolLeft = true;// 触发左挥动画
                    }
                    else if (playerDirection == Vector3Int.up)
                    {
                        isSwingingToolUp = true;// 触发上挥动画
                    }
                    else if (playerDirection == Vector3Int.down)
                    {
                        isSwingingToolDown = true;// 触发下挥动画
                    }
                    break;
            }

            // Define centre point of square which will be used for collision testing定义用于碰撞检测的正方形中心点
            // 碰撞检测区域计算
            // 检测中心点X坐标（玩家位置+方向偏移：当玩家角色位于(0,0)且向右挥动镰刀（作用半径2单位）时，检测中心将计算为(1,0)，这样形成的2x2单位检测框既能完整覆盖右侧2单位范围，又避免左侧无效检测）
            Vector2 point = new Vector2(GetPlayerCentrePosition().x + (playerDirection.x * (equippedItemDetails.itemUseRadius / 2f)),
                GetPlayerCentrePosition().y + playerDirection.y * (equippedItemDetails.itemUseRadius / 2f));// 检测中心点Y坐标

            // Define size of the square which will be used for collision testing定义用于碰撞检测的正方形尺寸
            Vector2 size = new Vector2(equippedItemDetails.itemUseRadius, equippedItemDetails.itemUseRadius);// 检测区域大小（正方形）

            // Get Item components with 2D collider located in the square at the centre point defined (2d colliders tested limited to maxCollidersToTestPerReapSwing)
            //获取位于定义中心点正方形区域内的、带有2D碰撞器的物品组件（测试的2D碰撞器数量受限于每次收割摆动可测试的最大碰撞器数量）
            Item[] itemArray = HelperMethods.GetComponentsAtBoxLocationNonAlloc<Item>(Settings.maxCollidersToTestPerReapSwing, point, size, 0f);

            int reapableItemCount = 0;// 可收割物品计数器

            // Loop through all items retrieved逆向遍历检测到的物品（避免修改集合问题）
            for (int i = itemArray.Length - 1; i >= 0; i--)
            {
                if (itemArray[i] != null)
                {
                    // Destory item game object if reapable验证物品类型是否可收割
                    if (InventoryManager.Instance.GetItemDetails(itemArray[i].ItemCode).itemType == ItemType.Reapable_scenary)
                    {
                        // Effect position计算特效生成位置（物品中心点上方半格）
                        Vector3 effectPosition = new Vector3(itemArray[i].transform.position.x, itemArray[i].transform.position.y + Settings.gridCellSize / 2f,
                            itemArray[i].transform.position.z);

                        // Trigger reaping effect 触发收割效果 
                        EventHandler.CallHarvestActionEffectEvent(effectPosition, HarvestActionEffect.reaping);

                        // 执行收割操作
                        Destroy(itemArray[i].gameObject);// 销毁游戏对象
                        reapableItemCount++;// 增加计数器

                        // 达到单次最大收割数量则终止
                        if (reapableItemCount >= Settings.maxTargetComponentsToDestroyPerReapSwing)
                            break;
                    }
                }
            }


        }
    }

    //Method processes crop with equipped item in player direction方法使用玩家方向的装备物品处理作物
    private void ProcessCropWithEquippedItemInPlayerDirection(Vector3Int playerDirection, ItemDetails equippedItemDetails, GridPropertyDetails gridPropertyDetails)
    {
        // 根据装备的物品类型进行判断
        switch (equippedItemDetails.itemType)
        {
            case ItemType.Chopping_tool:
            case ItemType.Breaking_tool:
                if (playerDirection == Vector3Int.right)
                {
                    isUsingToolRight = true;
                }
                else if (playerDirection == Vector3Int.left)
                {
                    isUsingToolLeft = true;
                }
                else if (playerDirection == Vector3Int.up)
                {
                    isUsingToolUp = true;
                }
                else if (playerDirection == Vector3Int.down)
                {
                    isUsingToolDown = true;
                }

            break;

            case ItemType.Collecting_tool:

                if (playerDirection == Vector3Int.right)
                {
                    isPickingRight = true;
                }
                else if (playerDirection == Vector3Int.left)
                {
                    isPickingLeft = true;
                }
                else if (playerDirection == Vector3Int.up)
                {
                    isPickingUp = true;
                }
                else if (playerDirection == Vector3Int.down)
                {
                    isPickingDown = true;
                }

            break;

            case ItemType.none:
            break;
        }

        // Get crop at cursor grid location在光标网格位置获取裁剪区域
        Crop crop = GridPropertiesManager.Instance.GetCropObjectAtGridLocation(gridPropertyDetails);// 通过单例管理器获取指定网格位置上的作物对象

        // Execute Process Tool Action For crop执行进程工具操作 用于作物
        if (crop != null)
        {
            switch (equippedItemDetails.itemType)// 再次根据工具类型判断
            {
                case ItemType.Chopping_tool:
                case ItemType.Breaking_tool:
                    crop.ProcessToolAction(equippedItemDetails, isUsingToolRight, isUsingToolLeft, isUsingToolDown, isUsingToolUp);
                break;

                case ItemType.Collecting_tool:
                    crop.ProcessToolAction(equippedItemDetails, isPickingRight, isPickingLeft, isPickingUp, isPickingDown);
                break;
            }
        }

    }

        // Temp routine for test input测试输入的临时例程
        private void PlayerTestInput()
    {
        // Trigger Advance Time触发提前时间
        if (Input.GetKey(KeyCode.T))
        {
            TimeManager.Instance.TestAdvanceGameMinute();
        }

        // Trigger Advance Day
        if (Input.GetKeyDown(KeyCode.G))
        {
            TimeManager.Instance.TestAdvanceGameDay();
        }

        // Test scene unload / load
        if (Input.GetKeyDown(KeyCode.L))
        {
            SceneControllerManager.Instance.FadeAndLoadScene(SceneName.Scene1_Farm.ToString(), transform.position);
        }

    }
        private void ResetMovement()
    {
        //Reset movement
        xInput = 0f; 
        yInput = 0f;
        isRunning = false;
        isWalking = false;
        isIdle = true;
    }

    public void DisablePlayerInputAndResetMovement()
    {
        DisablePlayerInput();
        ResetMovement();

        // Send event to any listeners for player movement input将玩家移动输入事件发送给所有监听器
        EventHandler.CallMovementEvent(xInput, yInput,
        isWalking, isRunning, isIdle, isCarrying,
        toolEffect,
        isUsingToolRight, isUsingToolLeft, isUsingToolUp, isUsingToolDown,
        isLiftingToolRight, isLiftingToolLeft, isLiftingToolUp, isLiftingToolDown,
        isPickingRight, isPickingLeft, isPickingUp, isPickingDown,
        isSwingingToolRight, isSwingingToolLeft, isSwingingToolUp, isSwingingToolDown
        , false, false, false, false);
    }


    public void DisablePlayerInput()
    {
        PlayerInputIsDisabled = true;
    }

    public void EnablePlayerInput()
    {
        PlayerInputIsDisabled=false;
    }

    public void ClearCarriedItem()
    {
        // 清空装备物品的Sprite
        equippedItemSpriteRenderer.sprite = null;
        // 设置物品颜色为完全透明
        equippedItemSpriteRenderer.color = new Color(1f, 1f, 1f, 0f);

        // Apply base character arms customisation应用基础角色手臂自定义
        // 重置手臂部位变体类型为无状态
        armsCharacterAttribute.partVariantType = PartVariantType.none;
        // 清空自定义参数列表并添加当前手臂配置
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(armsCharacterAttribute);
        // 重新应用动画覆盖配置
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        isCarrying = false;
    }

    public void ShowCarriedItem(int itemCode)
    {
        // 通过库存管理器获取物品详情
        ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(itemCode);
        // 检查物品是否存在
        if (itemDetails != null)
        {
            // 更新装备物品的Sprite
            equippedItemSpriteRenderer.sprite = itemDetails.itemSprite;
            // 设置物品颜色为完全不透明
            equippedItemSpriteRenderer.color = new Color(1f, 1f, 1f, 1f);

            // Apply 'carry' character arms customisation应用“携带”角色武器定制
            // 设置手臂部位变体类型为携带状态
            armsCharacterAttribute.partVariantType = PartVariantType.carry;


            // 清空自定义参数列表并添加当前手臂配置（表示清空当前存储的所有角色属性配置，目的是确保每次更新都是全新的配置，避免旧配置残留）
            characterAttributeCustomisationList.Clear();
            //将修改后的手臂属性(armsCharacterAttribute)添加到空列表中(包括：部位类型(characterPart)、颜色变体(partVariantColour)、动作变体(partVariantType)、动画名称(animationName)）
            characterAttributeCustomisationList.Add(armsCharacterAttribute);
            //总结：1.为动画系统准备一个仅包含最新手臂状态的最小配置集。2.当调用 ApplyCharacterCustomisationParameters 时，系统会根据这个精简列表精确更新相关动画。

            
            // 应用动画覆盖配置
            animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

            isCarrying = true;
        }
    }

    public Vector3 GetPlayerViewportPosition()
    {
      //Vector3 viewport  position for player ((0,0) viewport bottom left, (1,1) viewport top right
      //Vector3 玩家视口位置（(0,0) 为视口左下角，(1,1) 为视口右上角）
      return mainCamera.WorldToViewportPoint(transform.position);
    }

    //获取玩家人物中心点（从脚下挪到人物中心）
    public Vector3 GetPlayerCentrePosition()
    {
        return new Vector3(transform.position.x, transform.position.y + Settings.playerCentreYOffset, transform.position.z);

    }

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this); // 将当前对象注册到存档管理器的可存档对象列表中
    }

    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this); // 从存档管理器的可存档对象列表中注销当前对象
    }

    public GameObjectSave ISaveableSave()
    {
        // 如果游戏对象在持久化场景中已有存档数据，先删除旧的存档数据
        GameObjectSave.sceneData.Remove(Settings.PersistentScene);

        // 创建新的场景保存对象，用于存储当前游戏状态
        SceneSave sceneSave = new SceneSave();

        // 初始化Vector3序列化字典，用于存储三维坐标数据（如位置、旋转等）
        sceneSave.vector3Dictionary = new Dictionary<string, Vector3Serializable>();

        // 初始化字符串字典，用于存储文本类型的数据
        sceneSave.stringDictionary = new Dictionary<string, string>();

        // 将玩家当前位置转换为可序列化的Vector3格式
        Vector3Serializable vector3Serializable = new Vector3Serializable(transform.position.x, transform.position.y, transform.position.z);
        // 将玩家位置数据添加到Vector3字典中，键为"playerPosition"
        sceneSave.vector3Dictionary.Add("playerPosition", vector3Serializable);

        // 将当前激活的场景名称添加到字符串字典中
        sceneSave.stringDictionary.Add("currentScene", SceneManager.GetActiveScene().name);

        // 将玩家当前朝向转换为字符串并添加到字典中
        sceneSave.stringDictionary.Add("playerDirection", PlayerDirection.ToString());

        // 将场景保存数据添加到游戏对象保存数据中，关联到持久化场景
        GameObjectSave.sceneData.Add(Settings.PersistentScene, sceneSave);

        // 返回包含所有存档数据的游戏对象保存实例
        return GameObjectSave;
    }

    public void ISaveableLoad(GameSave gameSave)
    {
        // 尝试从游戏存档数据中获取当前对象的唯一ID对应的存档数据
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameObjectSave gameObjectSave))
        {
            // 从游戏对象存档数据中获取持久化场景的存档信息
            if (gameObjectSave.sceneData.TryGetValue(Settings.PersistentScene, out SceneSave sceneSave))
            {
                // 检查Vector3字典是否存在且不为空，然后尝试获取玩家位置数据
                if (sceneSave.vector3Dictionary != null && sceneSave.vector3Dictionary.TryGetValue("playerPosition", out Vector3Serializable playerPosition))
                {
                    // 将从存档中读取的玩家位置数据应用到当前游戏对象的transform上
                    transform.position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);
                }

                // 检查字符串字典是否存在且不为空
                if (sceneSave.stringDictionary != null)
                {
                    // 从字符串字典中获取玩家所在的场景名称
                    if (sceneSave.stringDictionary.TryGetValue("currentScene", out string currentScene))
                    {
                        // 通过场景控制器淡入淡出并加载指定的场景，同时保持玩家位置
                        SceneControllerManager.Instance.FadeAndLoadScene(currentScene, transform.position);
                    }

                    // 从字符串字典中获取玩家朝向信息
                    if (sceneSave.stringDictionary.TryGetValue("playerDirection", out string playerDir))
                    {
                        // 尝试将字符串形式的朝向解析为Direction枚举类型
                        bool playerDirFound = Enum.TryParse<Direction>(playerDir, true, out Direction direction);

                        // 如果解析成功，更新玩家朝向并设置相应的动画状态
                        if (playerDirFound)
                        {
                            PlayerDirection = direction; // 更新玩家朝向属性
                            SetPlayerDirection(PlayerDirection); // 调用方法设置玩家朝向相关的动画和状态
                        }
                    }
                }
            }
        }
    }

    public void ISaveableStoreScene(string sceneName)
    {
        // 由于玩家位于持久化场景中，不需要特别处理场景存储
        // 此方法为空实现，因为玩家的数据已经在持久化场景中处理
    }

    public void ISaveableRestoreScene(string sceneName)
    {
        // 由于玩家位于持久化场景中，不需要特别处理场景恢复
        // 此方法为空实现，因为玩家的数据恢复已经在ISaveableLoad中处理
    }

    private void SetPlayerDirection(Direction playerDirection)
    {
        // 根据玩家朝向设置相应的动画状态和移动参数
        switch (playerDirection)
        {
            case Direction.Up:
                // 设置玩家朝上的闲置动画状态
                // 通过事件处理器调用移动事件，传入大量布尔参数控制各种游戏状态
                // 最后一个true参数表示激活朝上的闲置动画触发器
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.None, false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false, true, false, false, false);
                break;

            case Direction.Down:
                // 设置玩家朝下的闲置动画状态
                // 参数结构与上面类似，但激活的是朝下的闲置动画触发器
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.None, false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false, false, true, false, false);
                break;

            case Direction.Left:
                // 设置玩家朝左的闲置动画状态
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.None, false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false, false, false, false, true);
                break;

            case Direction.Right:
                // 设置玩家朝右的闲置动画状态
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.None, false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false, false, false, true, false);
                break;

            default:
                // 默认情况下设置玩家朝下的闲置动画状态
                // 这是安全回退，确保即使遇到未知朝向也有合理的默认行为
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.None, false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false, false, true, false, false);
                break;
        }
    }

}