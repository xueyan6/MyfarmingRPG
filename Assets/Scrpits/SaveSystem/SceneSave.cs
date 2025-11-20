using System.Collections.Generic;

[System.Serializable]

public class SceneSave
{
    public Dictionary<string, int> intDictionary;//时间
    public Dictionary<string, bool> boolDictionary; // string key is an identifier name we choose for this list字符串键是我们为该列表选择的标识符名称
    public Dictionary<string, string> stringDictionary;
    public Dictionary<string, Vector3Serializable> vector3Dictionary; // 可以保存角色位置
    public Dictionary<string, int[]> intArrayDictionary;//库存容量信息
    public List<SceneItem> listSceneItem;//场景中的物品
    public Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary; // 作物详细信息
    public List<InventoryItem>[] listInvItemArray;//库存栏中的项目信息
}

//字典和列表是两种不同的数据结构，主要区别和使用场景如下：
//字典（Dictionary）
//数据结构：键值对集合，通过唯一键来访问值
//查找方式：通过键直接访问，时间复杂度O(1)→看PS
//元素顺序：无序（或按插入顺序，取决于实现）
//适用场景：
//需要按键名快速查找数据的场景
//存储有明确标识的对象，如玩家属性、物品信息等
//在您代码中的使用：
//vector3Dictionary：通过"playerPosition"键快速获取位置数据
//stringDictionary：通过"currentScene"、"playerDirection"键获取对应信息

//列表（List）
//数据结构：有序的元素集合，通过索引访问
//查找方式：通过索引位置访问，按值查找需要遍历
//元素顺序：保持插入顺序
//适用场景：
//需要保持元素顺序的集合
//需要频繁按位置访问元素的场景
//如敌人队列、任务列表等

//选择建议：
//需要按键名快速查找 → 字典
//需要保持元素顺序 → 列表
//数据有明确标识 → 字典
//数据只是简单集合 → 列表
//在您的存档系统中使用字典非常合适，因为需要通过明确的键名（如"playerPosition"）来快速存取特定的游戏数据。

/*
 *PS: 时间复杂度核心思想总结：
 * 
 * 1. O(1) - 常数时间
 * 核心思想：效率极高且稳定，操作时间与数据量完全无关
 * 生活比喻：用钥匙开一把特定的锁。无论钥匙串上有10把还是1000把钥匙，找到并打开那把锁的速度都一样
 * 对应数据结构：字典的按键访问、数组的按索引访问
 * 
 * 2. O(log n) - 对数时间  
 * 核心思想：效率极高且几乎不随数据量增长，数据量翻倍时操作仅需增加一步
 * 生活比喻：查英汉字典。不必从头翻到尾，而是根据字母区间快速二分，不断缩小范围
 * 对应数据结构：二叉搜索树的查找、有序列表的二分查找
 * 
 * 3. O(n) - 线性时间
 * 核心思想：效率与数据量成正比，数据量增大多少倍，耗时也大致增大多少倍
 * 生活比喻：在一条没有门牌号的街上，从头到尾挨家挨户找人
 * 对应数据结构：未排序的列表或数组的线性查找
 * 
 * 4. O(n²) - 平方时间
 * 核心思想：效率随数据量增长而急剧下降，当数据量较大时性能会变得非常差
 * 生活比喻：举办见面会，n个人每个人都要与其他n-1个人逐一握手
 * 对应数据结构/算法：简单的嵌套循环（如冒泡排序、选择排序）
 */
