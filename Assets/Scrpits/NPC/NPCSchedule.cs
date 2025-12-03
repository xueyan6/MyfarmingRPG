using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NPCPath))] // 要求游戏对象必须附加NPCPath组件，否则自动添加
public class NPCSchedule : MonoBehaviour // 定义NPC日程管理类
{
    [SerializeField] private SO_NPCScheduleEventList so_NPCScheduleEventList = null; // 序列化字段，用于在Inspector面板设置NPC日程事件列表
    private SortedSet<NPCScheduleEvent> npcScheduleEventSet; // 存储排序后的NPC日程事件集合
    private NPCPath npcPath; // NPC路径组件引用

    private void Awake() 
    {
        // Load NPC schedule event list into a sorted set将NPC日程事件列表加载到排序集合中
        npcScheduleEventSet = new SortedSet<NPCScheduleEvent>(new NPCScheduleEventSort()); // 使用自定义比较器创建排序集合

        foreach (NPCScheduleEvent npcScheduleEvent in so_NPCScheduleEventList.npcScheduleEventList) // 遍历日程事件列表
        {
            npcScheduleEventSet.Add(npcScheduleEvent); // 将每个事件添加到排序集合中
        }

        // Get NPC Path Component
        npcPath = GetComponent<NPCPath>(); // 获取同一游戏对象上的NPCPath组件
    }

    private void OnEnable() // 当组件启用时调用
    {
        EventHandler.AdvanceGameMinuteEvent += GameTimeSystem_AdvanceMinute; // 订阅游戏时间前进事件
    }

    private void OnDisable() // 当组件禁用时调用
    {
        EventHandler.AdvanceGameMinuteEvent -= GameTimeSystem_AdvanceMinute; // 取消订阅游戏时间前进事件
    }

    private void GameTimeSystem_AdvanceMinute(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek, int gameHour, int gameMinute, int gameSecond) // 游戏时间每分钟前进时调用的方法
    {
        int time = (gameHour * 100) + gameMinute; // 将小时和分钟转换为整数时间表示（如1430表示14:30）

        // Attempt to get matching schedule 尝试获取匹配的日程安排
        NPCScheduleEvent matchingNPCScheduleEvent = null; // 初始化匹配的日程事件为null

        foreach (NPCScheduleEvent npcScheduleEvent in npcScheduleEventSet) // 遍历排序后的日程事件集合
        {
            if (npcScheduleEvent.Time == time) // 检查事件时间是否匹配当前游戏时间
            {
                // Time match now check if paramenters match
                if (npcScheduleEvent.day != 0 && npcScheduleEvent.day != gameDay) // 检查日期是否匹配（0表示任何日期）
                    continue; // 日期不匹配，跳过当前事件

                if (npcScheduleEvent.season != Season.none && npcScheduleEvent.season != gameSeason) // 检查季节是否匹配（none表示任何季节）
                    continue; // 季节不匹配，跳过当前事件

                if (npcScheduleEvent.weather != Weather.none && npcScheduleEvent.weather != GameManager.Instance.currentWeather) // 检查天气是否匹配（none表示任何天气）
                    continue; // 天气不匹配，跳过当前事件

                matchingNPCScheduleEvent = npcScheduleEvent; // 所有条件匹配，记录该事件
                break; // 找到匹配事件，退出循环
            }
            else if (npcScheduleEvent.Time > time) // 如果事件时间已超过当前时间
            {
                break; // 由于集合已排序，后续事件时间都更晚，退出循环
            }
        }

        // Now test is matchingSchedule != null and do something检查是否找到了匹配的日程事件
        if (matchingNPCScheduleEvent != null) 
        {
            // Build path for matching schedule 构建匹配日程的路径
            npcPath.BuildPath(matchingNPCScheduleEvent); // 为匹配的日程事件构建移动路径
        }

    }

}
