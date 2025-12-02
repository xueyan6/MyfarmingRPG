using UnityEngine;


[System.Serializable]
public class NPCScheduleEvent
{
    //仅为数据容器，会向NPCPath传递有关NPC必须做什么的信息
    public int hour;//生效的事件
    public int minute;//生效的事件
    public int priority;//本来有几件事情时间一样，通过该字段区分它们的优先级
    public int day;
    public Weather weather;
    public Season season;
    public SceneName toSceneName;
    public GridCoordinate toGridCoordinate;
    public Direction npcFacingDirectionAtDestination = Direction.none;
    public AnimationClip animationAtDestination;

    public int Time
    {
        get
        {
            return (hour * 100) + minute;
        }
    }

    public NPCScheduleEvent(int hour, int minute, int priority, int day, Weather weather, Season season, SceneName toSceneName, GridCoordinate toGridCoordinate, AnimationClip animationAtDestination)
    {
        this.hour = hour;
        this.minute = minute;
        this.priority = priority;
        this.day = day;
        this.weather = weather;
        this.season = season;
        this.toSceneName = toSceneName;
        this.toGridCoordinate = toGridCoordinate;
        this.animationAtDestination = animationAtDestination;
    }

    public NPCScheduleEvent()
    {

    }

    public override string ToString()
    {
        return $"Time: {Time}, Priority: {priority}, Day: {day} Weather: {weather}, Season: {season}";
    }
}