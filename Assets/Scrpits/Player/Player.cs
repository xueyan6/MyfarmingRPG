using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : SingletonMonobehaviour<Player>
{
    private WaitForSeconds afterUseToolAnimationPause;
    private WaitForSeconds afterLiftToolAnimationPause;

    private AnimationOverrides animationOverrides;
    private GridCursor gridCursor;
    private Cursor cursor;

    //Movement Parameters运动参数
    private float xInput;
    private float yInput;
    private bool isCarrying = false;
    private bool isIdle;
    private bool isLiftingToolRight;
    private bool isLiftingToolLeft;
    private bool isLiftingToolUp;
    private bool isLiftingToolDown;
    private bool isRunning;
    private bool isUsingToolRight;
    private bool isUsingToolLeft;
    private bool isUsingToolUp;
    private bool isUsingToolDown;
    private bool isSwingingToolRight;
    private bool isSwingingToolLeft;
    private bool isSwingingToolUp;
    private bool isSwingingToolDown;
    private bool isWalking;
    private bool isPickingRight;
    private bool isPickingLeft;
    private bool isPickingUp;
    private bool isPickingDown;
    private WaitForSeconds liftToolAnimationPause;

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
    public bool PlayerInputIsDisabled
    {
        get => _playerInputIsDisable; set => _playerInputIsDisable = value;
    }

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

        //get reference to main camera获取主摄像头的引用
        mainCamera = Camera.main;
    }

    private void OnDisable()
    {
        EventHandler.BeforeSceneUnloadFadeOutEvent -= DisablePlayerInputAndResetMovement;
        EventHandler.AfterSceneLoadFadeInEvent -= EnablePlayerInput;
    }


    private void OnEnable()
    {
        EventHandler.BeforeSceneUnloadFadeOutEvent += DisablePlayerInputAndResetMovement;
        EventHandler.AfterSceneLoadFadeInEvent += EnablePlayerInput;
    }

        private void Start()
    {
        gridCursor = FindObjectOfType<GridCursor>();
        cursor = FindObjectOfType<Cursor>();

        useToolAnimationPause = new WaitForSeconds(Settings.useToolAnimationPause);
        liftToolAnimationPause = new WaitForSeconds(Settings.liftToolAnimationPause);

        afterUseToolAnimationPause = new WaitForSeconds(Settings.afterUseToolAnimationPause);
        afterLiftToolAnimationPause = new WaitForSeconds(Settings.afterLiftToolAnimationPause);
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
        isLiftingToolRight = false;
        isLiftingToolLeft = false;
        isLiftingToolUp = false;
        isLiftingToolDown = false;
        isUsingToolRight = false;
        isUsingToolLeft = false;
        isUsingToolUp = false;
        isUsingToolDown = false;
        isSwingingToolRight = false;
        isSwingingToolLeft = false;
        isSwingingToolUp = false;
        isSwingingToolDown = false;
        isPickingRight = false;
        isPickingLeft = false;
        isPickingUp = false;
        isPickingDown = false;
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
                        ProcessPlayerClickInputSeed(itemDetails);// 调用种子点击处理方法
                    }
                    break;
                case ItemType.Commodity:
                    if (Input.GetMouseButtonDown(0))// 如果是商品类型
                    {
                        ProcessPlayerClickInputCommodity(itemDetails);
                    }
                    break;

                case ItemType.Watering_tool:
                case ItemType.Hoeing_tool:
                case ItemType.Reaping_tool:
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


    private void ProcessPlayerClickInputSeed(ItemDetails itemDetails)
    {
        if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid)// 检查物品可丢弃且光标位置有效
        {
            EventHandler.CallDropSelectedItemEvent();// 触发丢弃物品事件
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
            Vector2 point = new Vector2(GetPlayerCentrePosition().x + playerDirection.x * (equippedItemDetails.itemUseRadius / 2f),
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


        // Temp routine for test input测试输入的临时例程
        private void PlayerTestInput()
    {
        // Trigger Advance Time触发提前时间
        if (Input.GetKey(KeyCode.T))
        {
            TimeManager.Instance.TestAdvanceGameMinute();
        }

        // Trigger Advance Day
        if (Input.GetKey(KeyCode.G))
        {
            TimeManager.Instance.TestAdvanceGameDay();
        }

        // Test scene unload / load锟斤拷锟皆筹拷锟斤拷卸锟斤拷/锟斤拷锟斤拷
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
        isIdle = false;
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
            //总结：1.为动画系统准备一个仅包含最新手臂状态的最小配置集。2.当调用ApplyCharacterCustomisationParameters时，系统会根据这个精简列表精确更新相关动画。

            
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
}