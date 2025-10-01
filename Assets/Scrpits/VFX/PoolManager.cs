using System.Collections.Generic;
using UnityEngine;

public class PoolManager : SingletonMonobehaviour<PoolManager>
{
    private Dictionary<int, Queue<GameObject>> poolDictionary = new Dictionary<int, Queue<GameObject>>();// ������ֵ䣬��ΪԤ����ʵ��ID��ֵΪ�������
    [SerializeField] private Pool[] pool = null;   // �����л��Ķ�����������飨��Inspector�б༭��
    [SerializeField] private Transform objectPoolTransform = null;  // ����ظ���Transform�����ڳ����㼶����

    [System.Serializable]
    public struct Pool
    {
        public int poolSize;// �ش�С��Ԥ����������
        public GameObject prefab;// Ԥ��������
    }

    private void Start()
    {
        // Create object pools on start�����������õĳ�
        for (int i = 0; i < pool.Length; i++)
        {
            // Ϊÿ�����ô��������
            CreatePool(pool[i].prefab, pool[i].poolSize);
        }
    }

    private void CreatePool(GameObject prefab, int poolSize)
    {
        // ��ȡԤ����ʵ��ID��Ϊ�ֵ��
        int poolKey = prefab.GetInstanceID();
        // ��ȡԤ�����������ڴ���������
        string prefabName = prefab.name; // get prefab name

        // �����صĸ����ն��󣨱��ֳ������ࣩ
        GameObject parentGameObject = new GameObject(prefabName + "Anchor");
        // ����ΪԤ�踸�����Ӷ���
        parentGameObject.transform.SetParent(objectPoolTransform);

        // ����Ƿ��Ѵ��ڸó�
        if (!poolDictionary.ContainsKey(poolKey))
        {
            // �����µĶ������
            poolDictionary.Add(poolKey, new Queue<GameObject>());

            // Ԥ����ָ�������Ķ���
            for (int i = 0; i < poolSize; i++)
            {
                // ʵ����Ԥ���壨����Ϊ�ظ�������Ӽ���
                GameObject newObject = Instantiate(prefab, parentGameObject.transform) as GameObject;
                // ��ʼ����Ϊ�Ǽ���״̬
                newObject.SetActive(false);
                // ����������
                poolDictionary[poolKey].Enqueue(newObject);
            }
        }
    }
    // �����÷���
    public GameObject ReuseObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        // ��ȡԤ�����ֵ
        int poolKey = prefab.GetInstanceID();
        // ����Ƿ���ڶ�Ӧ�����
        if (poolDictionary.ContainsKey(poolKey))
        {
            // Get object from pool queue�ӳ��л�ȡ���ö���
            GameObject objectToReuse = GetObjectFromPool(poolKey);
            //���ö���״̬
            ResetObject(position, rotation, objectToReuse, prefab);

            return objectToReuse;
        }
        else
        {
            // δ�ҵ���Ӧ��ʱ�Ĵ�����
            Debug.Log("No object pool for " + prefab);
            return null;
        }
    }

    private GameObject GetObjectFromPool(int poolKey)
    {
        // �Ӷ���ͷ��ȡ������
        GameObject objectToReuse = poolDictionary[poolKey].Dequeue();
        // ���������·Żض���β����ʵ��ѭ�����ã�
        poolDictionary[poolKey].Enqueue(objectToReuse);

        // log to console if object is currently active
        // ��������ڼ���״̬���쳣�������
        if (objectToReuse.activeSelf == true) // objectToReuse.activeSelf �� GameObject ���һ�����ԣ����ڼ��ö���ǰ�Ƿ��ڼ���״̬��
        {
            // ǿ������Ϊδ����״̬
            objectToReuse.SetActive(false); 
        }

        return objectToReuse;
    }
    // ���ö���״̬
    private static void ResetObject(Vector3 position, Quaternion rotation, GameObject objectToReuse, GameObject prefab)
    {
        // ���ö���λ�ú���ת
        objectToReuse.transform.position = position;
        objectToReuse.transform.rotation = rotation;

        // ����ΪԤ�����ԭʼ���ţ�����֮ǰʹ��ʱ�����Ų����� 
        objectToReuse.transform.localScale = prefab.transform.localScale;
    }

}
