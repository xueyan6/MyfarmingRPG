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
        // Update time将分钟数调整为10的倍数（如23→20，37→30），简化时间显示精度
        gameMinute = gameMinute - (gameMinute % 10);

        string ampm = "";
        string minute;

        // 判断当前是上午(AM)还是下午(PM)
        if (gameHour >= 12)
        {
            ampm = " pm";// 12小时制下午标记
        }
        else
        {
            ampm = " am";// 12小时制上午标记
        }

        //// 将24小时制转换为12小时制（13-23点转为1-11点）
        if (gameHour >= 13)
        {
            gameHour -= 12;
        }

        // 格式化分钟显示（小于10补零）
        if (gameMinute < 10)
        {
            minute = "0" + gameMinute.ToString();// 如5→"05"
        }
        else
        {
            minute = gameMinute.ToString();// 如15→"15"
        }

        // 组合时间字符串（示例："3:05 pm"）
        string time = gameHour.ToString() + ":" + minute + ampm;

        // 更新UI文本组件
        timeText.SetText(time);// 显示时间（时:分 AM/PM）
        dateText.SetText(gameDayOfWeek + ". " + gameDay.ToString());// 显示星期和日期（如"Mon. 15"）
        seasonText.SetText(gameSeason.ToString());// 显示季节（如"Spring"）
        yearText.SetText("Year " + gameYear);// 显示年份（如"Year 2"）

    }
}
