using UnityEngine;

// 组件必须附加到具有AudioSource的GameObject上
[RequireComponent(typeof(AudioSource))]
public class Sound : MonoBehaviour
{
    // 音频源组件引用
    private AudioSource audioSource;

    // 初始化时获取AudioSource组件
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // 设置音效参数
    public void SetSound(SoundItem soundItem)
    {
        // 设置音调随机范围
        audioSource.pitch = Random.Range(soundItem.soundPitchRandomVariationMin, soundItem.soundPitchRandomVariationMax);
        // 设置音量
        audioSource.volume = soundItem.soundVolume;
        // 设置音效剪辑
        audioSource.clip = soundItem.soundClip;
    }

    // 组件启用时播放音效
    private void OnEnable()
    {
        if (audioSource.clip != null)
        {
            audioSource.Play();
        }
    }

    // 组件禁用时停止音效
    private void OnDisable()
    {
        audioSource.Stop();
    }
}
