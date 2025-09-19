using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TimeManager : SingletonMonobehaviour<TimeManager>
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

    /// <summary>
    /// Advance 1 game minute加速1游戏分钟
    /// </summary>
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

}


