using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : SingletonMonobehaviour<Player>
{
    private WaitForSeconds afterUseToolAnimationPause;

    private AnimationOverrides animationOverrides;
    private GridCursor gridCursor;

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

        // Initialise character attribute list初始化字符属性列表
        characterAttributeCustomisationList = new List<CharacterAttribute>();

        //get reference to main camera获取主摄像头的引用
        mainCamera = Camera.main;
    }

    private void Start()
    {
        gridCursor = FindObjectOfType<GridCursor>();
        useToolAnimationPause = new WaitForSeconds(Settings.useToolAnimationPause);
        afterUseToolAnimationPause = new WaitForSeconds(Settings.afterUseToolAnimationPause);

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


                if (gridCursor.CursorIsEnabled)
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

                case ItemType.Hoeing_tool:
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

        // Test scene unload / load测试场景卸载/加载
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

}