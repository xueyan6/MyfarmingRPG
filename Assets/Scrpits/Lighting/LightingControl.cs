using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightingControl : MonoBehaviour
{
    [SerializeField] private LightingSchedule lightingSchedule;//灯光管理
    [SerializeField] private bool isLightFlicker = false;//灯光是否闪烁
    [SerializeField][Range(0f, 1f)] private float lightFlickerIntensity;//灯光闪烁强度设定
    [SerializeField][Range(0f, 0.2f)] private float lightFlickerTimeMin;//灯光最小闪烁时间
    [SerializeField][Range(0f, 0.2f)] private float lightFlickerTimeMax;//灯光最大闪烁时间

    private Light2D light2D;

    private Dictionary<string, float> lightingBrightnessDictionary = new Dictionary<string, float>();//灯效字典（字符串键为季节+小时，值为光照强度）
    private float currentLightIntensity;//当前光照强度
    private float lightFlickerTimer = 0f;//灯光闪烁计时器
    private Coroutine fadeInLightRoutine;//淡入灯光程序

    private void Awake()
    {
        //Get 2D light 获取2D光源
        light2D = GetComponentInChildren<Light2D>();

        // disable if no light2D 若无2D光源则禁用
        if (light2D == null)
            enabled = false;

        // populate lighting brightness dictionary 填充照明亮度字典
        foreach (LightingBrightness lightingBrightness in lightingSchedule.lightingBrightnessArray)
        {
            string key = lightingBrightness.season.ToString() + lightingBrightness.hour.ToString();

            // 验证键是否已存在
            if (!lightingBrightnessDictionary.ContainsKey(key))
            {
                lightingBrightnessDictionary.Add(key, lightingBrightness.lightIntensity);
            }
            else
            {
                // 处理重复键的情况（例如记录日志或抛出异常）
                Debug.Log($"重复键: {key} 已存在，跳过添加");
            }
        }
    }

    private void OnEnable() 
    {
        // Subscribe to events订阅事件  
        EventHandler.AdvanceGameHourEvent += EventHandler_AdvanceGameHourEvent ; 
        EventHandler.AfterSceneLoadEvent += EventHandler_AfterSceneLoadEvent; 

    }

    private void OnDisable()
    {
        EventHandler.AdvanceGameHourEvent -= EventHandler_AdvanceGameHourEvent;
        EventHandler.AfterSceneLoadEvent -= EventHandler_AfterSceneLoadEvent;
    }

    // Advance game hour event handler提前游戏小时事件处理程序
    private void EventHandler_AdvanceGameHourEvent(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek, int gameHour, int gameMinute, int gameSecond)
    {
        SetLightingIntensity(gameSeason, gameHour, true);
    }

    // After scene loaded event handler场景加载完成后的事件处理程序
    private void EventHandler_AfterSceneLoadEvent()
    {
        SetLightingAfterSceneLoaded();
    }

    //Handle light flicker timer处理灯光闪烁计时器
    private void Update()

    {
        if (isLightFlicker)
            lightFlickerTimer-= Time.deltaTime;
    }

    // Handle flicker or set light2d intensity处理闪烁或设置Light2D强度
    private void LateUpdate()
    {

        if (lightFlickerTimer <= 0f && isLightFlicker)
        {
            LightFlicker();
        }
        else
        {

            light2D.intensity = currentLightIntensity;
        }
    }


    // After the scene is loaded get the season and hour to set the light intensity 场景加载完成后获取季节和时间，用于设置光照强度
    private void SetLightingAfterSceneLoaded()
    {
        Season gameSeason = TimeManager.Instance.GetGameSeason();
        int gameHour = TimeManager.Instance.GetGameTime().Hours;

        // Set light intensity immediately without fading in 立即设置光照强度，不进行渐变效果 
        SetLightingIntensity(gameSeason, gameHour, false);
    }

    // Set the light intensity based on season and game hour根据季节和游戏时间设置光照强度
    private void SetLightingIntensity(Season gameSeason, int gameHour, bool fadein)
    {
        int i = 0;

        //Get light intensity for nearest game hour that is less than or equal to the current game hour for the same season 
        //获取当前游戏时段所属季节内，最近且小于或等于当前游戏时段的游戏时段的光照强度 
        while (i <= 23)
        {
            // check dictionary for value 在字典中查找值
            string key = gameSeason.ToString() + (gameHour).ToString();
            if (lightingBrightnessDictionary.TryGetValue(key, out float targetLightingIntensity))
            {

                if (fadein)
                {
                    // stop fade in coroutine if already running  若淡入协程已运行则停止 
                    if (fadeInLightRoutine != null) StopCoroutine(fadeInLightRoutine);

                    // fade in to new light intensity level 淡入至新光照强度级别
                    fadeInLightRoutine = StartCoroutine(FadeLightRoutine(targetLightingIntensity));
                }

                else

                {
                    currentLightIntensity = targetLightingIntensity;
                }
                break;

            }
            i++;
            gameHour--;
            if (gameHour < 0)
            {
                gameHour = 23;
            }

        }
    }


    private IEnumerator FadeLightRoutine(float targetLightingIntensity)
    {
        float fadeDuration = 5f;

        // Calculate how fast the light should fade based current and target intensity and duration 
        //根据当前亮度、目标亮度和淡出时长计算光线应以多快速度变暗
        float fadeSpeed = Mathf.Abs(currentLightIntensity - targetLightingIntensity) / fadeDuration;

        // loop while fading  淡入淡出循环
        while (!Mathf.Approximately(currentLightIntensity, targetLightingIntensity))
        {
            // move the light intensity towards it's target intensity 使光照强度向目标强度移动 
            currentLightIntensity = Mathf.MoveTowards(currentLightIntensity, targetLightingIntensity,fadeSpeed * Time.deltaTime);

            yield return null;
        }

        currentLightIntensity = targetLightingIntensity;
    }

    private void LightFlicker()
    {
        // calculate a random flicker intensity 计算随机闪烁强度 
        light2D.intensity = UnityEngine.Random.Range(currentLightIntensity, currentLightIntensity + (currentLightIntensity * lightFlickerIntensity));

        // if the light is to flicker calculate a random flicker interval 若灯光需闪烁，则计算随机闪烁间隔 
        lightFlickerTimer = UnityEngine.Random.Range(lightFlickerTimeMin, lightFlickerTimeMax);
    }
}
