using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(Rigidbody2D))] // 强制要求Rigidbody2D组件，用于物理运动控制
[RequireComponent(typeof(Animator))] // 强制要求Animator组件，用于动画状态管理
[RequireComponent(typeof(NPCPath))] // 强制要求NPCPath组件，用于路径规划
[RequireComponent(typeof(SpriteRenderer))] // 强制要求SpriteRenderer组件，用于精灵渲染
[RequireComponent(typeof(BoxCollider2D))] // 强制要求BoxCollider2D组件，用于碰撞检测
public class NPCMovement : MonoBehaviour
{
    // 私有字段 - 内部状态管理
    [HideInInspector] public SceneName npcCurrentScene; // NPC当前所在场景
    [HideInInspector] public SceneName npcTargetScene; // NPC目标场景
    [HideInInspector] public Vector3Int npcCurrentGridPosition; // NPC当前网格坐标
    [HideInInspector] public Vector3Int npcTargetGridPosition; // NPC目标网格坐标
    [HideInInspector] public Vector3 npcTargetWorldPosition; // NPC目标世界坐标
    [HideInInspector] public Direction npcFacingDirectionAtDestination; // NPC到达目的地时的朝向
                                                                        
    private SceneName npcPreviousMovementStepScene; // 上一步移动的场景
    private Vector3Int npcNextGridPosition; // 下一个网格坐标
    private Vector3 npcNextWorldPosition; // 下一个世界坐标

    [Header("NPC Movement")] 
    public float npcNormalSpeed = 2f; // NPC正常移动速度

    [SerializeField] private float npcMinSpeed = 1f; // NPC最小移动速度（可序列化）
    [SerializeField] private float npcMaxSpeed = 3f; // NPC最大移动速度（可序列化）
    private bool npcIsMoving = false; // NPC是否正在移动的标志

    [HideInInspector] public AnimationClip npcTargetAniamtionClip; // 到达终点时播放的动画剪辑

    [Header("NPC Animation")] // 动画相关设置分组
    [SerializeField] private AnimationClip blankAnimation = null; // 空白动画剪辑，用于动画覆盖

    // 组件引用
    private Grid grid; // 网格系统引用
    private Rigidbody2D rigidBody2D; // 刚体组件引用
    private BoxCollider2D boxCollider2D; // 碰撞体组件引用
    private WaitForFixedUpdate waitForFixedUpdate; // 等待固定更新的对象
    private Animator animator; // 动画控制器引用
    private AnimatorOverrideController animatorOverrideController; // 动画覆盖控制器
    private int lastMoveAnimationParameter; // 上一次移动动画参数
    private NPCPath npcPath; // 路径规划组件引用
    private bool npcInitialised = false; // NPC是否已初始化的标志
    private SpriteRenderer spriteRenderer; // 精灵渲染器引用
    [HideInInspector] public bool npcActiveInScene = false; // NPC在场景中是否激活

    // 场景状态管理
    private bool sceneLoaded = false; // 场景是否已加载的标志
    private Coroutine moveToGridPositionRoutine; // 移动协程引用

    // 事件订阅方法
    private void OnEnable() // 当组件启用时调用
    {
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad; // 订阅场景加载后事件
        EventHandler.BeforeSceneUnloadEvent += BeforeSceneUnloaded; // 订阅场景卸载前事件
    }

    private void OnDisable() // 当组件禁用时调用
    {
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad; // 取消订阅场景加载后事件
        EventHandler.BeforeSceneUnloadEvent -= BeforeSceneUnloaded; // 取消订阅场景卸载前事件
    }

    private void Awake() 
    {
        rigidBody2D = GetComponent<Rigidbody2D>(); // 获取刚体组件
        boxCollider2D = GetComponent<BoxCollider2D>(); // 获取碰撞体组件
        animator = GetComponent<Animator>(); // 获取动画控制器
        npcPath = GetComponent<NPCPath>(); // 获取路径规划组件
        spriteRenderer = GetComponent<SpriteRenderer>(); // 获取精灵渲染器

        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController); // 创建动画覆盖控制器
        animator.runtimeAnimatorController = animatorOverrideController; // 设置动画控制器为覆盖版本

        // 初始化目标世界位置、目标网格位置和目标场景为当前位置
        npcTargetScene = npcCurrentScene;
        npcTargetGridPosition = npcCurrentGridPosition;
        npcTargetWorldPosition = transform.position;
    }

    private void Start() 
    {
        waitForFixedUpdate = new WaitForFixedUpdate(); // 创建等待固定更新对象

        SetIdleAnimation(); // 设置站立动画
    }

    private void FixedUpdate() // 固定时间间隔调用，用于物理更新
    {
        if (sceneLoaded) // 如果场景已加载
        {
            if (npcIsMoving == false) // 如果NPC不在移动状态
            {
                // 设置NPC当前和下一个网格位置 - 考虑到NPC可能正在播放动画
                npcCurrentGridPosition = GetGridPosition(transform.position);
                npcNextGridPosition = npcCurrentGridPosition;

                if (npcPath.npcMovementStepStack.Count > 0) // 如果路径栈中有移动步骤
                {
                    NPCMovementStep npcMovementStep = npcPath.npcMovementStepStack.Peek(); // 查看栈顶元素但不移除

                    npcCurrentScene = npcMovementStep.sceneName; // 更新当前场景

                    // if NPC is about to move to a new scene reset position to starting point in new scene and update the step times
                    // 若NPC即将移动至新场景，则将位置重置为新场景的起始点并更新步进时间
                    if (npcCurrentScene != npcPreviousMovementStepScene) // 检查NPC当前场景是否与上一步所在场景不同，即判断是否发生了场景切换
                    {
                        npcCurrentGridPosition = (Vector3Int)npcMovementStep.gridCoordinate; // 将NPC的当前网格位置设置为移动步骤中定义的网格坐标，并进行类型转换
                        npcNextGridPosition = npcCurrentGridPosition; // 将NPC的下一个网格位置也设置为当前位置，重置移动状态
                        transform.position = GetWorldPosition(npcCurrentGridPosition); // 将游戏对象的实际世界坐标设置为当前网格位置对应的世界坐标
                        npcPreviousMovementStepScene = npcCurrentScene; // 更新记录的上一步所在场景为当前场景，为下一次场景切换判断做准备
                        npcPath.UpdateTimesOnPath(); // 调用路径对象的更新方法，重新计算路径上的时间步进或相关时序数据
                    }


                    // 如果NPC在当前场景中，则设置NPC为激活状态使其可见，从栈中弹出移动步骤，然后调用移动NPC的方法
                    if (npcCurrentScene.ToString() == SceneManager.GetActiveScene().name) // 检查NPC当前所在场景是否与游戏当前激活场景相同
                    {
                        SetNPCActiveInScene(); // 激活NPC，使其在当前场景中可见并可交互

                        npcMovementStep = npcPath.npcMovementStepStack.Pop(); // 从NPC移动步骤栈中弹出最顶层的移动步骤

                        npcNextGridPosition = (Vector3Int)npcMovementStep.gridCoordinate; // 将弹出的移动步骤中的网格坐标转换为Vector3Int类型，并设置为NPC的下一个目标位置

                        TimeSpan npcMovementStepTime = new TimeSpan(npcMovementStep.hour, npcMovementStep.minute, npcMovementStep.second); // 使用移动步骤中的时、分、秒创建一个TimeSpan对象，表示该移动步骤的计划执行时间

                        MoveToGridPosition(npcNextGridPosition, npcMovementStepTime, TimeManager.Instance.GetGameTime()); // 调用移动方法，让NPC向目标网格位置移动，并传入移动步骤时间和当前游戏时间进行时序控制
                    }
                    // else if NPC is not in current scene then set NPC to inactive to make invisible
                    //否则如果NPC不在当前场景中，则将NPC设为非活动状态使其不可见
                    // once the movement step time is less than game time (in the past) then pop movement step off the stack and set NPC position to movement step position
                    //一旦移动步时间小于游戏时间（过去），则从栈中弹出移动步，并将NPC位置设置为移动步位置。
                    else // 如果NPC不在当前激活的场景中
                    {
                        SetNPCInactiveInScene(); // 将NPC设为非活动状态，使其在当前场景中不可见且不可交互

                        npcCurrentGridPosition = (Vector3Int)npcMovementStep.gridCoordinate; // 将NPC的当前网格位置设置为移动步骤中定义的网格坐标

                        npcNextGridPosition = npcCurrentGridPosition; // 将下一个网格位置也设置为当前位置，暂停移动状态

                        transform.position = GetWorldPosition(npcCurrentGridPosition); // 将游戏对象的实际世界坐标更新为当前网格位置对应的世界坐标

                        TimeSpan npcMovementStepTime = new TimeSpan(npcMovementStep.hour, npcMovementStep.minute, npcMovementStep.second); // 创建移动步骤时间的TimeSpan对象

                        TimeSpan gameTime = TimeManager.Instance.GetGameTime(); // 获取当前的游戏时间

                        if (npcMovementStepTime < gameTime) // 检查移动步骤的计划时间是否已经过去（小于当前游戏时间）
                        {
                            npcMovementStep = npcPath.npcMovementStepStack.Pop(); // 如果时间已过，从栈中弹出下一个移动步骤

                            npcCurrentGridPosition = (Vector3Int)npcMovementStep.gridCoordinate; // 更新NPC的当前网格位置为新弹出的移动步骤中的坐标

                            npcNextGridPosition = npcCurrentGridPosition; // 同步更新下一个网格位置

                            transform.position = GetWorldPosition(npcCurrentGridPosition); // 更新游戏对象的世界坐标，确保NPC在场景切换后位置正确
                        }
                    }


                }
                // 如果没有更多NPC移动步骤
                else
                {
                    ResetMoveAnimation(); // 重置移动动画

                    SetNPCFacingDirection(); // 设置NPC朝向

                    SetNPCEventAnimation(); // 设置NPC事件动画
                }
            }
        }
    }

    public void SetScheduleEventDetails(NPCScheduleEvent npcScheduleEvent) // 设置日程事件详情方法
    {
        npcTargetScene = npcScheduleEvent.toSceneName; // 设置目标场景
        npcTargetGridPosition = (Vector3Int)npcScheduleEvent.toGridCoordinate; // 设置目标网格坐标
        npcTargetWorldPosition = GetWorldPosition(npcTargetGridPosition); // 设置目标世界坐标
        npcFacingDirectionAtDestination = npcScheduleEvent.npcFacingDirectionAtDestination; // 设置目标朝向
        npcTargetAniamtionClip = npcScheduleEvent.animationAtDestination; // 设置目标动画剪辑
        ClearNPCEventAnimation(); // 清除NPC事件动画
    }

    private void SetNPCEventAnimation() // 设置NPC事件动画方法
    {
        if (npcTargetAniamtionClip != null) // 如果目标动画剪辑不为空
        {
            ResetIdleAnimation(); // 重置站立动画
            animatorOverrideController[blankAnimation] = npcTargetAniamtionClip; // 用目标动画覆盖空白动画
            animator.SetBool(Settings.eventAnimation, true); // 设置事件动画状态为true
        }
        else
        {
            animatorOverrideController[blankAnimation] = blankAnimation; // 保持空白动画不变
            animator.SetBool(Settings.eventAnimation, false); // 设置事件动画状态为false
        }
    }

    public void ClearNPCEventAnimation() // 清除NPC事件动画方法
    {
        animatorOverrideController[blankAnimation] = blankAnimation; // 恢复空白动画
        animator.SetBool(Settings.eventAnimation, false); // 设置事件动画状态为false

        // 清除NPC上的任何旋转
        transform.rotation = Quaternion.identity; // 重置旋转
    }

    private void SetNPCFacingDirection() // 设置NPC朝向方法
    {
        ResetIdleAnimation(); // 重置站立动画

        switch (npcFacingDirectionAtDestination) // 根据目标朝向设置对应动画
        {
            case Direction.Up: // 如果朝上
                animator.SetBool(Settings.idleUp, true); // 设置向上站立动画
                break;
            case Direction.Down: // 如果朝下
                animator.SetBool(Settings.idleDown, true); // 设置向下站立动画
                break;
            case Direction.Left: // 如果朝左
                animator.SetBool(Settings.idleLeft, true); // 设置向左站立动画
                break;
            case Direction.Right: // 如果朝右
                animator.SetBool(Settings.idleRight, true); // 设置向右站立动画
                break;

            case Direction.none: // 如果没有指定朝向
                break;

            default:
                break;
        }
    }

    public void SetNPCActiveInScene() // 设置NPC在场景中激活方法
    {
        spriteRenderer.enabled = true; // 启用精灵渲染器
        boxCollider2D.enabled = true; // 启用碰撞体
        npcActiveInScene = true; // 设置激活状态为true
    }

    public void SetNPCInactiveInScene() // 设置NPC在场景中禁用方法
    {
        spriteRenderer.enabled = false; // 禁用精灵渲染器
        boxCollider2D.enabled = false; // 禁用碰撞体
        npcActiveInScene = false; // 设置激活状态为false
    }

    private void AfterSceneLoad() // 场景加载后事件处理方法
    {
        grid = GameObject.FindObjectOfType<Grid>(); // 查找场景中的网格对象

        if (!npcInitialised) // 如果NPC未初始化
        {
            InitialisedNPC(); // 初始化NPC
            npcInitialised = true; // 设置初始化状态为true
        }

        sceneLoaded = true; // 设置场景加载状态为true
    }

    private void BeforeSceneUnloaded() // 场景卸载前事件处理方法
    {
        sceneLoaded = false; // 设置场景加载状态为false
    }

    
    private Vector3Int GetGridPosition(Vector3 worldPosition) // 获取网格位置方法
    {
        if (grid != null) // 如果网格存在
        {
            return grid.WorldToCell(worldPosition); // 转换世界坐标到网格坐标
        }
        else
        {
            return Vector3Int.zero; // 返回零向量
        }
    }

    public Vector3 GetWorldPosition(Vector3Int gridPosition) // 获取世界位置方法
    {
        Vector3 worldPosition = grid.CellToWorld(gridPosition); // 转换网格坐标到世界坐标

        // 获取网格方块中心
        return new Vector3(worldPosition.x + Settings.gridCellSize / 2f, worldPosition.y + Settings.gridCellSize / 2f, worldPosition.z); // 返回中心位置
    }

    public void CancelNPCMovement()
    {
        npcPath.ClearPath();
        npcNextGridPosition = Vector3Int.zero;
        npcNextWorldPosition = Vector3.zero;
        npcIsMoving = false;

        if (moveToGridPositionRoutine != null)
        {
            StopCoroutine(moveToGridPositionRoutine);
        }

        // Reset move animation
        ResetMoveAnimation();

        // Clear event animation
        ClearNPCEventAnimation();
        npcTargetAniamtionClip = null;

        // Reset idle animation
        ResetIdleAnimation();

        // Set idle animation
        SetIdleAnimation();
    }

    private void InitialisedNPC() // 初始化NPC方法
    {
        // 在场景中激活
        if (npcCurrentScene.ToString() == SceneManager.GetActiveScene().name) // 如果当前场景是活动场景
        {
            SetNPCActiveInScene(); // 激活NPC
        }
        else
        {
            SetNPCInactiveInScene(); // 禁用NPC
        }

        npcPreviousMovementStepScene = npcCurrentScene;

        // 获取NPC当前网格位置
        npcCurrentGridPosition = GetGridPosition(transform.position);

        // 设置下一个网格位置和目标网格位置为当前网格位置
        npcNextGridPosition = npcCurrentGridPosition;
        npcTargetGridPosition = npcCurrentGridPosition;
        npcTargetWorldPosition = GetWorldPosition(npcTargetGridPosition);

        // 获取NPC世界位置
        npcNextWorldPosition = GetWorldPosition(npcCurrentGridPosition);
    }

    private void MoveToGridPosition(Vector3Int gridPosition, TimeSpan npcMovementStepTime, TimeSpan gameTime) // 移动到网格位置方法
    {
        moveToGridPositionRoutine = StartCoroutine(MoveToGridPositionRoutine(gridPosition, npcMovementStepTime, gameTime)); // 启动移动协程
    }

    private IEnumerator MoveToGridPositionRoutine(Vector3Int gridPosition, TimeSpan npcMovementStepTime, TimeSpan gameTime) // 移动协程
    {
        npcIsMoving = true; // 设置移动状态为true

        SetMoveAnimation(gridPosition); // 设置移动动画

        npcNextWorldPosition = GetWorldPosition(gridPosition); // 获取下一个世界位置

        // 如果移动步骤时间在未来，否则跳过并立即将NPC移动到位置
        if (npcMovementStepTime > gameTime)
        {
            // 计算时间差（秒）
            float timeToMove = (float)(npcMovementStepTime.TotalSeconds - gameTime.TotalSeconds);

            // 计算速度（确保速度至少为最小速度）
            float npcCalculatedSpeed =Mathf.Max(npcMinSpeed, Vector3.Distance(transform.position, npcNextWorldPosition) / timeToMove / Settings.secondsPerGameSecond);

            // 如果速度小于NPC最大速度，则处理，否则跳过并立即将NPC移动到位置
            if (npcCalculatedSpeed  <= npcMaxSpeed)
            {
                while (Vector3.Distance(transform.position, npcNextWorldPosition) > Settings.pixelSize) // 当距离大于像素大小时循环
                {
                    Vector3 unitVector = Vector3.Normalize(npcNextWorldPosition - transform.position); // 计算单位向量
                    Vector2 move = new Vector2(unitVector.x * npcCalculatedSpeed * Time.fixedDeltaTime, unitVector.y * npcCalculatedSpeed * Time.fixedDeltaTime); // 计算移动向量

                    rigidBody2D.MovePosition(rigidBody2D.position + move); // 移动刚体位置

                    yield return waitForFixedUpdate; // 等待下一个固定更新
                }
            }

            Debug.Log("here!"); // 调试输出
        }
    
        rigidBody2D.position = npcNextWorldPosition; // 设置刚体位置到目标位置
        npcCurrentGridPosition = gridPosition; // 更新当前网格位置
        npcNextGridPosition = npcCurrentGridPosition; // 更新下一个网格位置
        npcIsMoving = false; // 设置移动状态为false
    }

    private void SetMoveAnimation(Vector3Int gridPosition) // 设置移动动画方法
    {
        // 重置站立动画
        ResetIdleAnimation();

        // 重置移动动画
        ResetMoveAnimation();

        // 获取世界位置
        Vector3 toWorldPosition = GetWorldPosition(gridPosition);

        // 获取方向向量
        Vector3 directionVector = toWorldPosition - transform.position;

        if (Mathf.Abs(directionVector.x) >= Mathf.Abs(directionVector.y)) // 如果水平移动距离大于垂直移动距离
        {
            // 使用左/右动画
            if (directionVector.x > 0) // 如果向右移动
            {
                animator.SetBool(Settings.walkRight, true); // 设置向右行走动画
            }
            else // 如果向左移动
            {
                animator.SetBool(Settings.walkLeft, true); // 设置向左行走动画
            }
        }
        else // 如果垂直移动距离大于水平移动距离
        {
            // 使用上/下动画
            if (directionVector.y > 0) // 如果向上移动
            {
                animator.SetBool(Settings.walkUp, true); // 设置向上行走动画
            }
            else // 如果向下移动
            {
                animator.SetBool(Settings.walkDown, true); // 设置向下行走动画
            }
        }
    }

    private void SetIdleAnimation() // 设置站立动画方法
    {
        animator.SetBool(Settings.idleDown, true); // 设置向下站立动画状态为true
    }

    private void ResetMoveAnimation() // 重置移动动画方法
    {
        animator.SetBool(Settings.walkRight, false); // 重置向右行走动画
        animator.SetBool(Settings.walkLeft, false); // 重置向左行走动画
        animator.SetBool(Settings.walkUp, false); // 重置向上行走动画
        animator.SetBool(Settings.walkDown, false); // 重置向下行走动画
    }

    private void ResetIdleAnimation() // 重置站立动画方法
    {
        animator.SetBool(Settings.idleRight, false); // 重置向右站立动画
        animator.SetBool(Settings.idleLeft, false); // 重置向左站立动画
        animator.SetBool(Settings.idleUp, false); // 重置向上站立动画
        animator.SetBool(Settings.idleDown, false); // 重置向下站立动画
    }

    //一、NPC接受移动指令流程
    //1. 外部指令接收（SetScheduleEventDetails 方法）
    //这是整个系统的配置入口，负责接收外部日程事件数据

    //二、NPC开始移动流程
    //1. 路径规划触发（FixedUpdate 方法）
    //2. 移动协程执行（MoveToGridPositionRoutine 方法）

    //三、移动过程中的关键处理
    //1. 时间同步与速度计算（MoveToGridPositionRoutine）
    //2. 动画同步处理（SetMoveAnimation）

    //四、NPC结束移动流程
    //1. 位置精确定位（MoveToGridPositionRoutine）
    //2. 最终状态设置（FixedUpdate的else分支）

    //五、重置与清理流程
    //1. 动画状态重置（ResetMoveAnimation、ResetIdleAnimation）
    //2. 事件动画清理（ClearNPCEventAnimation 方法）

    //六、场景生命周期管理
    //1. 场景加载后处理（AfterSceneLoad 方法）
    //2. 场景卸载前处理（BeforeSceneUnloaded 方法）

    //完整状态流转图
    //[初始化] → [等待指令] → [接收日程事件] → [路径规划]
    //    ↓
    //[场景加载] → [检查移动条件] → [启动移动协程]
    //    ↓
    //[移动中] → [动画同步] → [物理移动] → [到达目标]
    //    ↓
    //[重置动画] → [设置朝向] → [播放事件动画] → [等待指令]

}