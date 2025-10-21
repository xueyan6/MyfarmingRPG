
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Crop : MonoBehaviour
{
    private int harvestActionCount = 0;// 记录当前已经对该作物执行了多少次收获操作

    [HideInInspector]
    public Vector2Int cropGridPosition;// 存储此作物在网格地图中的坐标位置

    // 处理工具对作物施加的操作，是作物交互的核心入口
    public void ProcessToolAction(ItemDetails equippedItemDetails)
    {
        // Get grid property details获取网格属性详情
        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cropGridPosition.x, cropGridPosition.y);

        if (gridPropertyDetails == null)
            return;

        // Get seed item details获取种子项详情
        ItemDetails seedItemDetails = InventoryManager.Instance.GetItemDetails(gridPropertyDetails.seedItemCode);
        if (seedItemDetails == null) return;

        // 据种子物品代码，获取该种作物对应的生长配置详情（如不同生长阶段的Sprite、收获所需次数等）
        CropDetails cropDetails = GridPropertiesManager.Instance.GetCropDetails(seedItemDetails.itemCode);
        if (cropDetails == null) return;

        // Get required harvest actions for tool查询使用当前工具收获此作物总共需要多少次操作；如果返回-1，表示该工具无法用于收获此作物
        int requiredHarvestActions = cropDetails.RequiredHarvestActionsForTool(equippedItemDetails.itemCode);
        if (requiredHarvestActions == -1) return;//“return 语句会返回到调用该方法的代码所在的位置（相当于退出来），回到原来的执行轨道上继续前进。由于 private 方法只能被本类调用，所以它总是返回到本类；而 public 方法可以被任何类调用，所以它可能返回到本类，也可能返回到其他类

        // Increment harvest action count增加收获操作计数，表示本次成功执行了一次有效的收获操作
        harvestActionCount++;

        // Check if required harvest actions made检查当前的收获操作次数是否已经达到或超过了要求的总次数
        if (harvestActionCount >= requiredHarvestActions)
            HarvestCrop(cropDetails, gridPropertyDetails);// 如果已达到要求，则执行最终的收获逻辑

    }

    // 执行最终的收获操作，清理网格数据并产生产物
    private void HarvestCrop(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails)
    {
        // Delete crop from grid properties将网格属性中的种子代码重置为-1（空），表示此格子上不再有作物
        gridPropertyDetails.seedItemCode = -1;
        gridPropertyDetails.growthDays = -1;// 重置生长天数为-1
        gridPropertyDetails.daysSinceLastHarvest = -1;// 重置上次收获后的天数为-1
        gridPropertyDetails.daysSinceWatered = -1;// 重置浇水后的天数为-1

        // 将更新后的网格属性详情写回网格管理系统，使地图状态得以更新
        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        // 执行收获的具体行为，如生成产物和销毁对象
        HarvestActions(cropDetails, gridPropertyDetails);
    }

    // 收获行为的具体实现，负责生成产物并销毁作物游戏对象
    private void HarvestActions(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails)
    {
        // 生成收获的产物（如小麦、胡萝卜等）
        SpawnHarvestedItems(cropDetails);

        // 从游戏场景中销毁（删除）这个作物游戏对象
        Destroy(gameObject);  
    }

    // 生成作物被收获后产出的物品
    private void SpawnHarvestedItems(CropDetails cropDetails)
    {
        // Spawn the item(s) to be produced遍历作物配置中定义的所有可产出物品的代码数组
        for (int i = 0; i < cropDetails.cropProducedItemCode.Length; i++)
        {
            int cropsToProduce;

            // Calculate how many crops to produce计算需要生产多少作物
            if (cropDetails.cropProducedMinQuantity[i] == cropDetails.cropProducedMaxQuantity[i] ||
                cropDetails.cropProducedMaxQuantity[i] < cropDetails.cropProducedMinQuantity[i])
            {
                //如果最小产量等于最大产量，或配置错误（最大小于最小），则直接使用最小产量
                cropsToProduce = cropDetails.cropProducedMinQuantity[i];
            }
            else
            {
                //在最小产量和最大产量之间（含最小值，不含最大值+1）随机一个数量
                cropsToProduce = Random.Range(cropDetails.cropProducedMinQuantity[i], cropDetails.cropProducedMaxQuantity[i] + 1);
            }

            // 根据计算出的数量cropsToProduce，循环生成每一个物品
            for (int j = 0; j < cropsToProduce; j++)
            {
                Vector3 spawnPosition;
                if (cropDetails.spawnCropProducedAtPlayerPosition)
                {
                    // Add item to the players inventory如果配置为将产物直接发送到玩家位置，则调用物品管理器将物品添加到玩家库存中
                    InventoryManager.Instance.AddItem(InventoryLocation.player, cropDetails.cropProducedItemCode[i]);
                }
                else
                {
                    // Random position否则，在作物周围的随机位置生成场景物品
                    spawnPosition = new Vector3(transform.position.x + Random.Range(-1f, 1f), transform.position.y + Random.Range(-1f, 1f), 0f);
                    // 通过场景物品管理器在计算出的位置实例化一个场景物品（如掉在地上的小麦）
                    SceneItemsManager.Instance.InstantiateSceneItem(cropDetails.cropProducedItemCode[i], spawnPosition);
                }
            }
        }
    }

}


