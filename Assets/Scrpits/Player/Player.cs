using System.Collections.Generic;
using UnityEngine;


public class Player : SingletonMonobehaviour<Player>
{
    private AnimationOverrides animationOverrides;

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

    private ToolEffect toolEffect = ToolEffect.None;

    private Rigidbody2D Rigidbody2D;

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

    private void Update()
    {
        #region Player Input
        if (!PlayerInputIsDisabled)
        {
        ResetAnimationTrigger();

        PlayerMovementInput();

        PlayerWalkInput();

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


    // Temp routine for test input
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