using System.Collections.Generic;

public class NPCScheduleEventSort : IComparer<NPCScheduleEvent> // 定义事件排序类，实现IComparer接口
{
    public int Compare(NPCScheduleEvent npcScheduleEvent1, NPCScheduleEvent npcScheduleEvent2) // 实现比较方法
    {
        if (npcScheduleEvent1?.Time == npcScheduleEvent2?.Time) // 比较事件时间，使用空条件运算符防止空引用（?.空条件运算:若NPC事件为null，则返回null）
        {
            if (npcScheduleEvent1?.priority < npcScheduleEvent2?.priority) // 如果时间相同，按优先级排序
            {
                return -1; // 优先级低的事件排在前
            }
            else
            {
                return 1; // 优先级高的事件排在前
            }
        }
        else if (npcScheduleEvent1?.Time > npcScheduleEvent2?.Time) // 时间晚的事件排在后
        {
            return 1;
        }
        else if (npcScheduleEvent1?.Time < npcScheduleEvent2?.Time) // 时间早的事件排在前
        {
            return -1;
        }
        else
        {
            return 0; // 时间相同且优先级相同，认为相等
        }
    }
}