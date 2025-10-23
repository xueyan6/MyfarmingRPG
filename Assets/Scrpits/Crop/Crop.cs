
using System.Collections;
using UnityEngine;

public class Crop : MonoBehaviour
{
    [Tooltip("This should be populated from child transform gameobject showing harvest effect spawn point")]
    [SerializeField] private Transform harvestActionEffectTransform = null;

    private int harvestActionCount = 0;// ��¼��ǰ�Ѿ��Ը�����ִ���˶��ٴ��ջ����

    [Tooltip("This should be populated from child gameobject�˴�Ӧ������Ϸ�������������")]
    [SerializeField] private SpriteRenderer cropHarvestedSpriteRenderer = null;

    [HideInInspector]
    public Vector2Int cropGridPosition;// �洢�������������ͼ�е�����λ��

    // �����߶�����ʩ�ӵĲ����������ｻ���ĺ������
    public void ProcessToolAction(ItemDetails equippedItemDetails, bool isToolRight, bool isToolLeft, bool isToolDown, bool isToolUp)
    {
        // Get grid property details��ȡ������������
        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cropGridPosition.x, cropGridPosition.y);

        if (gridPropertyDetails == null)
            return;

        // Get seed item details��ȡ����������
        ItemDetails seedItemDetails = InventoryManager.Instance.GetItemDetails(gridPropertyDetails.seedItemCode);
        if (seedItemDetails == null) return;

        // ��������Ʒ���룬��ȡ���������Ӧ�������������飨�粻ͬ�����׶ε�Sprite���ջ���������ȣ�
        CropDetails cropDetails = GridPropertiesManager.Instance.GetCropDetails(seedItemDetails.itemCode);
        if (cropDetails == null) return;

        // Get animator for crop if presen���������򴥷�������
        Animator animator = GetComponentInChildren<Animator>();
        // Trigger tool animation�������߶���
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

        // Trigger tool particle effect on crop�������ϴ�����������Ч��
        if (cropDetails.isHarvestActionEffect)
        {
            EventHandler.CallHarvestActionEffectEvent(harvestActionEffectTransform.position, cropDetails.harvestActionEffect);
        }


        // Get required harvest actions for tool��ѯʹ�õ�ǰ�����ջ�������ܹ���Ҫ���ٴβ������������-1����ʾ�ù����޷������ջ������
        int requiredHarvestActions = cropDetails.RequiredHarvestActionsForTool(equippedItemDetails.itemCode);
        if (requiredHarvestActions == -1) return;//��return ���᷵�ص����ø÷����Ĵ������ڵ�λ�ã��൱���˳��������ص�ԭ����ִ�й���ϼ���ǰ�������� private ����ֻ�ܱ�������ã����������Ƿ��ص����ࣻ�� public �������Ա��κ�����ã����������ܷ��ص����࣬Ҳ���ܷ��ص�������

        // Increment harvest action count�����ջ������������ʾ���γɹ�ִ����һ����Ч���ջ����
        harvestActionCount++;

        // Check if required harvest actions made��鵱ǰ���ջ���������Ƿ��Ѿ��ﵽ�򳬹���Ҫ����ܴ���
        if (harvestActionCount >= requiredHarvestActions)
            HarvestCrop(isToolRight, isToolUp, cropDetails, gridPropertyDetails, animator);// ����ѴﵽҪ����ִ�����յ��ջ��߼�

    }

    // ִ�����յ��ջ�����������������ݲ���������
    private void HarvestCrop(bool isUsingToolRight, bool isUsingToolUp, CropDetails cropDetails, GridPropertyDetails gridPropertyDetails, Animator animator)
    {
        // Is there a harvested animation�Ƿ�����Ѳɼ��Ķ�����
        if (cropDetails.isHarvestedAnimation && animator != null)
        {
            // if harvest sprite then add to sprite renderer�����ջ�ͼƬ�������������Ⱦ��
            if (cropDetails.harvestedSprite != null)
            {
                if (cropHarvestedSpriteRenderer != null)
                {
                    cropHarvestedSpriteRenderer.sprite = cropDetails.harvestedSprite;  // һ��ͼƬ
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


        // Delete crop from grid properties�����������е����Ӵ�������Ϊ-1���գ�����ʾ�˸����ϲ���������
        gridPropertyDetails.seedItemCode = -1;
        gridPropertyDetails.growthDays = -1;// ������������Ϊ-1
        gridPropertyDetails.daysSinceLastHarvest = -1;// �����ϴ��ջ�������Ϊ-1
        gridPropertyDetails.daysSinceWatered = -1;// ���ý�ˮ�������Ϊ-1

        // Should the crop be hidden before the harvested animation�Ƿ�Ӧ���ո������ǰ��������
        if (cropDetails.hideCropBeforeHarvestedAnimation)// �жϵ�ǰ���������Ƿ�Ҫ���ڲ����ջ񶯻�ǰ��������
        {
            GetComponentInChildren<SpriteRenderer>().enabled = false;// �������Ϊ�棬�������������������Ӷ����е�SpriteRenderer�����ʹ�����ڳ����в��ɼ�
        }

        // �����º��������������д���������ϵͳ��ʹ��ͼ״̬���Ը��£�ͨ���Ǳ�Ǵ˵ؿ���������
        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        // Is there a harvested animation - Destory this crop game object after animation completed�Ƿ�����ջ񶯻� - �ڶ�����ɺ����ٸ�������Ϸ����
        if (cropDetails.isHarvestedAnimation && animator != null)// ��鵱ǰ�����Ƿ����ջ񶯻������Ҹ��ӵ�Animator�����Ϊ�գ�ȷ�������ܹ�����
        {
            StartCoroutine(ProcessHarvestedActionsAfterAnimation(cropDetails, gridPropertyDetails, animator));// ���������������������һ��Э�̡���Э�̽��ȴ��ջ񶯻�������Ϻ���ִ��ʵ�ʵ��ջ�������
        }
        else
        {
            HarvestActions(cropDetails, gridPropertyDetails);// �������û���ջ񶯻�������Animator���Ϊ�գ������������ջ���Ϊ������ִ�������ɲ�����ٶ���Ȳ���
        }

    }

    // ����һ��˽��Э�̷����������ڶ������ź����ջ���Ϊ�����������������顢�������ԺͶ���������
    private IEnumerator ProcessHarvestedActionsAfterAnimation(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails, Animator animator)
    {
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Harvested"))// ѭ����鵱ǰ����״̬�Ƿ��Ѿ�����"Harvested"״̬
        {
            yield return null;// ���������û�в��ŵ�"Harvested"״̬������ͣЭ��ֱ����һ֡�ټ������
        }
        // ������������Ϻ󣬵���ʵ�ʵ��ջ���Ϊ����
        HarvestActions(cropDetails, gridPropertyDetails);
    }
    

    // �ջ���Ϊ�ľ���ʵ�֣��������ɲ��ﲢ����������Ϸ����
    private void HarvestActions(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails)
    {
        // �����ջ�Ĳ����С�󡢺��ܲ��ȣ�
        SpawnHarvestedItems(cropDetails);

        // Does this crop transform into another crop���������ת��Ϊ��һ��������
        if (cropDetails.harvestedTransformItemCode > 0)
        {
            CreateHarvestedTransformCrop(cropDetails, gridPropertyDetails);
        }


        // ����Ϸ���������٣�ɾ�������������Ϸ����
        Destroy(gameObject);  
    }

    // �������ﱻ�ջ���������Ʒ
    private void SpawnHarvestedItems(CropDetails cropDetails)
    {
        // Spawn the item(s) to be produced�������������ж�������пɲ�����Ʒ�Ĵ�������
        for (int i = 0; i < cropDetails.cropProducedItemCode.Length; i++)
        {
            int cropsToProduce;

            // Calculate how many crops to produce������Ҫ������������
            if (cropDetails.cropProducedMinQuantity[i] == cropDetails.cropProducedMaxQuantity[i] ||
                cropDetails.cropProducedMaxQuantity[i] < cropDetails.cropProducedMinQuantity[i])
            {
                //�����С���������������������ô������С����С������ֱ��ʹ����С����
                cropsToProduce = cropDetails.cropProducedMinQuantity[i];
            }
            else
            {
                //����С������������֮�䣨����Сֵ���������ֵ+1�����һ������
                cropsToProduce = Random.Range(cropDetails.cropProducedMinQuantity[i], cropDetails.cropProducedMaxQuantity[i] + 1);
            }

            // ���ݼ����������cropsToProduce��ѭ������ÿһ����Ʒ
            for (int j = 0; j < cropsToProduce; j++)
            {
                Vector3 spawnPosition;
                if (cropDetails.spawnCropProducedAtPlayerPosition)
                {
                    // Add item to the players inventory�������Ϊ������ֱ�ӷ��͵����λ�ã��������Ʒ����������Ʒ��ӵ���ҿ����
                    InventoryManager.Instance.AddItem(InventoryLocation.player, cropDetails.cropProducedItemCode[i]);
                }
                else
                {
                    // Random position������������Χ�����λ�����ɳ�����Ʒ
                    spawnPosition = new Vector3(transform.position.x + Random.Range(-1f, 1f), transform.position.y + Random.Range(-1f, 1f), 0f);
                    // ͨ��������Ʒ�������ڼ������λ��ʵ����һ��������Ʒ������ڵ��ϵ�С��
                    SceneItemsManager.Instance.InstantiateSceneItem(cropDetails.cropProducedItemCode[i], spawnPosition);

                }
            }
        }
    }


    private void CreateHarvestedTransformCrop(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails)
    {
        // Update crop in grid properties�����������и�������
        gridPropertyDetails.seedItemCode = cropDetails.harvestedTransformItemCode;
        gridPropertyDetails.growthDays = 0;
        gridPropertyDetails.daysSinceLastHarvest = -1;
        gridPropertyDetails.daysSinceWatered = -1;

        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        // Display planted cropչʾ��ֲ����
        GridPropertiesManager.Instance.DisplayPlantedCrop(gridPropertyDetails);

    }

}


