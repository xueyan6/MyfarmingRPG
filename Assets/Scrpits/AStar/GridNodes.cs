using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridNodes // 定义GridNodes类，用于管理网格节点（Node）的二维数组
{
    private int width; // 私有成员变量：网格的宽度（列数）
    private int height; // 私有成员变量：网格的高度（行数）

    private Node[,] gridNode; // 私有成员变量：存储网格节点的二维数组（Node[,]）

    // 构造函数：初始化网格的宽度、高度，并创建二维节点数组
    public GridNodes(int width, int height)
    {
        this.width = width; // 赋值网格宽度
        this.height = height; // 赋值网格高度

        gridNode = new Node[width, height]; // 创建二维数组，大小为width×height

        // 遍历所有行和列，为每个位置初始化Node对象
        for (int x = 0; x < width; x++) // 外层循环：遍历列（x轴）
        {
            for (int y = 0; y < height; y++) // 内层循环：遍历行（y轴）
            {
                gridNode[x, y] = new Node(new Vector2Int(x, y)); // 为位置(x,y)创建Node对象，并传入网格坐标
            }
        }
    }

    // 公共方法：根据坐标(xPosition, yPosition)获取对应的网格节点
    public Node GetGridNode(int xPosition, int yPosition)
    {
        // 检查坐标是否在网格范围内
        if (xPosition < width && yPosition < height)
        {
            return gridNode[xPosition, yPosition]; // 若在范围内，返回对应位置的节点
        }
        else
        {
            Debug.Log("Requested grid node is out of range"); // 若超出范围，打印调试日志
            return null; // 返回null，表示未找到有效节点
        }
    }
}

//该GridNodes类是A寻路算法中“网格结构”的封装，通过width、height定义网格大小，通过Node[,]存储所有网格节点，
//并提供GetGridNode方法按坐标获取节点。它是A算法中“路径搜索空间”的基础结构，为后续节点的遍历、代价计算、路径规划提供了数据支持。