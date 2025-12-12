using System;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : SingletonMonobehaviour<TimeManager>, ISaveable
{
    // 游戏时间变量初始化
    private int gameYear = 1;// 游戏当前年份（初始值1）
    private Season gameSeason = Season.Spring;// 游戏当前季节（初始春季）
    private int gameDay = 1;// 当前月份的第几天
    private int gameHour = 6;// 当前小时（24小时制）
    private int gameMinute = 30;// 当前分钟
    private int gameSecond = 0;// 当前秒数
    private string gameDayOfWeek = "Mon";// 当前星期几（枚举值）

    // 游戏时钟控制变量
    private bool gameClockPaused = false;// 是否暂停时钟
    private float gameTick = 0f;// 游戏时间累积

    private string _iSaveableUniqueID;
    public string ISaveableUniqueID { get { return _iSaveableUniqueID; } set { _iSaveableUniqueID = value; } }

    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get { return _gameObjectSave; } set { _gameObjectSave = value; } }

    protected override void Awake()
    {
        base.Awake();

        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }

    private void OnEnable()
    {
        ISaveableRegister();

        EventHandler.BeforeSceneUnloadEvent += BeforeSceneUnloadFadeOut;
        EventHandler.AfterSceneLoadEvent += AfterSceneLoadFadeIn;
    }

    private void OnDisable()
    {
        ISaveableDeregister();

        EventHandler.BeforeSceneUnloadEvent -= BeforeSceneUnloadFadeOut;
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoadFadeIn;
    }

    private void BeforeSceneUnloadFadeOut()
    {
        gameClockPaused = true;
    }

    private void AfterSceneLoadFadeIn()
    {
        gameClockPaused = false;
    }

    private void Start()
    {
        // 触发分钟推进事件（传递当前完整时间参数）
        EventHandler.CallAdvanceGameMinuteEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
    }

    private void Update()
    {
        if (!gameClockPaused)// 当游戏未暂停时
        {
            GameTick();// 执行游戏时间更新逻辑
        }
    }

    private void GameTick()
    {
        gameTick += Time.deltaTime;// 累积真实时间

        if (gameTick >= Settings.secondsPerGameSecond)// 达到游戏时间阈值
        {
            gameTick -= Settings.secondsPerGameSecond;  // 重置计数器

            UpdateGameSecond();
        }
    }

    private void UpdateGameSecond()
    {
        gameSecond++;// 秒数+1

        if (gameSecond > 59)// 秒数溢出处理
        {
            gameSecond = 0;
            gameMinute++;// 分钟+1

            if (gameMinute > 59)// 分钟溢出处理
            {
                gameMinute = 0;
                gameHour++;// 小时+1

                if (gameHour > 23)// 小时溢出处理
                {
                    gameHour = 0;
                    gameDay++;// 天数+1

                    if (gameDay > 30)// 天数溢出处理（假设每月30天）
                    {
                        gameDay = 1;

                        int gs = (int)gameSeason;// 季节切换
                        gs++;

                        gameSeason = (Season)gs;

                        if (gs > 3)// 季节溢出处理
                        {
                            gs = 0;
                            gameSeason = (Season)gs;

                            gameYear++;// 年份+1

                            if (gameYear > 9999) gameYear = 1;

                            // 触发年份推进事件
                            EventHandler.CallAdvanceGameYearEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
                        }
                        // 触发季节推进事件
                        EventHandler.CallAdvanceGameSeasonEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
                    }

                    gameDayOfWeek = GetDayOfWeek();// 更新星期几

                    // 触发天推进事件
                    EventHandler.CallAdvanceGameDayEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
                }

                // 触发小时推进事件
                EventHandler.CallAdvanceGameHourEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
            }

            // 触发分钟推进事件
            EventHandler.CallAdvanceGameMinuteEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);

        }

        //Call to advance game second event would go here if required若需要，此处可添加推进游戏第二阶段活动的召唤指令。
    }

    private string GetDayOfWeek()
    {
        int totalDays = (((int)gameSeason) * 30) + gameDay;// 计算总天数
        int dayOfWeek = totalDays % 7;// 取模得到星期索引

        switch (dayOfWeek)// 转换字符串
        {
            case 1:
                return "Mon";
            case 2:
                return "Tue";
            case 3:
                return "Wed";
            case 4:
                return "Thu";
            case 5:
                return "Fri";
            case 6:
                return "Sat";
            case 0:
                return "Sun";
            default:
                return "";
        }
    }

    //用于计算每个移动步骤的时间
    public TimeSpan GetGameTime()
    {
        TimeSpan gameTime = new TimeSpan(gameHour, gameMinute, gameSecond);
        return gameTime;
    }

    public Season GetGameSeason()
    {
        return gameSeason;
    }

    // Advance 1 game minute加速1游戏分钟
    public void TestAdvanceGameMinute()
    {
        for (int i = 0; i < 60; i++)
        {
            UpdateGameSecond();
        }
    }

    /// <summary>
    /// Advance 1 day加速游戏1天
    /// </summary>
    public void TestAdvanceGameDay()
    {
        for (int i = 0; i < 86400; i++)
        {
            UpdateGameSecond();
        }
    }

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }

    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }

    public GameObjectSave ISaveableSave()
    {
        // 从游戏对象的存档数据中移除旧的持久化场景数据，避免重复
        GameObjectSave.sceneData.Remove(Settings.PersistentScene);

        // 创建一个新的场景保存对象，用于存储当前游戏时间状态
        SceneSave sceneSave = new SceneSave();

        // 初始化一个整数类型的字典，用于存储游戏时间的数值部分
        sceneSave.intDictionary = new Dictionary<string, int>();

        // 初始化一个字符串类型的字典，用于存储游戏时间的文本和枚举部分
        sceneSave.stringDictionary = new Dictionary<string, string>();

        // 将当前的游戏年份存入整数字典，键为"gameYear"
        sceneSave.intDictionary.Add("gameYear", gameYear);
        // 将当前的游戏天数存入整数字典，键为"gameDay"
        sceneSave.intDictionary.Add("gameDay", gameDay);
        // 将当前的游戏小时数存入整数字典，键为"gameHour"
        sceneSave.intDictionary.Add("gameHour", gameHour);
        // 将当前的游戏分钟数存入整数字典，键为"gameMinute"
        sceneSave.intDictionary.Add("gameMinute", gameMinute);
        // 将当前的游戏秒数存入整数字典，键为"gameSecond"
        sceneSave.intDictionary.Add("gameSecond", gameSecond);

        // 将当前的游戏星期几存入字符串字典，键为"gameDayOfWeek"
        sceneSave.stringDictionary.Add("gameDayOfWeek", gameDayOfWeek);
        // 将当前的游戏季节（枚举类型）转换为字符串后存入字符串字典，键为"gameSeason"
        sceneSave.stringDictionary.Add("gameSeason", gameSeason.ToString());

        // 将包含时间数据的场景保存对象添加到游戏对象的存档数据中，关联到持久化场景
        GameObjectSave.sceneData.Add(Settings.PersistentScene, sceneSave);

        // 返回包含所有时间存档数据的游戏对象保存实例
        return GameObjectSave;
    }

    public void ISaveableLoad(GameSave gameSave)
    {
        // 尝试从游戏存档数据中根据当前对象的唯一ID获取对应的存档数据
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameObjectSave gameObjectSave))
        {
            // 将从存档中获取的游戏对象保存数据赋给当前对象
            GameObjectSave = gameObjectSave;

            // 从游戏对象的存档数据中获取持久化场景的存档信息
            if (GameObjectSave.sceneData.TryGetValue(Settings.PersistentScene, out SceneSave sceneSave))
            {
                // 检查整数字典和字符串字典是否存在且不为空
                if (sceneSave.intDictionary != null && sceneSave.stringDictionary != null)
                {
                    // 从整数字典中尝试获取"gameYear"键对应的值
                    if (sceneSave.intDictionary.TryGetValue("gameYear", out int savedGameYear))
                    {
                        // 将从存档中读取的年份数据应用到当前游戏时间系统中
                        gameYear = savedGameYear;
                    }

                    // 从整数字典中尝试获取"gameDay"键对应的值
                    if (sceneSave.intDictionary.TryGetValue("gameDay", out int savedGameDay))
                    {
                        // 恢复游戏天数
                        gameDay = savedGameDay;
                    }

                    // 从整数字典中尝试获取"gameHour"键对应的值
                    if (sceneSave.intDictionary.TryGetValue("gameHour", out int savedGameHour))
                    {
                        // 恢复游戏小时数
                        gameHour = savedGameHour;
                    }

                    // 从整数字典中尝试获取"gameMinute"键对应的值
                    if (sceneSave.intDictionary.TryGetValue("gameMinute", out int savedGameMinute))
                    {
                        // 恢复游戏分钟数
                        gameMinute = savedGameMinute;
                    }

                    // 从整数字典中尝试获取"gameSecond"键对应的值
                    if (sceneSave.intDictionary.TryGetValue("gameSecond", out int savedGameSecond))
                    {
                        // 恢复游戏秒数
                        gameSecond = savedGameSecond;
                    }

                    // 从字符串字典中尝试获取"gameDayOfWeek"键对应的值
                    if (sceneSave.stringDictionary.TryGetValue("gameDayOfWeek", out string savedGameDayOfWeek))
                    {
                        // 恢复游戏星期几
                        gameDayOfWeek = savedGameDayOfWeek;
                    }

                    // 从字符串字典中尝试获取"gameSeason"键对应的值
                    if (sceneSave.stringDictionary.TryGetValue("gameSeason", out string savedGameSeason))
                    {
                        // 尝试将从存档中读取的字符串形式的季节解析为Season枚举类型
                        if (Enum.TryParse<Season>(savedGameSeason, out Season season))
                        {
                            // 如果解析成功，更新当前游戏季节
                            gameSeason = season;
                        }
                    }

                    // 重置游戏内部计时器（tick）为0，确保时间同步
                    gameTick = 0f;

                    // 调用事件处理器，触发游戏时间前进事件
                    // 该事件会通知游戏内其他系统时间已更新，需要同步刷新相关状态
                    EventHandler.CallAdvanceGameMinuteEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);

                    // 刷新游戏内时钟显示，确保UI与数据一致
                }
            }
        }
    }

    public void ISaveableStoreScene(string sceneName)
    {
        // Nothing required here since Time Manager is running on the persistent scene
    }

    public void ISaveableRestoreScene(string sceneName)
    {
        // Nothing required here since Time Manager is running on the persistent scene
    }

}








