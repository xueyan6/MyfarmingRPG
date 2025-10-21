
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Crop : MonoBehaviour
{
    private int harvestActionCount = 0;// ��¼��ǰ�Ѿ��Ը�����ִ���˶��ٴ��ջ����

    [HideInInspector]
    public Vector2Int cropGridPosition;// �洢�������������ͼ�е�����λ��

    // �����߶�����ʩ�ӵĲ����������ｻ���ĺ������
    public void ProcessToolAction(ItemDetails equippedItemDetails)
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

        // Get required harvest actions for tool��ѯʹ�õ�ǰ�����ջ�������ܹ���Ҫ���ٴβ������������-1����ʾ�ù����޷������ջ������
        int requiredHarvestActions = cropDetails.RequiredHarvestActionsForTool(equippedItemDetails.itemCode);
        if (requiredHarvestActions == -1) return;//��return ���᷵�ص����ø÷����Ĵ������ڵ�λ�ã��൱���˳��������ص�ԭ����ִ�й���ϼ���ǰ�������� private ����ֻ�ܱ�������ã����������Ƿ��ص����ࣻ�� public �������Ա��κ�����ã����������ܷ��ص����࣬Ҳ���ܷ��ص�������

        // Increment harvest action count�����ջ������������ʾ���γɹ�ִ����һ����Ч���ջ����
        harvestActionCount++;

        // Check if required harvest actions made��鵱ǰ���ջ���������Ƿ��Ѿ��ﵽ�򳬹���Ҫ����ܴ���
        if (harvestActionCount >= requiredHarvestActions)
            HarvestCrop(cropDetails, gridPropertyDetails);// ����ѴﵽҪ����ִ�����յ��ջ��߼�

    }

    // ִ�����յ��ջ�����������������ݲ���������
    private void HarvestCrop(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails)
    {
        // Delete crop from grid properties�����������е����Ӵ�������Ϊ-1���գ�����ʾ�˸����ϲ���������
        gridPropertyDetails.seedItemCode = -1;
        gridPropertyDetails.growthDays = -1;// ������������Ϊ-1
        gridPropertyDetails.daysSinceLastHarvest = -1;// �����ϴ��ջ�������Ϊ-1
        gridPropertyDetails.daysSinceWatered = -1;// ���ý�ˮ�������Ϊ-1

        // �����º��������������д���������ϵͳ��ʹ��ͼ״̬���Ը���
        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        // ִ���ջ�ľ�����Ϊ�������ɲ�������ٶ���
        HarvestActions(cropDetails, gridPropertyDetails);
    }

    // �ջ���Ϊ�ľ���ʵ�֣��������ɲ��ﲢ����������Ϸ����
    private void HarvestActions(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails)
    {
        // �����ջ�Ĳ����С�󡢺��ܲ��ȣ�
        SpawnHarvestedItems(cropDetails);

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

}


