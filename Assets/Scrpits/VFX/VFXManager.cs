
using System.Collections;
using UnityEngine;

public class VFXManager : SingletonMonobehaviour<VFXManager>
{
    private WaitForSeconds twoSeconds;// 2��ȴ���ʱ��
    [SerializeField] private GameObject reapingPrefab = null;// �ո���ЧԤ����

    protected override void Awake()
    {
        base.Awake();

        twoSeconds = new WaitForSeconds(2f);// ��ʼ����ʱ��
    }

    private void OnDisable()
    {
        EventHandler.HarvestActionEffectEvent -= displayHarvestActionEffect;
    }

    private void OnEnable()
    {
        EventHandler.HarvestActionEffectEvent += displayHarvestActionEffect;
    }

    private IEnumerator DisableHarvestActionEffect(GameObject effectGameObject, WaitForSeconds secondsToWait)// Э�̷������ӳٹر���Ч����
    {
        yield return secondsToWait;// �ȴ�ָ��ʱ��
        effectGameObject.SetActive(false);// ������Ч��Ϸ����
    }

    private void displayHarvestActionEffect(Vector3 effectPosition, HarvestActionEffect harvestActionEffect) // ������Ч��ʾ��������
    {
        switch (harvestActionEffect)// ������Ч���ͷ�֧����
        {
            case HarvestActionEffect.reaping:// �ո���Ч
                GameObject reaping = PoolManager.Instance.ReuseObject(reapingPrefab, effectPosition, Quaternion.identity); // �Ӷ���ػ�ȡ��Чʵ��
                reaping.SetActive(true);// ������Ч����
                StartCoroutine(DisableHarvestActionEffect(reaping, twoSeconds)); // ����Э��2����Զ��ر�
                break;

            case HarvestActionEffect.none:
                break;

            default:
                break;
        }
    }
}