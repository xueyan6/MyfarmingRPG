using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Attach to a crop prefab to set the values in the grid property dictionary附加到作物预制件上，以设置网格属性字典中的值
public class CropInstantiator : MonoBehaviour
{
    private Grid grid;
    [SerializeField] private int daysSinceDug = -1;
    [SerializeField] private int daysSinceWatered = -1;
    [ItemCodeDescription]
    [SerializeField] private int seedItemCode = 0;
    [SerializeField] private int growthDays = 0;


    private void OnDisable()
    {
        EventHandler.InstantiateCropPrefabsEvent -= InstantiateCropPrefabs;
    }

    private void OnEnable()
    {
        EventHandler.InstantiateCropPrefabsEvent += InstantiateCropPrefabs;
    }



    private void InstantiateCropPrefabs()
    {
        // Get grid gameobject获取网格游戏对象
        grid = GameObject.FindObjectOfType<Grid>();// 在场景中查找唯一的Grid组件，这是进行世界坐标与网格坐标转换所必需的

        // Get grid position for crop获取裁剪区域的网格位置
        Vector3Int cropGridPosition = grid.WorldToCell(transform.position);// 将这棵作物游戏对象在世界空间中的位置，转换为它所在网格的坐标（例如(10, 5, 0)）。

        // Set Crop Grid Properties设置裁剪网格属性
        SetCropGridProperties(cropGridPosition);// 调用下面的方法，将当前组件上设置的作物状态（挖掘、浇水、种子、生长天数）写入到网格管理系统中的对应位置。

        // Destroy this gameobject销毁此游戏对象
        Destroy(gameObject); // 关键步骤：完成数据注册后，销毁这个场景中原始的作物游戏对象。它的使命已经完成。
    }

    private void SetCropGridProperties(Vector3Int cropGridPosition)
    {
        if (seedItemCode > 0)// 只有当地里确实有种子（即存在作物）时，才进行初始化。
        {
            GridPropertyDetails gridPropertyDetails;

            gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cropGridPosition.x, cropGridPosition.y);// 从网格属性管理器（一个单例）中，尝试获取该网格位置现有的属性详情。

            if (gridPropertyDetails == null)
            {
                gridPropertyDetails = new GridPropertyDetails();// 如果这个网格位置之前没有被初始化过（例如一块新地），就创建一个新的属性详情对象
            }

            gridPropertyDetails.daysSinceDug = daysSinceDug;
            gridPropertyDetails.daysSinceWatered = daysSinceWatered;
            gridPropertyDetails.seedItemCode = seedItemCode;
            gridPropertyDetails.growthDays = growthDays;
            // 将当前组件上在Inspector中预设好的状态数据，赋给网格属性详情对象。

            GridPropertiesManager.Instance.SetGridPropertyDetails(cropGridPosition.x, cropGridPosition.y, gridPropertyDetails);
            // 核心操作：将包含了作物所有状态信息的`gridPropertyDetails`对象，保存回网格管理系统中。从此，这片土地的状态由管理器统一负责。
        }
        //总结一下整个流程，就好比是：
        //场景布置（编辑时）： 你在场景编辑器中，手动放置了一些不同生长阶段的作物（比如有些刚发芽，有些快成熟了），并在这个组件上设置好它们各自的状态
        //游戏启动（运行时）： 游戏开始，一个全局事件被触发。
        //数据迁移： 每个预设作物上的这个脚本被唤醒，它们各自：
        //找到自己的“户籍地址”（网格坐标）。
        //把自己的“身份证信息”（生长状态）上报给中央管理系统（GridPropertiesManager）。
        //交接完成： 数据上报后，这些“临时工”（场景中的原始作物对象）就功成身退，被销毁了。
        //统一管理： 之后，游戏运行时所有关于作物的逻辑（生长、浇水、收获）都将基于GridPropertiesManager中的数据来处理，而不再是直接操作场景中最初的那个游戏对象。
    }

}
