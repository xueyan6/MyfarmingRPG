using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : SingletonMonobehaviour<Player>
{
    private WaitForSeconds afterUseToolAnimationPause;

    private AnimationOverrides animationOverrides;
    private GridCursor gridCursor;

    //Movement Parameters�˶�����
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

    private bool playerToolUseDisabled = false;  // ����������ʹ��ĳ�����ߣ��������߽�������

    private ToolEffect toolEffect = ToolEffect.None;

    private Rigidbody2D Rigidbody2D;

    private WaitForSeconds useToolAnimationPause;

    private Direction PlayerDirection;

    private List<CharacterAttribute> characterAttributeCustomisationList;  // Ŀ�궯���б�
    private float MovementSpeed;

    [Tooltip("Should be populated in the prefab with the equippped item sprite renderer")]
    [SerializeField] private SpriteRenderer equippedItemSpriteRenderer = null; // ������ͼƬ


    // Player attributes that can be swapped���滻���������
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

        // Initialise swappable character attributes��ʼ���ɽ����ַ�����
        armsCharacterAttribute = new CharacterAttribute(CharacterPartAnimator.arms, PartVariantColour.none, PartVariantType.none);

        // Initialise character attribute list��ʼ���ַ������б�
        characterAttributeCustomisationList = new List<CharacterAttribute>();

        //get reference to main camera��ȡ������ͷ������
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



        //Send event to any listeners for player movement input���κμ����������¼��Ի�ȡ����ƶ�����
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

            //Capture player direction for save game��¼��ҷ������ڴ浵
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
                    // Get Cursor Grid Position��ȡ�������λ��
                    Vector3Int cursorGridPosition = gridCursor.GetGridPositionForCursor();

                    // Get Player Grid Position��ȡ�������λ��
                    Vector3Int playerGridPosition = gridCursor.GetGridPositionForPlayer();

                    ProcessPlayerClickInput(cursorGridPosition, playerGridPosition);
                }
            }
        }
    }

    private void ProcessPlayerClickInput(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
    {
        ResetMovement();

        //��ȡ��ҳ���
        Vector3Int playerDirection = GetPlayerClickDirection(cursorGridPosition, playerGridPosition);

        // Get Grid property details at cursor position (the GridCursor validation routine ensures that grid property details are not null)
        //��ȡ���λ�ô��������������飨GridCursor��֤����ȷ�������������鲻Ϊ�գ�
        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cursorGridPosition.x, cursorGridPosition.y);

        // Get Selected item details��ȡѡ�������ϸ��Ϣ
        ItemDetails itemDetails = InventoryManager.Instance.GetSelectedInventoryItemDetails(InventoryLocation.player);// ��ȡ��ҵ�ǰѡ�е���Ʒ����

        if (itemDetails != null)// ����Ƿ���ѡ����Ʒ
        {
            switch (itemDetails.itemType)// ������Ʒ���ͷ�֧����
            {
                case ItemType.Seed:// �������������
                    if (Input.GetMouseButtonDown(0))// ������������
                    {
                        ProcessPlayerClickInputSeed(itemDetails);// �������ӵ��������
                    }
                    break;
                case ItemType.Commodity:
                    if (Input.GetMouseButtonDown(0))// �������Ʒ����
                    {
                        ProcessPlayerClickInputCommodity(itemDetails);
                    }
                    break;

                case ItemType.Hoeing_tool:
                    ProcessPlayerClickInputTool(gridPropertyDetails, itemDetails, playerDirection);
                    break;

                case ItemType.none:// ����Ʒ����
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
        if (cursorGridPosition.x > playerGridPosition.x)//������������Ҳࣨx�������
        {
            return Vector3Int.right;// ����UnityԤ������ҷ�������(1,0,0)
        }
        else if (cursorGridPosition.x < playerGridPosition.x) //�������������ࣨx�����С��
        {
            return Vector3Int.left;// ����UnityԤ�������������(-1,0,0)
        }
        else if (cursorGridPosition.y > playerGridPosition.y)// ���x������ͬ�ҹ��������Ϸ���y�������
        {
            return Vector3Int.up;// ����UnityԤ������Ϸ�������(0,1,0)
        }
        else
        {
            // Ĭ�������x������ͬ�ҹ��������·���ͬһλ�ã�
            return Vector3Int.down;// ����UnityԤ������·�������(0,-1,0)
        }
        //���ص���UnityԤ����ı�׼������������ֱ�����ڽ�ɫ�������
    }


    private void ProcessPlayerClickInputSeed(ItemDetails itemDetails)
    {
        if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid)// �����Ʒ�ɶ����ҹ��λ����Ч
        {
            EventHandler.CallDropSelectedItemEvent();// ����������Ʒ�¼�
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
        // ������������Ʒ����߼�
        // ������
        // gridPropertyDetails - Ŀ�������������Ϣ
        // itemDetails - ������Ʒ����ϸ����
        // playerDirection - ��ҵ�ǰ����

        // Switch on tool��������
        switch (itemDetails.itemType)// ���ݹ������ͷ�֧����
        {
            case ItemType.Hoeing_tool:// ��������
                if (gridCursor.CursorPositionIsValid)// ��֤���λ����Ч��
                {
                    HoeGroundAtCursor(gridPropertyDetails, playerDirection);// ִ�г��ز���
                }
                break;

            default:
                break;

        }
    }


    private void HoeGroundAtCursor(GridPropertyDetails gridPropertyDetails, Vector3Int playerDirection)
    {
        // Trigger animation��������
        StartCoroutine(HoeGroundAtCursorRoutine(playerDirection, gridPropertyDetails));
    }

    private IEnumerator HoeGroundAtCursorRoutine(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails)
    {
        // �����������͹���ʹ��
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        // Set tool animation to hoe in override animation���ó��߶�������
        toolCharacterAttribute.partVariantType = PartVariantType.hoe;//���ù��߶�������Ϊ"hoe"����ͷ��
        characterAttributeCustomisationList.Clear();//��ս�ɫ���Զ����б�׼�������µĶ�����������
        characterAttributeCustomisationList.Add(toolCharacterAttribute); //�����úõĳ�ͷ����������ӵ������б����б����������Ҫ���ǵĶ�������
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);//Ӧ�ö����������ǣ�ϵͳ������б��е����ö�̬�滻Ĭ�϶���Ƭ��

        // ���ݳ������ö�Ӧ�Ķ�����־λ
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
   
        yield return useToolAnimationPause;// �ȴ����ض������

        // Set Grid property details for dug ground���µؿ��ھ�״̬
        if (gridPropertyDetails.daysSinceDug == -1)
        {
            gridPropertyDetails.daysSinceDug = 0;
        }

        // Set grid property to dug������º�ĵؿ�����
        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        // Display dug grid tiles ��ʾ�ھ�������Ƭ
        GridPropertiesManager.Instance.DisplayDugGround(gridPropertyDetails);

        // After animation pause�ȴ���������ʱ��
        yield return afterUseToolAnimationPause;

        // �ָ���ҿ���
        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    // Temp routine for test input�����������ʱ����
    private void PlayerTestInput()
    {
        // Trigger Advance Time������ǰʱ��
        if (Input.GetKey(KeyCode.T))
        {
            TimeManager.Instance.TestAdvanceGameMinute();
        }

        // Trigger Advance Day
        if (Input.GetKey(KeyCode.G))
        {
            TimeManager.Instance.TestAdvanceGameDay();
        }

        // Test scene unload / load���Գ���ж��/����
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

        // Send event to any listeners for player movement input������ƶ������¼����͸����м�����
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
        // ���װ����Ʒ��Sprite
        equippedItemSpriteRenderer.sprite = null;
        // ������Ʒ��ɫΪ��ȫ͸��
        equippedItemSpriteRenderer.color = new Color(1f, 1f, 1f, 0f);

        // Apply base character arms customisationӦ�û�����ɫ�ֱ��Զ���
        // �����ֱ۲�λ��������Ϊ��״̬
        armsCharacterAttribute.partVariantType = PartVariantType.none;
        // ����Զ�������б���ӵ�ǰ�ֱ�����
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(armsCharacterAttribute);
        // ����Ӧ�ö�����������
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        isCarrying = false;
    }

    public void ShowCarriedItem(int itemCode)
    {
        // ͨ������������ȡ��Ʒ����
        ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(itemCode);
        // �����Ʒ�Ƿ����
        if (itemDetails != null)
        {
            // ����װ����Ʒ��Sprite
            equippedItemSpriteRenderer.sprite = itemDetails.itemSprite;
            // ������Ʒ��ɫΪ��ȫ��͸��
            equippedItemSpriteRenderer.color = new Color(1f, 1f, 1f, 1f);

            // Apply 'carry' character arms customisationӦ�á�Я������ɫ��������
            // �����ֱ۲�λ��������ΪЯ��״̬
            armsCharacterAttribute.partVariantType = PartVariantType.carry;


            // ����Զ�������б���ӵ�ǰ�ֱ����ã���ʾ��յ�ǰ�洢�����н�ɫ�������ã�Ŀ����ȷ��ÿ�θ��¶���ȫ�µ����ã���������ò�����
            characterAttributeCustomisationList.Clear();
            //���޸ĺ���ֱ�����(armsCharacterAttribute)��ӵ����б���(��������λ����(characterPart)����ɫ����(partVariantColour)����������(partVariantType)����������(animationName)��
            characterAttributeCustomisationList.Add(armsCharacterAttribute);
            //�ܽ᣺1.Ϊ����ϵͳ׼��һ�������������ֱ�״̬����С���ü���2.������ApplyCharacterCustomisationParametersʱ��ϵͳ�������������б�ȷ������ض�����

            
            // Ӧ�ö�����������
            animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

            isCarrying = true;
        }
    }

    public Vector3 GetPlayerViewportPosition()
    {
      //Vector3 viewport  position for player ((0,0) viewport bottom left, (1,1) viewport top right
      //Vector3 ����ӿ�λ�ã�(0,0) Ϊ�ӿ����½ǣ�(1,1) Ϊ�ӿ����Ͻǣ�
      return mainCamera.WorldToViewportPoint(transform.position);
    }

}