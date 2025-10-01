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
    private WaitForSeconds liftToolAnimationPause;

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
        toolCharacterAttribute = new CharacterAttribute(CharacterPartAnimator.tool, PartVariantColour.none, PartVariantType.none);

        // Initialise character attribute list��ʼ���ַ������б�
        characterAttributeCustomisationList = new List<CharacterAttribute>();

        //get reference to main camera��ȡ������ͷ������
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


                if (gridCursor.CursorIsEnabled|| cursor.CursorIsEnable)
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

                case ItemType.Watering_tool:
                case ItemType.Hoeing_tool:
                case ItemType.Reaping_tool:
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

    //������λ�ú����λ�ã�������ҳ���ķ�������
    private Vector3Int GetPlayerDirection(Vector3 cursorPosition, Vector3 playerPosition)
    {
        // ������Ƿ�������Ҳ���Y�����ݲΧ��
        if (
            cursorPosition.x > playerPosition.x// ���X����������X����
            &&
            cursorPosition.y < (playerPosition.y + cursor.ItemUseRadius / 2f)// Y����С������
            &&
            cursorPosition.y > (playerPosition.y - cursor.ItemUseRadius / 2f)// Y�����������
            )
        {
            return Vector3Int.right;// �������ҵĵ�λ����(1,0,0)
        }
        // ������Ƿ�����������Y�����ݲΧ��
        else if (
            cursorPosition.x < playerPosition.x// ���X����С�����X����
            &&
            cursorPosition.y < (playerPosition.y + cursor.ItemUseRadius / 2f)// Y����С������
            &&
            cursorPosition.y > (playerPosition.y - cursor.ItemUseRadius / 2f)// Y�����������
            )
        {
            return Vector3Int.left;// ��������ĵ�λ����(-1,0,0)
        }
        // ������Ƿ�������Ϸ���������X�ᣩ
        else if (cursorPosition.y > playerPosition.y)// ���Ƚ�Y����
        {
            return Vector3Int.up;// �������ϵĵ�λ����(0,1,0)
        }
        else
        {
            return Vector3Int.down;// �������µĵ�λ����(0,-1,0)
        }
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

    private void WaterGroundAtCursor(GridPropertyDetails gridPropertyDetails, Vector3Int playerDirection)
    {
        // Trigger animation��������
        StartCoroutine(WaterGroundAtCursorRoutine(playerDirection, gridPropertyDetails));

    }

    private IEnumerator WaterGroundAtCursorRoutine(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails)
    {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        // Set tool animation to watering can in override animation�����߶�������Ϊ��ˮ�������Ƕ�����
        toolCharacterAttribute.partVariantType = PartVariantType.wateringCan;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        //TODO: If there is water in the watering can����������������ˮ
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

        // Set Grid property details for watered ground���ù�ȵؿ��������������
        if (gridPropertyDetails.daysSinceWatered == -1)
        {
            gridPropertyDetails.daysSinceWatered = 0;
        }

        // Set grid property to watered��������������Ϊ��ˮ״̬
        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        // Display watered grid tiles��ʾ�������ߵ�ˮ�մ�ש
        GridPropertiesManager.Instance.DisplayWateredGround(gridPropertyDetails);

        // After animation pause������ͣ��
        yield return afterLiftToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;

    }

    private void ReapInPlayerDirectionAtCursor(ItemDetails itemDetails, Vector3Int playerDirection)
    {
        // StartCoroutine:����Э��ִ���ո����̣������������̣߳�
        StartCoroutine(ReapInPlayerDirectionAtCursorRoutine(itemDetails, playerDirection));
    }

    private IEnumerator ReapInPlayerDirectionAtCursorRoutine(ItemDetails itemDetails, Vector3Int playerDirection)
    {
        // ��������ƶ�����
        PlayerInputIsDisabled = true;
        // ���ù���ʹ�ã���ֹ�ظ�������
        playerToolUseDisabled = true;

        // Set tool animation to scythe in override animation�����߶�������Ϊ�����ĸ��Ƕ���
        // ���ý�ɫ��������
        toolCharacterAttribute.partVariantType = PartVariantType.scythe;// ���趨Ϊ��������������������ָ��Ŀ�꣬�ᵼ�¶��ݵ���״̬�ڣ����ܳ���T-Pose��˸����
        characterAttributeCustomisationList.Clear();// ���ԭ�ж�������
        characterAttributeCustomisationList.Add(toolCharacterAttribute);// �����������
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);// Ӧ�ö������ǣ�������Ч��

        // Reap in player direction����ҷ����ո�
        // ִ��ʵ���ո��߼�
        UseToolInPlayerDirection(itemDetails, playerDirection);

        // �ȴ�������ɣ�useToolAnimationPause��Ԥ����ĵȴ�ʱ�䣩
        yield return useToolAnimationPause;

        // �ָ���ҿ���
        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    // ʵ��ִ�й���ʹ�õĺ��ķ���
    private void UseToolInPlayerDirection(ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        // �����������������״̬
        if (Input.GetMouseButton(0))
        {
            // ���ݹ�������ִ�в�ͬ�߼�
            switch (equippedItemDetails.itemType)
            {
                case ItemType.Reaping_tool:// �ո�ߴ����֧
                    // ���ݷ������ö�Ӧ�Ķ�����־λ
                    if (playerDirection == Vector3Int.right)
                    {
                        isSwingingToolRight = true;// �����һӶ���
                    }
                    else if (playerDirection == Vector3Int.left)
                    {
                        isSwingingToolLeft = true;// ������Ӷ���
                    }
                    else if (playerDirection == Vector3Int.up)
                    {
                        isSwingingToolUp = true;// �����ϻӶ���
                    }
                    else if (playerDirection == Vector3Int.down)
                    {
                        isSwingingToolDown = true;// �����»Ӷ���
                    }
                    break;
            }

            // Define centre point of square which will be used for collision testing����������ײ�������������ĵ�
            // ��ײ����������
            // ������ĵ�X���꣨���λ��+����ƫ�ƣ�����ҽ�ɫλ��(0,0)�����һӶ����������ð뾶2��λ��ʱ��������Ľ�����Ϊ(1,0)�������γɵ�2x2��λ����������������Ҳ�2��λ��Χ���ֱ��������Ч��⣩
            Vector2 point = new Vector2(GetPlayerCentrePosition().x + playerDirection.x * (equippedItemDetails.itemUseRadius / 2f),
                GetPlayerCentrePosition().y + playerDirection.y * (equippedItemDetails.itemUseRadius / 2f));// ������ĵ�Y����

            // Define size of the square which will be used for collision testing����������ײ���������γߴ�
            Vector2 size = new Vector2(equippedItemDetails.itemUseRadius, equippedItemDetails.itemUseRadius);// ��������С�������Σ�

            // Get Item components with 2D collider located in the square at the centre point defined (2d colliders tested limited to maxCollidersToTestPerReapSwing)
            //��ȡλ�ڶ������ĵ������������ڵġ�����2D��ײ������Ʒ��������Ե�2D��ײ������������ÿ���ո�ڶ��ɲ��Ե������ײ��������
            Item[] itemArray = HelperMethods.GetComponentsAtBoxLocationNonAlloc<Item>(Settings.maxCollidersToTestPerReapSwing, point, size, 0f);

            int reapableItemCount = 0;// ���ո���Ʒ������

            // Loop through all items retrieved���������⵽����Ʒ�������޸ļ������⣩
            for (int i = itemArray.Length - 1; i >= 0; i--)
            {
                if (itemArray[i] != null)
                {
                    // Destory item game object if reapable��֤��Ʒ�����Ƿ���ո�
                    if (InventoryManager.Instance.GetItemDetails(itemArray[i].ItemCode).itemType == ItemType.Reapable_scenary)
                    {
                        // Effect position������Ч����λ�ã���Ʒ���ĵ��Ϸ����
                        Vector3 effectPosition = new Vector3(itemArray[i].transform.position.x, itemArray[i].transform.position.y + Settings.gridCellSize / 2f,
                            itemArray[i].transform.position.z);

                        // Trigger reaping effect �����ո�Ч�� 
                        EventHandler.CallHarvestActionEffectEvent(effectPosition, HarvestActionEffect.reaping);

                        // ִ���ո����
                        Destroy(itemArray[i].gameObject);// ������Ϸ����
                        reapableItemCount++;// ���Ӽ�����

                        // �ﵽ��������ո���������ֹ
                        if (reapableItemCount >= Settings.maxTargetComponentsToDestroyPerReapSwing)
                            break;
                    }
                }
            }


        }
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

    //��ȡ����������ĵ㣨�ӽ���Ų���������ģ�
    public Vector3 GetPlayerCentrePosition()
    {
        return new Vector3(transform.position.x, transform.position.y + Settings.playerCentreYOffset, transform.position.z);

    }
}