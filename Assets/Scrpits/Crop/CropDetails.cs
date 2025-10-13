
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CropDetails
{
    [ItemCodeDescription]
    public int seedItemCode; // this is the item code for the corresponding seed���Ƕ�Ӧ���ӵ���Ʒ����
    public int[] growthDays; // days growth for each stageÿ���׶ε���������
    public int totalGrowthDays; // total growth days����������
    public GameObject[] growthPrefab; // prefab to use when instantiating growth stages����ʵ�����ɳ��׶ε�Ԥ�Ƽ�
    public Sprite[] growthSprite; // growth sprite����ͼƬ
    public Season[] seasons; // growth seasons��������
    public Sprite harvestedSprite; // sprite used once harvested�ջ��ͼƬ

    [ItemCodeDescription]
    public int harvestedTransformItemCode; // if the item transform into another item when harvested this item code will be populated������Ʒ���ջ�ʱ��ת��Ϊ��һ��Ʒ�������Ʒ���뽫����䡣
    public bool hideCropBeforeHarvestedAnimation; // the crop should be disabled before the harvested animation���ո����ǰ��������
    public bool disableCropCollidersBeforeHarvestedAnimation; // if colliders on crop should be disabled to avoid the harvested animation effecting any other game objects
                                                              //�Ƿ�Ӧ����������ײ�����Ա����ո��Ӱ��������Ϸ����
    public bool isHarvestedAnimation; // true if harvested animation to be played on final growth stage prefab������������׶�Ԥ�Ƽ���Ҫ�����ջ񶯻�����Ϊ��
    public bool isHarvestActionEffect = false; // flag to determine whether there is a harvest action effect��־�����ж��Ƿ�����ջ���Ч��
    public bool spawnCropProducedAtPlayerPosition;//�����λ����������
    public HarvestActionEffect harvestActionEffect; // the harvest action effect for the crop�����ջ���Ч��

    [ItemCodeDescription]
    public int[] harvestToolItemCode; // array of item codes for the tools that can harvest or 0 array elements if no tool required�����ڲ��յĹ����������飬�����蹤���򷵻�0������Ԫ��
    public int[] requiredHarvestActions; // number of harvest actions required for corresponding tool in harvest tool item code array��Ӧ��������Ĳ��ղ������������չ�����Ŀ�������飩

    [ItemCodeDescription]
    public int[] cropProducedItemCode; // array of item codes produced for the harvested crop�ջ���������������Ŀ��������
    public int[] cropProducedMinQuantity; // array of minimum quantities produced for the harvested crop�ջ�������Ͳ�������
    public int[] cropProducedMaxQuantity; // if max quantity is > min quantity then a random number of crops between min and max are produced��������������С���������������������Сֵ�����ֵ֮�������������
    public int daysToRegrow; // days to regrow next crop or -1 if a single crop��Ҫ���������������һ�������һ����ʱΪ-1

    // returns true if the tool item code can be used to harvest this crop, else returns false���������Ŀ����������ջ������򷵻� true�����򷵻� false
    public bool CanUseToolToHarvestCrop(int toolItemCode)
    {
        if (RequiredHarvestActionsForTool(toolItemCode) == -1)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    // returns -1 if the tool can't be used to harvest this crop, else returns thhe number of harvest actions required by this tool
    //���ù����޷������ջ������򷵻�-1�����򷵻ظù���������ջ����������
    public int RequiredHarvestActionsForTool(int toolItemCode)
    {
        for (int i = 0; i < harvestToolItemCode.Length; i++)
        {
            if (harvestToolItemCode[i] == toolItemCode)
            {
                return requiredHarvestActions[i];
            }
        }
        return -1;
    }
}