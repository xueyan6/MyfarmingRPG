using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TimeManager : SingletonMonobehaviour<TimeManager>
{
    // ��Ϸʱ�������ʼ��
    private int gameYear = 1;// ��Ϸ��ǰ��ݣ���ʼֵ1��
    private Season gameSeason = Season.Spring;// ��Ϸ��ǰ���ڣ���ʼ������
    private int gameDay = 1;// ��ǰ�·ݵĵڼ���
    private int gameHour = 6;// ��ǰСʱ��24Сʱ�ƣ�
    private int gameMinute = 30;// ��ǰ����
    private int gameSecond = 0;// ��ǰ����
    private string gameDayOfWeek = "Mon";// ��ǰ���ڼ���ö��ֵ��

    // ��Ϸʱ�ӿ��Ʊ���
    private bool gameClockPaused = false;// �Ƿ���ͣʱ��
    private float gameTick = 0f;// ��Ϸʱ���ۻ�

    private void Start()
    {
        // ���������ƽ��¼������ݵ�ǰ����ʱ�������
        EventHandler.CallAdvanceGameMinuteEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
    }

    private void Update()
    {
        if (!gameClockPaused)// ����Ϸδ��ͣʱ
        {
            GameTick();// ִ����Ϸʱ������߼�
        }
    }

    private void GameTick()
    {
        gameTick += Time.deltaTime;// �ۻ���ʵʱ��

        if (gameTick >= Settings.secondsPerGameSecond)// �ﵽ��Ϸʱ����ֵ
        {
            gameTick -= Settings.secondsPerGameSecond;  // ���ü�����

            UpdateGameSecond();
        }
    }

    private void UpdateGameSecond()
    {
        gameSecond++;// ����+1

        if (gameSecond > 59)// �����������
        {
            gameSecond = 0;
            gameMinute++;// ����+1

            if (gameMinute > 59)// �����������
            {
                gameMinute = 0;
                gameHour++;// Сʱ+1

                if (gameHour > 23)// Сʱ�������
                {
                    gameHour = 0;
                    gameDay++;// ����+1

                    if (gameDay > 30)// ���������������ÿ��30�죩
                    {
                        gameDay = 1;

                        int gs = (int)gameSeason;// �����л�
                        gs++;

                        gameSeason = (Season)gs;

                        if (gs > 3)// �����������
                        {
                            gs = 0;
                            gameSeason = (Season)gs;

                            gameYear++;// ���+1

                            if (gameYear > 9999) gameYear = 1;

                            // ��������ƽ��¼�
                            EventHandler.CallAdvanceGameYearEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
                        }
                        // ���������ƽ��¼�
                        EventHandler.CallAdvanceGameSeasonEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
                    }

                    gameDayOfWeek = GetDayOfWeek();// �������ڼ�

                    // �������ƽ��¼�
                    EventHandler.CallAdvanceGameDayEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
                }

                // ����Сʱ�ƽ��¼�
                EventHandler.CallAdvanceGameHourEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
            }

            // ���������ƽ��¼�
            EventHandler.CallAdvanceGameMinuteEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);

        }

        //Call to advance game second event would go here if required����Ҫ���˴�������ƽ���Ϸ�ڶ��׶λ���ٻ�ָ�
    }

    private string GetDayOfWeek()
    {
        int totalDays = (((int)gameSeason) * 30) + gameDay;// ����������
        int dayOfWeek = totalDays % 7;// ȡģ�õ���������

        switch (dayOfWeek)// ת���ַ���
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
    /// Advance 1 game minute����1��Ϸ����
    /// </summary>
    public void TestAdvanceGameMinute()
    {
        for (int i = 0; i < 60; i++)
        {
            UpdateGameSecond();
        }
    }

    /// <summary>
    /// Advance 1 day������Ϸ1��
    /// </summary>
    public void TestAdvanceGameDay()
    {
        for (int i = 0; i < 86400; i++)
        {
            UpdateGameSecond();
        }
    }

}


