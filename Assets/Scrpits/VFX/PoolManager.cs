using System.Collections.Generic;
using UnityEngine;

public class PoolManager : SingletonMonobehaviour<PoolManager>
{
    private Dictionary<int, Queue<GameObject>> poolDictionary = new Dictionary<int, Queue<GameObject>>();// 对象池字典，键为预制体实例ID，值为对象队列
    [SerializeField] private Pool[] pool = null;   // 可序列化的对象池配置数组（在Inspector中编辑）
    [SerializeField] private Transform objectPoolTransform = null;  // 对象池父级Transform（用于场景层级管理）

    [System.Serializable]
    public struct Pool
    {
        public int poolSize;// 池大小（预生成数量）
        public GameObject prefab;// 预制体引用
    }

    private void Start()
    {
        // Create object pools on start遍历所有配置的池
        for (int i = 0; i < pool.Length; i++)
        {
            // 为每个配置创建对象池
            CreatePool(pool[i].prefab, pool[i].poolSize);
        }
    }

    private void CreatePool(GameObject prefab, int poolSize)
    {
        // 获取预制体实例ID作为字典键
        int poolKey = prefab.GetInstanceID();
        // 获取预制体名称用于创建父对象
        string prefabName = prefab.name; // get prefab name

        // 创建池的父级空对象（保持场景整洁）
        GameObject parentGameObject = new GameObject(prefabName + "Anchor");
        // 设置为预设父级的子对象
        parentGameObject.transform.SetParent(objectPoolTransform);

        // 检查是否已存在该池
        if (!poolDictionary.ContainsKey(poolKey))
        {
            // 创建新的对象队列
            poolDictionary.Add(poolKey, new Queue<GameObject>());

            // 预生成指定数量的对象
            for (int i = 0; i < poolSize; i++)
            {
                // 实例化预制体（设置为池父对象的子级）
                GameObject newObject = Instantiate(prefab, parentGameObject.transform) as GameObject;
                // 初始设置为非激活状态
                newObject.SetActive(false);
                // 加入对象队列
                poolDictionary[poolKey].Enqueue(newObject);
            }
        }
    }
    // 对象复用方法
    public GameObject ReuseObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        // 获取预制体键值
        int poolKey = prefab.GetInstanceID();
        // 检查是否存在对应对象池
        if (poolDictionary.ContainsKey(poolKey))
        {
            // Get object from pool queue从池中获取可用对象
            GameObject objectToReuse = GetObjectFromPool(poolKey);
            //重置对象状态
            ResetObject(position, rotation, objectToReuse, prefab);

            return objectToReuse;
        }
        else
        {
            // 未找到对应池时的错误处理
            Debug.Log("No object pool for " + prefab);
            return null;
        }
    }

    private GameObject GetObjectFromPool(int poolKey)
    {
        // 从队列头部取出对象
        GameObject objectToReuse = poolDictionary[poolKey].Dequeue();
        // 将对象重新放回队列尾部（实现循环复用）
        poolDictionary[poolKey].Enqueue(objectToReuse);

        // log to console if object is currently active
        // 如果对象处于激活状态（异常情况处理）
        if (objectToReuse.activeSelf == true) // objectToReuse.activeSelf 是 GameObject 类的一个属性，用于检查该对象当前是否处于激活状态。
        {
            // 强制重置为未激活状态
            objectToReuse.SetActive(false); 
        }

        return objectToReuse;
    }
    // 重置对象状态
    private static void ResetObject(Vector3 position, Quaternion rotation, GameObject objectToReuse, GameObject prefab)
    {
        // 设置对象位置和旋转
        objectToReuse.transform.position = position;
        objectToReuse.transform.rotation = rotation;

        // 重置为预制体的原始缩放（避免之前使用时的缩放残留） 
        objectToReuse.transform.localScale = prefab.transform.localScale;
    }

}
