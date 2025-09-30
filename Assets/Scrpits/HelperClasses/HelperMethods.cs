using System.Collections.Generic;
using UnityEngine;

public static class HelperMethods
{
    //Gets Components of type T at positionToCheck. Returns truue if at least one found and the found components are returned in componentAtPositionList
    //获取位于位置ToCheck处的类型T的组件。若至少找到一个组件，则返回true，并将找到的组件返回至componentAtPositionList中。

    public static bool GetComponentsAtCursorLocation<T>(out List<T> componentsAtPositionList, Vector3 positionToCheck)
    {
        bool found = false;

        // 创建临时列表，用于存储找到的组件
        List<T> componentList = new List<T>();

        // 使用Physics2D.OverlapPointAll检测指定位置的所有2D碰撞器
        Collider2D[] collider2DArray = Physics2D.OverlapPointAll(positionToCheck);

        // Loop through all colliders to get an object of type T遍历所有碰撞器以获取类型为 T 的对象
        T tComponent = default(T);

        // 使用Physics2D.OverlapPointAll检测指定位置的所有2D碰撞器
        for (int i = 0; i < collider2DArray.Length; i++)
        {
            // 尝试获取碰撞器关联游戏对象的父级组件
            tComponent = collider2DArray[i].gameObject.GetComponentInParent<T>();
            if (tComponent != null)
            {
                // 如果找到目标组件，设置标志位并添加到列表
                found = true;
                componentList.Add(tComponent);
            }
            else
            {
                // 如果父级未找到，尝试获取子级组件
                tComponent = collider2DArray[i].gameObject.GetComponentInChildren<T>();
                if (tComponent != null)
                {
                    found = true;
                    componentList.Add(tComponent);
                }
            }
        }
        // 将临时列表赋值给输出参数
        componentsAtPositionList = componentList;

        return found;

    }

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

    // Returns array of components of type T at box with centre point and size and angle.The numberOfCollidersToTest for is passed as a parameter.Found components are returned in the array
    // 返回以中心点、尺寸和角度为参数的盒子中类型为 T 的组件数组。测试的碰撞体数量作为参数传递。找到的组件将返回在数组中。
    // 检测指定矩形区域内的所有碰撞器，并返回这些碰撞器所在游戏对象上的指定类型组件
    public static T[] GetComponentsAtBoxLocationNonAlloc<T>(int numberOfCollidersToTest, Vector2 point, Vector2 size, float angle)
    {
        Collider2D[] collider2DArray = new Collider2D[numberOfCollidersToTest];

        Physics2D.OverlapBoxNonAlloc(point, size, angle, collider2DArray);//检测与矩形区域重叠的所有碰撞器，结果存入 collider2DArray

        T tComponent = default(T);

        //遍历所有检测到的碰撞器，尝试获取其所在游戏对象的 T 类型组件。
        T[] componentArray = new T[collider2DArray.Length];
        for (int i = collider2DArray.Length - 1; i >= 0; i--)
        {
            if (collider2DArray[i] != null)
            {
                tComponent = collider2DArray[i].gameObject.GetComponent<T>();

                if (tComponent != null)
                {
                    componentArray[i] = tComponent;
                }
            }
        }
        //返回包含所有有效组件的数组，未找到组件的元素为 null。
        return componentArray;

    }
}
