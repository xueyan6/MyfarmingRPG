
using System.Collections;
using UnityEngine;

public class VFXManager : SingletonMonobehaviour<VFXManager>
{
    private WaitForSeconds twoSeconds;// 2秒等待计时器
    [SerializeField] private GameObject reapingPrefab = null;// 收割特效预制体
    [SerializeField] private GameObject deciduousLeavesFallingPrefab = null;//落叶树叶飘零预制件

    protected override void Awake()
    {
        base.Awake();

        twoSeconds = new WaitForSeconds(2f);// 初始化计时器
    }

    private void OnDisable()
    {
        EventHandler.HarvestActionEffectEvent -= displayHarvestActionEffect;
    }

    private void OnEnable()
    {
        EventHandler.HarvestActionEffectEvent += displayHarvestActionEffect;
    }

    private IEnumerator DisableHarvestActionEffect(GameObject effectGameObject, WaitForSeconds secondsToWait)// 协程方法：延迟关闭特效对象
    {
        yield return secondsToWait;// 等待指定时长
        effectGameObject.SetActive(false);// 禁用特效游戏对象
    }

    private void displayHarvestActionEffect(Vector3 effectPosition, HarvestActionEffect harvestActionEffect) // 处理特效显示的主方法
    {
        switch (harvestActionEffect)// 根据特效类型分支处理
        {
            case HarvestActionEffect.deciduousLeavesFalling:
                GameObject deciduousLeavesFalling = PoolManager.Instance.ReuseObject(deciduousLeavesFallingPrefab, effectPosition, Quaternion.identity);
                deciduousLeavesFalling.SetActive(true);
                StartCoroutine(DisableHarvestActionEffect(deciduousLeavesFalling, twoSeconds));
                break;

            case HarvestActionEffect.reaping:// 收割特效
                GameObject reaping = PoolManager.Instance.ReuseObject(reapingPrefab, effectPosition, Quaternion.identity); // 从对象池获取特效实例
                reaping.SetActive(true);// 激活特效对象
                StartCoroutine(DisableHarvestActionEffect(reaping, twoSeconds)); // 启动协程2秒后自动关闭
                break;

            case HarvestActionEffect.none:
                break;

            default:
                break;
        }
    }
}