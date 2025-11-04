
using System.Collections;
using UnityEngine;

public class Crop : MonoBehaviour
{
    [Tooltip("This should be populated from child transform gameobject showing harvest effect spawn point")]
    [SerializeField] private Transform harvestActionEffectTransform = null;

    private int harvestActionCount = 0;// 记录当前已经对该作物执行了多少次收获操作

    [Tooltip("This should be populated from child gameobject此处应从子游戏对象中填充数据")]
    [SerializeField] private SpriteRenderer cropHarvestedSpriteRenderer = null;

    [HideInInspector]
    public Vector2Int cropGridPosition;// 存储此作物在网格地图中的坐标位置

    // 处理工具对作物施加的操作，是作物交互的核心入口
    public void ProcessToolAction(ItemDetails equippedItemDetails, bool isToolRight, bool isToolLeft, bool isToolDown, bool isToolUp)
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

        // Get animator for crop if presen若有作物则触发动画器
        Animator animator = GetComponentInChildren<Animator>();
        // Trigger tool animation触发工具动画
        if (animator != null)
        {
            if (isToolRight || isToolUp)
            {
                animator.SetTrigger("usetoolright");
            }
            else if (isToolLeft || isToolDown)
            {
                animator.SetTrigger("usetoolleft");
            }
        }

        // Trigger tool particle effect on crop在作物上触发工具粒子效果
        if (cropDetails.isHarvestActionEffect)
        {
            EventHandler.CallHarvestActionEffectEvent(harvestActionEffectTransform.position, cropDetails.harvestActionEffect);
        }


        // Get required harvest actions for tool查询使用当前工具收获此作物总共需要多少次操作；如果返回-1，表示该工具无法用于收获此作物
        int requiredHarvestActions = cropDetails.RequiredHarvestActionsForTool(equippedItemDetails.itemCode);
        if (requiredHarvestActions == -1) return;//“return 语句会返回到调用该方法的代码所在的位置（相当于退出来），回到原来的执行轨道上继续前进。由于 private 方法只能被本类调用，所以它总是返回到本类；而 public 方法可以被任何类调用，所以它可能返回到本类，也可能返回到其他类

        // Increment harvest action count增加收获操作计数，表示本次成功执行了一次有效的收获操作
        harvestActionCount++;

        // Check if required harvest actions made检查当前的收获操作次数是否已经达到或超过了要求的总次数
        if (harvestActionCount >= requiredHarvestActions)
            HarvestCrop(isToolRight, isToolUp, cropDetails, gridPropertyDetails, animator);// 如果已达到要求，则执行最终的收获逻辑

    }

    // 执行最终的收获操作，清理网格数据并产生产物
    private void HarvestCrop(bool isUsingToolRight, bool isUsingToolUp, CropDetails cropDetails, GridPropertyDetails gridPropertyDetails, Animator animator)
    {
        // Is there a harvested animation是否存在已采集的动画？
        if (cropDetails.isHarvestedAnimation && animator != null)
        {
            // if harvest sprite then add to sprite renderer若有收获图片则添加至精灵渲染器
            if (cropDetails.harvestedSprite != null)
            {
                if (cropHarvestedSpriteRenderer != null)
                {
                    cropHarvestedSpriteRenderer.sprite = cropDetails.harvestedSprite;  // 一张图片
                }
            }

            if (isUsingToolRight || isUsingToolUp)
            {
                animator.SetTrigger("harvestright");
            }
            else
            {
                animator.SetTrigger("harvestleft");
            }
        }


        // Delete crop from grid properties将网格属性中的种子代码重置为-1（空），表示此格子上不再有作物
        gridPropertyDetails.seedItemCode = -1;
        gridPropertyDetails.growthDays = -1;// 重置生长天数为-1
        gridPropertyDetails.daysSinceLastHarvest = -1;// 重置上次收获后的天数为-1
        gridPropertyDetails.daysSinceWatered = -1;// 重置浇水后的天数为-1

        // Should the crop be hidden before the harvested animation是否应在收割动画播放前隐藏作物
        if (cropDetails.hideCropBeforeHarvestedAnimation)// 判断当前作物配置是否要求在播放收获动画前隐藏作物
        {
            GetComponentInChildren<SpriteRenderer>().enabled = false;// 如果配置为真，则禁用作物对象下所有子对象中的SpriteRenderer组件，使作物在场景中不可见
        }

        // Should box colliders be disabled before harvest在收获前是否应禁用盒子碰撞器
        if (cropDetails.disableCropCollidersBeforeHarvestedAnimation)
        {
            // Disable any box colliders禁用所有盒形碰撞体
            Collider2D[] collider2Ds = GetComponentsInChildren<Collider2D>();
            foreach (Collider2D collider2D in collider2Ds)
            {
                collider2D.enabled = false;
            }
        }

        // 将更新后的网格属性详情写回网格管理系统，使地图状态得以更新，通常是标记此地块已无作物
        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        // Is there a harvested animation - Destory this crop game object after animation completed是否存在收获动画 - 在动画完成后销毁该作物游戏对象
        if (cropDetails.isHarvestedAnimation && animator != null)// 检查当前作物是否有收获动画，并且附加的Animator组件不为空，确保动画能够播放
        {
            StartCoroutine(ProcessHarvestedActionsAfterAnimation(cropDetails, gridPropertyDetails, animator));// 如果满足上述条件，启动一个协程。该协程将等待收获动画播放完毕后，再执行实际的收获后处理操作
        }
        else
        {
            HarvestActions(cropDetails, gridPropertyDetails);// 如果作物没有收获动画，或者Animator组件为空，则立即调用收获行为方法，执行如生成产物、销毁对象等操作
        }

    }

    // 定义一个私有协程方法，用于在动画播放后处理收获行为。参数包括作物详情、网格属性和动画控制器
    private IEnumerator ProcessHarvestedActionsAfterAnimation(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails, Animator animator)
    {
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Harvested"))// 循环检查当前动画状态是否已经进入"Harvested"状态
        {
            yield return null;// 如果动画还没有播放到"Harvested"状态，就暂停协程直到下一帧再继续检查
        }
        // 当动画播放完毕后，调用实际的收获行为方法
        HarvestActions(cropDetails, gridPropertyDetails);
    }
    

    // 收获行为的具体实现，负责生成产物并销毁作物游戏对象
    private void HarvestActions(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails)
    {
        // 生成收获的产物（如小麦、胡萝卜等）
        SpawnHarvestedItems(cropDetails);

        // Does this crop transform into another crop这种作物会转化为另一种作物吗？
        if (cropDetails.harvestedTransformItemCode > 0)
        {
            CreateHarvestedTransformCrop(cropDetails, gridPropertyDetails);
        }


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


    private void CreateHarvestedTransformCrop(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails)
    {
        // Update crop in grid properties在网格属性中更新作物
        gridPropertyDetails.seedItemCode = cropDetails.harvestedTransformItemCode;
        gridPropertyDetails.growthDays = 0;
        gridPropertyDetails.daysSinceLastHarvest = -1;
        gridPropertyDetails.daysSinceWatered = -1;

        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        // Display planted crop展示种植作物
        GridPropertiesManager.Instance.DisplayPlantedCrop(gridPropertyDetails);

    }

}


