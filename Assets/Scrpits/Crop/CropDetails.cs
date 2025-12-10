
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CropDetails
{
    [ItemCodeDescription]
    public int seedItemCode; // this is the item code for the corresponding seed这是对应种子的商品编码
    public int[] growthDays; // days growth for each stage每个阶段的生长天数
    public GameObject[] growthPrefab; // prefab to use when instantiating growth stages用于实例化成长阶段的预制件
    public Sprite[] growthSprite; // growth sprite生长图片
    public Season[] seasons; // growth seasons生长季节
    public Sprite harvestedSprite; // sprite used once harvested收获后图片

    [ItemCodeDescription]
    public int harvestedTransformItemCode; // if the item transform into another item when harvested this item code will be populated若该物品在收获时会转化为另一物品，则此物品代码将被填充。
    public bool hideCropBeforeHarvestedAnimation; // the crop should be disabled before the harvested animation在收割动画前让作物消失
    public bool disableCropCollidersBeforeHarvestedAnimation; // if colliders on crop should be disabled to avoid the harvested animation effecting any other game objects
                                                              //是否应禁用作物碰撞器，以避免收割动画影响其他游戏对象
    public bool isHarvestedAnimation; // true if harvested animation to be played on final growth stage prefab如果最终生长阶段预制件需要播放收获动画，则为真
    public bool isHarvestActionEffect = false; // flag to determine whether there is a harvest action effect标志用于判断是否存在收获动作效果
    public bool spawnCropProducedAtPlayerPosition;//在玩家位置生成作物
    public HarvestActionEffect harvestActionEffect; // the harvest action effect for the crop作物收获动作效果
    public SoundName harvestSound;//农作物收获声音

    [ItemCodeDescription]
    public int[] harvestToolItemCode; // array of item codes for the tools that can harvest or 0 array elements if no tool required可用于采收的工具项码数组，若无需工具则返回0个数组元素
    public int[] requiredHarvestActions; // number of harvest actions required for corresponding tool in harvest tool item code array对应工具所需的采收操作次数（采收工具项目代码数组）

    [ItemCodeDescription]
    public int[] cropProducedItemCode; // array of item codes produced for the harvested crop收获作物所产生的项目代码数组
    public int[] cropProducedMinQuantity; // array of minimum quantities produced for the harvested crop收获作物最低产量数组
    public int[] cropProducedMaxQuantity; // if max quantity is > min quantity then a random number of crops between min and max are produced若最大产量大于最小产量，则随机生产介于最小值与最大值之间的作物数量。
    public int daysToRegrow; // days to regrow next crop or -1 if a single crop需要数天才能再生长下一茬作物，或单一作物时为-1

    // returns true if the tool item code can be used to harvest this crop, else returns false如果工具项目代码可用于收获此作物，则返回 true，否则返回 false
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
    //若该工具无法用于收获此作物，则返回-1；否则返回该工具所需的收获操作次数。
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