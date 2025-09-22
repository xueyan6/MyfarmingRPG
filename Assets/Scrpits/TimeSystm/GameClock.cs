using TMPro;
using UnityEngine;


public class GameClock : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText = null;
    [SerializeField] private TextMeshProUGUI dateText = null;
    [SerializeField] private TextMeshProUGUI seasonText = null;
    [SerializeField] private TextMeshProUGUI yearText = null;

    private void OnEnable()
    {
        EventHandler.AdvanceGameMinuteEvent += UpdateGameTime;
    }

    private void UpdateGameTime(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek, int gameHour, int gameMinute, int gameSecond)
    {
        // Update time������������Ϊ10�ı�������23��20��37��30������ʱ����ʾ����
        gameMinute = gameMinute - (gameMinute % 10);

        string ampm = "";
        string minute;

        // �жϵ�ǰ������(AM)��������(PM)
        if (gameHour >= 12)
        {
            ampm = " pm";// 12Сʱ��������
        }
        else
        {
            ampm = " am";// 12Сʱ��������
        }

        //// ��24Сʱ��ת��Ϊ12Сʱ�ƣ�13-23��תΪ1-11�㣩
        if (gameHour >= 13)
        {
            gameHour -= 12;
        }

        // ��ʽ��������ʾ��С��10���㣩
        if (gameMinute < 10)
        {
            minute = "0" + gameMinute.ToString();// ��5��"05"
        }
        else
        {
            minute = gameMinute.ToString();// ��15��"15"
        }

        // ���ʱ���ַ�����ʾ����"3:05 pm"��
        string time = gameHour.ToString() + ":" + minute + ampm;

        // ����UI�ı����
        timeText.SetText(time);// ��ʾʱ�䣨ʱ:�� AM/PM��
        dateText.SetText(gameDayOfWeek + ". " + gameDay.ToString());// ��ʾ���ں����ڣ���"Mon. 15"��
        seasonText.SetText(gameSeason.ToString());// ��ʾ���ڣ���"Spring"��
        yearText.SetText("Year " + gameYear);// ��ʾ��ݣ���"Year 2"��

    }
}
