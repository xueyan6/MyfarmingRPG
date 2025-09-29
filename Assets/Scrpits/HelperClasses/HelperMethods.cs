using System.Collections.Generic;
using UnityEngine;

public static class HelperMethods
{
    // Gets Components of type T at box with center point and size and angle. Returns true if at least one found and the found components are returned in the list
    // ��ȡָ������T��������ڸ������ĵ㡢�ߴ�ͽǶȵľ��������ڡ���������ҵ�һ��������򷵻�true�����ҵ�������洢���б���
    public static bool GetComponentsAtBoxLocation<T>(out List<T> listComponentsAtBoxPosition, Vector2 point, Vector2 size, float angle)
    {
        // ���巽������ָ�����������ڻ�ȡ����T�����
        // ����˵����
        // - out List<T> listComponentsAtBoxPosition������������洢�ҵ�������б�
        // - Vector2 point��������������ĵ�����
        // - Vector2 size����������ĳߴ磨��Ⱥ͸߶ȣ�
        // - float angle�������������ת�Ƕȣ����ȣ�

        bool found = false;
        List<T> componentList = new List<T>();

        // ʹ��Physics2D.OverlapBoxAll�����������ڵ�������ײ��
        // ����һ��Collider2D���飬������������������ཻ����ײ��
        Collider2D[] collider2DArray = Physics2D.OverlapBoxAll(point, size, angle);

        // Loop through all colliders to get an object of type T
        // ����������ײ�������Ի�ȡ����T�����
        for (int i = 0; i < collider2DArray.Length; i++)
        {
            // ����ͨ����ײ����ȡ�������е�T�������
            T tComponent = collider2DArray[i].gameObject.GetComponentInParent<T>();
            if (tComponent != null)
            {
                found = true;
                componentList.Add(tComponent);// ���ҵ��������ӵ��б�
            }
            else
            {
                // �����������û���ҵ����������ͨ����ײ����ȡ�Ӷ����е�T�������
                tComponent = collider2DArray[i].gameObject.GetComponentInChildren<T>();
                if (tComponent != null)
                {
                    found = true;
                    componentList.Add(tComponent);
                }
            }
        }
        //���ҵ�������б�ֵ���������
        listComponentsAtBoxPosition = componentList;
        return found;
    }

}
