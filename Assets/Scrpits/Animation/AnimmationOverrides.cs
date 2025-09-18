using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;


public class AnimationOverrides : MonoBehaviour
{
    [SerializeField] private GameObject character = null;
    [SerializeField] private SO_AnimationType[] soAnimationTypeArray = null;

    private Dictionary<AnimationClip, SO_AnimationType> animationTypeDictionaryByAnimation;
    private Dictionary<string, SO_AnimationType> animationTypeDictionaryByCompositeAttributeKey;

    private void Start()
    {
        // Initialise animation type dictionary keyed by animation clip初始化以AnimationClip为键的字典，用于直接通过动画片段查找对应的动画类型数据
        animationTypeDictionaryByAnimation = new Dictionary<AnimationClip, SO_AnimationType>();
        // 遍历所有配置的动画类型数据（SO_AnimationType脚本化对象数组）
        foreach (SO_AnimationType item in soAnimationTypeArray)
        {
            // 将动画片段作为键，关联对应的动画类型数据
            animationTypeDictionaryByAnimation.Add(item.animationClip, item);
        }

        // Initialise animation type dictionary keyed by string初始化复合键字典，用于通过多个属性组合查找动画类型数据
        animationTypeDictionaryByCompositeAttributeKey = new Dictionary<string, SO_AnimationType>();
        //// 再次遍历动画类型数据数组
        foreach (SO_AnimationType item in soAnimationTypeArray)
        {
            // 生成复合键：拼接角色部位+颜色变体+类型变体+动画名称的字符串
            string key = item.characterPart.ToString() + item.partVariantColour.ToString() + item.partVariantType.ToString() + item.animationName.ToString();
            // 将生成的复合键与动画类型数据关联存储
            animationTypeDictionaryByCompositeAttributeKey.Add(key, item);
        }

    }

    public void ApplyCharacterCustomisationParameters(List<CharacterAttribute> characterAttributesList)
    {

        // Loop through all character attributes and set the animation override controller for each遍历所有角色属性，并为每个属性设置动画覆盖控制器
        foreach (CharacterAttribute characterAttribute in characterAttributesList)
        {
            Animator currentAnimator = null;
            List<KeyValuePair<AnimationClip, AnimationClip>> animsKeyValuePairList = new List<KeyValuePair<AnimationClip, AnimationClip>>();

            string animatorSOAssetName = characterAttribute.characterPart.ToString();//获取当前部位的Animator资源名称（基于角色部位枚举）

            // Find animators in scene that match scriptable object animator type在场景中查找与可编程对象动画器类型匹配的动画器
            Animator[] animatorArray = character.GetComponentsInChildren<Animator>();

            foreach (Animator animator in animatorArray)
            {
                if (animator.name == animatorSOAssetName)
                {
                    currentAnimator = animator;
                    break;// 找到后立即终止循环
                }
            }

            // Get base current animations for animator创建AnimatorOverrideController用于动画替换
            AnimatorOverrideController aoc = new AnimatorOverrideController(currentAnimator.runtimeAnimatorController);

            List<AnimationClip> animationsList = new List<AnimationClip>(aoc.animationClips);
            // 遍历当前部位的所有动画
            foreach (AnimationClip animationClip in animationsList)
            {
                // find animation in dictionary从动画片段字典中查找要换的匹配动画配置
                SO_AnimationType so_AnimationType;
                bool foundAnimation = animationTypeDictionaryByAnimation.TryGetValue(animationClip, out so_AnimationType);

                if (foundAnimation)
                {
                    // 生成复合键：部位+颜色变体+类型变体+动画名称
                    string key = characterAttribute.characterPart.ToString() + characterAttribute.partVariantColour.ToString() +
                        characterAttribute.partVariantType.ToString() + so_AnimationType.animationName.ToString();

                    // 使用复合键查找替换动画配置
                    SO_AnimationType swapSO_AnimationType;
                    bool foundSwapAnimation = animationTypeDictionaryByCompositeAttributeKey.TryGetValue(key, out swapSO_AnimationType);

                    if (foundSwapAnimation)
                    {
                        // 找到替换动画时，记录原始动画和替换动画的键值对
                        AnimationClip swapAnimationClip = swapSO_AnimationType.animationClip;
                        animsKeyValuePairList.Add(new KeyValuePair<AnimationClip, AnimationClip>(animationClip, swapAnimationClip));
                    }
                }
            }

            // Apply animation updates to animation override controller and then update animator with the new controller将动画更新应用于动画覆盖控制器，然后使用新控制器更新动画器

            //应用所有动画替换
            aoc.ApplyOverrides(animsKeyValuePairList);

            // 将替换后的控制器应用到当前Animator
            currentAnimator.runtimeAnimatorController = aoc;

            //总结：‌初始化阶段‌（Start方法）
            //1.创建两个字典：animationTypeDictionaryByAnimation：以AnimationClip为键的快速查找表
            //animationTypeDictionaryByCompositeAttributeKey：以复合键（部位 + 颜色 + 类型 + 动画名）为键的精确匹配表
            //遍历动画配置数组（soAnimationTypeArray）填充两个字典
            //2.动画替换阶段‌（foreach循环）
            //定位目标Animator‌：
            //根据characterPart名称在角色子物体中查找对应Animator
            //创建替换控制器‌：
            //使用AnimatorOverrideController克隆原控制器
            //‌动画匹配过程‌：
            //遍历原控制器所有动画片段
            //通过动画片段字典查找配置
            //使用复合键匹配替换动画
            //应用替换‌：
            //将新旧动画对添加到替换列表
            //通过ApplyOverrides应用所有替换
        }

    }
}