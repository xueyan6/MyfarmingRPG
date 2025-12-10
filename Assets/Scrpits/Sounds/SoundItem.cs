using UnityEngine;

[System.Serializable]
public class SoundItem
{
    public SoundName soundName;
    public AudioClip soundClip;
    public string soundDescription;//声音描述
    [Range(0.1f, 1.5f)]
    public float soundPitchRandomVariationMin = 0.8f;//声音音高随机变化最小值
    [Range(0.1f, 1.5f)]
    public float soundPitchRandomVariationMax = 1.2f;//声音音高随机变化最大值
    [Range(0f, 1f)]
    public float soundVolume = 1f;
}