using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;


[ExecuteAlways]
public class TilemapGridProperties : MonoBehaviour
{
    private Tilemap tilemap;
    [SerializeField] private SO_GridProperties gridProperties = null;
    [SerializeField] private GridBoolProperty gridBoolProperty = GridBoolProperty.diggable;

    private void OnEnable()
    {
        // Only populate in the editor仅在编辑器模式下执行（非运行时）
        if (!Application.IsPlaying(gameObject))
        {
            tilemap = GetComponent<Tilemap>();

            if (gridProperties != null)// 如果网格属性配置已赋值
            {
                // 编辑模式下开启脚本，清空所有的gridPropertyList数据
                gridProperties.gridPropertyList.Clear();// 清空网格属性列表中的现有数据
            }
        }
    }

    private void OnDisable()
    {
        // Only populate in the editor仅在编辑器模式下执行（非运行时）
        if (!Application.IsPlaying(gameObject))
        {
            // 更新网格属性数据。（关闭脚本，储存数据）
            UpdateGridProperties();

            if (gridProperties != null)
            {
                // This is required to ensure that the updated gridproperties gameobject gets saved when the game is saved - otherwise they are not saved.
                //此操作旨在确保更新后的网格属性游戏对象在保存游戏时被保存——否则这些属性将不会被保存。   
                EditorUtility.SetDirty(gridProperties);// 标记网格属性对象为已修改状态，确保保存时生效
            }
        }
    }


    private void UpdateGridProperties()
    {
        // Compress tilemap bounds压缩瓦片地图边界（去除无效或冗余的边界数据，使地图显示更加紧凑高效）
        tilemap.CompressBounds();

        // Only populate in the editor仅在编辑器模式下执行（非运行时）
        if (!Application.IsPlaying(gameObject))
        {
            if (gridProperties != null)
            {
                //// 获取瓦片地图的起始和结束单元格坐标
                Vector3Int startCell = tilemap.cellBounds.min;
                Vector3Int endCell = tilemap.cellBounds.max;

                //遍历瓦片地图上的每个单元格
                for (int x = startCell.x; x < endCell.x; x++)
                {
                    for (int y = startCell.y; y < endCell.y; y++)
                    {
                        // 获取指定位置的瓦片
                        TileBase tile = tilemap.GetTile(new Vector3Int(x, y, 0));

                        // 如果该位置有瓦片
                        if (tile != null)
                        {
                            // 将瓦片坐标和属性添加到网格属性列表中
                            gridProperties.gridPropertyList.Add(new GridProperty(new GridCoordinate(x, y), gridBoolProperty, true));
                        }
                    }
                }
            }
        }

    }

    private void Update()
    {
        // Only populate in the editor仅在编辑器模式下执行（非运行时）
        if (!Application.IsPlaying(gameObject))
        {
            Debug.Log("DISABLE PROPERTY TILEMAPS");
        }
    }

    //核心功能：
    //该脚本是Unity编辑器扩展工具，专门用于管理Tilemap的网格属性数据
    //主要实现Tilemap单元格属性的自动采集和存储功能
    //运行机制：
    //仅在编辑器模式下工作（通过Application.IsPlaying判断）

    //启用时清空现有属性数据（OnEnable）：
    //当脚本组件被激活/启用时（如在Inspector勾选组件或场景加载时）
    //立即清空gridProperties.gridPropertyList中存储的所有已有属性数据
    //目的是确保每次编辑都从干净状态开始，避免旧数据干扰
    //仅发生在编辑器模式（非游戏运行时）

    //禁用时保存更新后的属性数据（OnDisable）：
    //当脚本组件被禁用/场景结束编辑时
    //调用UpdateGridProperties()扫描当前Tilemap所有有效瓦片
    //将新的网格坐标和属性值存入gridProperties.gridPropertyList
    //通过EditorUtility.SetDirty标记数据需要保存
    //确保编辑结果能持久化到项目文件中

    //本质是建立"编辑时重置→修改→关闭时保存"的工作流，用于Tilemap与网格属性系统的数据同步。

    //数据处理流程：
    //通过CompressBounds优化瓦片地图边界
    //遍历所有包含瓦片的单元格坐标
    //将有效单元格坐标和属性值存入SO_GridProperties脚本化对象
    //数据持久化：
    //使用EditorUtility.SetDirty确保修改能被保存
    //通过序列化字段暴露配置参数（gridProperties/gridBoolProperty）
}
