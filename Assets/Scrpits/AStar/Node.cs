using System; 
using UnityEngine; 

public class Node : IComparable<Node> // 定义Node类，实现IComparable<Node>接口（用于节点的比较排序）
{
    // 成员变量：存储节点的属性信息
    public Vector2Int gridPosition; // 节点在网格中的位置（x,y坐标）
    public int gCost = 0; // 距离起始节点的代价（A*算法中“已走路径代价”）
    public int hCost = 0; // 距离终点节点的代价（A*算法中“启发式估计代价”）
    public bool isObstacle = false; // 节点是否为障碍物（true表示障碍，false表示可通行）
    public int movementPenalty; // 移动惩罚值（如不同地形的移动代价差异）
    public Node parentNode; // 节点的父节点（用于A*算法中反向追踪最优路径）

    // 构造函数：初始化节点的gridPosition，将parentNode设为null
    public Node(Vector2Int gridPosition)
    {
        this.gridPosition = gridPosition; // 赋值网格位置
        parentNode = null; // 初始化父节点为null（表示当前节点无父节点）
    }

    // 属性：计算并返回节点的FCost（总代价，A*算法核心评估值）
    public int FCost
    {
        get
        {
            return gCost + hCost; // 总代价 = 已走代价 + 启发式代价
        }
    }

    // 实现IComparable<Node>接口的方法：比较两个节点的优先级（用于A*算法中Open List的排序）
    public int CompareTo(Node nodeToCompare)
    {
        // 若当前实例的Fcost小于nodeToCompare.FCost，则比较结果为<0
        // 若当前实例的Fcost大于nodeToCompare.FCost，则比较结果为>0
        // 若值相等，则比较结果为==0

        int compare = FCost.CompareTo(nodeToCompare.FCost); // 先比较总代价（FCost）

        if (compare == 0) // 若总代价相同，进一步比较启发式代价（hCost）
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }

        return compare; // 返回比较结果（负数表示当前节点优先级更高，正数表示当前节点优先级更低）
    }
    //该Node类是A*寻路算法中“节点”的封装，通过成员变量存储节点的位置、代价、障碍状态、移动惩罚、父节点等信息，通过FCost属性计算节点的“总代价”，
    //并通过CompareTo方法实现节点的优先级比较（用于Open List的排序）。它是A*算法中“路径搜索空间”的基本单元，为后续路径规划提供了数据结构支持。
}
