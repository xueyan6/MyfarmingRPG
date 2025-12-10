using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : SingletonMonobehaviour<AudioManager>
{
    [SerializeField] private GameObject soundPrefab = null;

    [Header("Audio Sources")] 
    [SerializeField] private AudioSource ambientSoundAudioSource = null;//环境音频源
    [SerializeField] private AudioSource gameMusicAudioSource = null;//游戏音乐音频源 

    [Header("Audio Mixers")] 
    [SerializeField] private AudioMixer gameAudioMixer = null;//游戏音频混音器

    [Header("Audio Snapshots")] 
    [SerializeField] private AudioMixerSnapshot gameMusicSnapshot = null;//游戏音乐快照
    [SerializeField] private AudioMixerSnapshot gameAmbientSnapshot = null;//游戏环境快照

    // 音效列表SO
    [Header("Other")]
    [SerializeField] private SO_SoundList so_soundList = null;

    [SerializeField] private SO_SceneSoundsList so_sceneSoundsList = null;
    [SerializeField] private float defaultSceneMusicPlayTimeSeconds = 120f;//背景音乐播放时间
    [SerializeField] private float sceneMusicStartMinSecs = 20f;//开始播放的等待时间最小值
    [SerializeField] private float sceneMusicStartMaxSecs = 40f;//开始播放的等待时间最大值
    [SerializeField] private float musicTransitionsecs = 8f;//音乐过渡时间


    // 音效字典
    private Dictionary<SoundName, SoundItem> soundDictionary;
    // 场景音效字典
    private Dictionary<SceneName, SceneSoundsItem> sceneSoundsDictionary;
    //场景音效切换协程
    private Coroutine playSceneSoundsCoroutine;
    // 初始化音效字典
    protected override void Awake()
    {
        base.Awake();

        soundDictionary = new Dictionary<SoundName, SoundItem>();

        // 加载音效到字典
        foreach (SoundItem soundItem in so_soundList.soundDetails)
        {
            soundDictionary.Add(soundItem.soundName, soundItem);
        }

        // Initialise scene sounds dictionary 初始化场景音效字典 
        sceneSoundsDictionary = new Dictionary<SceneName, SceneSoundsItem>();

        // Load scene sounds dictionary 加载场景声音字典 
        foreach (SceneSoundsItem sceneSoundsItem in so_sceneSoundsList.sceneSoundsDetails)
        {
            sceneSoundsDictionary.Add(sceneSoundsItem.sceneName, sceneSoundsItem);
        }
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += PlaySceneSounds;
    }

    // 场景禁用时取消订阅
    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= PlaySceneSounds; // 取消事件订阅
    }

    // 场景音效播放逻辑
    private void PlaySceneSounds()
    {
        SoundItem musicSoundItem = null; // 音乐音效项
        SoundItem ambientSoundItem = null; // 环境音效项
        float musicPlayTime = defaultSceneMusicPlayTimeSeconds; // 默认音乐播放时间

        // 获取当前场景名称
        if (Enum.TryParse<SceneName>(SceneManager.GetActiveScene().name, true, out SceneName currentSceneName))
        {
            // 获取场景对应的音效配置
            if (sceneSoundsDictionary.TryGetValue(currentSceneName, out SceneSoundsItem sceneSoundsItem))
            {
                soundDictionary.TryGetValue(sceneSoundsItem.musicForScene, out musicSoundItem); // 获取音乐音效
                soundDictionary.TryGetValue(sceneSoundsItem.ambientSoundForScene, out ambientSoundItem); // 获取环境音效
            }
            else
            {
                return; // 无音效配置直接返回
            }

            // 停止已播放的场景音效
            if (playSceneSoundsCoroutine != null)
            {
                StopCoroutine(playSceneSoundsCoroutine);
            }

            // 启动场景音效协程
            playSceneSoundsCoroutine = StartCoroutine(PlaySceneSoundsRoutine(musicPlayTime, musicSoundItem, ambientSoundItem));
        }
    }

    // 场景音效播放协程
    private IEnumerator PlaySceneSoundsRoutine(float musicPlaySeconds, SoundItem musicSoundItem, SoundItem ambientSoundItem)
    {
        if (musicSoundItem != null && ambientSoundItem != null)
        {
            // 播放环境音效
            PlayAmbientSoundClip(ambientSoundItem, 0f);

            // 随机延迟播放音乐
            yield return new WaitForSeconds(UnityEngine.Random.Range(sceneMusicStartMinSecs, sceneMusicStartMaxSecs));

            // 播放音乐音效
            PlayMusicSoundClip(musicSoundItem, musicTransitionsecs);

            // 等待音乐播放时间
            yield return new WaitForSeconds(musicPlaySeconds);

            // 重新播放环境音效
            PlayAmbientSoundClip(ambientSoundItem, musicTransitionsecs);
        }
    }

    private void PlayMusicSoundClip(SoundItem musicSoundItem, float transitionTimeSeconds)
    {
        // 设置音量（分贝转换）
        gameAudioMixer.SetFloat("MusicVolume", ConvertSoundVolumeDecimalFractionToDecibels(musicSoundItem.soundVolume));

        // 设置音效剪辑并播放
        gameMusicAudioSource.clip = musicSoundItem.soundClip;
        gameMusicAudioSource.Play();

        // 切换到音乐音效快照
        gameMusicSnapshot.TransitionTo(transitionTimeSeconds);
    }


    // 播放环境音效
    private void PlayAmbientSoundClip(SoundItem ambientSoundItem, float transitionTimeSeconds)
    {
        // 设置音量（分贝转换）
        gameAudioMixer.SetFloat("AmbientVolume", ConvertSoundVolumeDecimalFractionToDecibels(ambientSoundItem.soundVolume));

        // 设置音效剪辑并播放
        ambientSoundAudioSource.clip = ambientSoundItem.soundClip;
        ambientSoundAudioSource.Play();

        // 切换到环境音效快照
        gameAmbientSnapshot.TransitionTo(transitionTimeSeconds);
    }

    // 音量分贝转换
    private float ConvertSoundVolumeDecimalFractionToDecibels(float volumeDecimalFraction)
    {
        // 将音量从0-1转换为-80到+20分贝
        return (volumeDecimalFraction * 100f - 80f);
    }

    // 播放音乐音效
    

    // 播放音效
    public void PlaySound(SoundName soundName)
    {
        if (soundDictionary.TryGetValue(soundName, out SoundItem soundItem) && soundPrefab != null)
        {
            // 从对象池获取音效对象
            GameObject soundGameObject = PoolManager.Instance.ReuseObject(soundPrefab, Vector3.zero, Quaternion.identity);

            Sound sound = soundGameObject.GetComponent<Sound>();

            // 设置音效参数
            sound.SetSound(soundItem);
            soundGameObject.SetActive(true);

            // 延迟禁用音效对象
            StartCoroutine(DisableSound(soundGameObject, soundItem.soundClip.length));
        }
    }

    // 延迟禁用音效对象协程
    private IEnumerator DisableSound(GameObject soundGameObject, float soundDuration)
    {
        yield return new WaitForSeconds(soundDuration);
        soundGameObject.SetActive(false);
    }

}