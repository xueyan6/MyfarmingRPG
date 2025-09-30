using System.Collections.Generic;
using UnityEngine;

public static class HelperMethods
{
    //Gets Components of type T at positionToCheck. Returns truue if at least one found and the found components are returned in componentAtPositionList
    //��ȡλ��λ��ToCheck��������T��������������ҵ�һ��������򷵻�true�������ҵ������������componentAtPositionList�С�

    public static bool GetComponentsAtCursorLocation<T>(out List<T> componentsAtPositionList, Vector3 positionToCheck)
    {
        bool found = false;

        // ������ʱ�б����ڴ洢�ҵ������
        List<T> componentList = new List<T>();

        // ʹ��Physics2D.OverlapPointAll���ָ��λ�õ�����2D��ײ��
        Collider2D[] collider2DArray = Physics2D.OverlapPointAll(positionToCheck);

        // Loop through all colliders to get an object of type T����������ײ���Ի�ȡ����Ϊ T �Ķ���
        T tComponent = default(T);

        // ʹ��Physics2D.OverlapPointAll���ָ��λ�õ�����2D��ײ��
        for (int i = 0; i < collider2DArray.Length; i++)
        {
            // ���Ի�ȡ��ײ��������Ϸ����ĸ������
            tComponent = collider2DArray[i].gameObject.GetComponentInParent<T>();
            if (tComponent != null)
            {
                // ����ҵ�Ŀ����������ñ�־λ����ӵ��б�
                found = true;
                componentList.Add(tComponent);
            }
            else
            {
                // �������δ�ҵ������Ի�ȡ�Ӽ����
                tComponent = collider2DArray[i].gameObject.GetComponentInChildren<T>();
                if (tComponent != null)
                {
                    found = true;
                    componentList.Add(tComponent);
                }
            }
        }
        // ����ʱ�б�ֵ���������
        componentsAtPositionList = componentList;

        return found;

    }

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

    // Returns array of components of type T at box with centre point and size and angle.The numberOfCollidersToTest for is passed as a parameter.Found components are returned in the array
    // ���������ĵ㡢�ߴ�ͽǶ�Ϊ�����ĺ���������Ϊ T ��������顣���Ե���ײ��������Ϊ�������ݡ��ҵ�������������������С�
    // ���ָ�����������ڵ�������ײ������������Щ��ײ��������Ϸ�����ϵ�ָ���������
    public static T[] GetComponentsAtBoxLocationNonAlloc<T>(int numberOfCollidersToTest, Vector2 point, Vector2 size, float angle)
    {
        Collider2D[] collider2DArray = new Collider2D[numberOfCollidersToTest];

        Physics2D.OverlapBoxNonAlloc(point, size, angle, collider2DArray);//�������������ص���������ײ����������� collider2DArray

        T tComponent = default(T);

        //�������м�⵽����ײ�������Ի�ȡ��������Ϸ����� T ���������
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
        //���ذ���������Ч��������飬δ�ҵ������Ԫ��Ϊ null��
        return componentArray;

    }
}
