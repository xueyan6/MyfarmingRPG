using System.Collections.Generic;
using UnityEngine;

public static class HelperMethods
{
    // Gets Components of type T at box with center point and size and angle. Returns true if at least one found and the found components are returned in the list
    // 获取指定类型T的组件，在给定中心点、尺寸和角度的矩形区域内。如果至少找到一个组件，则返回true并将找到的组件存储在列表中
    public static bool GetComponentsAtBoxLocation<T>(out List<T> listComponentsAtBoxPosition, Vector2 point, Vector2 size, float angle)
    {
        // 定义方法：在指定矩形区域内获取类型T的组件
        // 参数说明：
        // - out List<T> listComponentsAtBoxPosition：输出参数，存储找到的组件列表
        // - Vector2 point：矩形区域的中心点坐标
        // - Vector2 size：矩形区域的尺寸（宽度和高度）
        // - float angle：矩形区域的旋转角度（弧度）

        bool found = false;
        List<T> componentList = new List<T>();

        // 使用Physics2D.OverlapBoxAll检测矩形区域内的所有碰撞器
        // 返回一个Collider2D数组，包含所有与矩形区域相交的碰撞器
        Collider2D[] collider2DArray = Physics2D.OverlapBoxAll(point, size, angle);

        // Loop through all colliders to get an object of type T
        // 遍历所有碰撞器，尝试获取类型T的组件
        for (int i = 0; i < collider2DArray.Length; i++)
        {
            // 尝试通过碰撞器获取父对象中的T类型组件
            T tComponent = collider2DArray[i].gameObject.GetComponentInParent<T>();
            if (tComponent != null)
            {
                found = true;
                componentList.Add(tComponent);// 将找到的组件添加到列表
            }
            else
            {
                // 如果父对象中没有找到组件，尝试通过碰撞器获取子对象中的T类型组件
                tComponent = collider2DArray[i].gameObject.GetComponentInChildren<T>();
                if (tComponent != null)
                {
                    found = true;
                    componentList.Add(tComponent);
                }
            }
        }
        //将找到的组件列表赋值给输出参数
        listComponentsAtBoxPosition = componentList;
        return found;
    }

}
