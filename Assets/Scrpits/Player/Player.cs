
using UnityEngine;


public class Player : SingletonMonobehaviour<Player>
{
    //Movement Parameters
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
    private ToolEffect toolEffect = ToolEffect.None;

    private Rigidbody2D Rigidbody2D;

    private Direction PlayerDirection;

    private float MovementSpeed;

    private bool _playerInputIsDisable = false;
    public bool PlayerInputIsEnable
    {
        get => _playerInputIsDisable; set => _playerInputIsDisable = value;
    }

    protected override void Awake()
    {
        base.Awake();
        Rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        #region Player Input

        ResetAnimationTrigger();
        PlayerMovementInput();
        PlayerWalkInput();

        //Send event to any listeners for player movement input
        EventHandler.CallMovementEvent(xInput, yInput,
                isWalking, isRunning, isIdle, isCarrying,
                toolEffect, 
                isUsingToolRight, isUsingToolLeft, isUsingToolUp, isUsingToolDown,
                isLiftingToolRight, isLiftingToolLeft, isLiftingToolUp, isLiftingToolDown,
                isPickingRight, isPickingLeft, isPickingUp, isPickingDown,
                isSwingingToolRight, isSwingingToolLeft, isSwingingToolUp, isSwingingToolDown
                ,false,false,false,false);

        #endregion
    }

    private void FixedUpdate()
    {
        PlayerMovement();
    }

    private void PlayerMovement()
    {
        Vector2 move=new Vector2(xInput*MovementSpeed*Time.deltaTime,yInput*MovementSpeed*Time.deltaTime);
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
            xInput =xInput * 0.71f;
            yInput = yInput * 0.71f;
        }

        if(xInput != 0 ||yInput != 0)
        {
            isRunning = true;
            isWalking = false;
            isIdle = false;
            MovementSpeed = Settings.runingSpeed;

            //Capture player direction for save game
            if (xInput < 0)
            {
                PlayerDirection=Direction.Left;
            }
            else if (xInput >0)
            {
                PlayerDirection=Direction.Right;
            }
            else if (yInput < 0)
            {
                PlayerDirection=Direction.Down;
            }
            else 
            {
                PlayerDirection=Direction.Up;
            }
        }
        else if(xInput==0 &&yInput==0)
        {
            isRunning=false;
            isWalking=false;
            isIdle=true;
        }
    }

    private void PlayerWalkInput()
    {
        if(Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift))
        {
            isRunning=false ;
            isWalking=true;
            isIdle = false;
            MovementSpeed=Settings.walkingSpeed;
        }
        else
        {
            isRunning = true;
            isWalking = false;
            isIdle = false;
            MovementSpeed = Settings.runingSpeed;
        }
    }


}