using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "so_AnimationType", menuName = "Scriptable Objects/Animation/Animation Type")]
public class SO_AnimationType : ScriptableObject
{
    public AnimationClip animationClip;  // a reference to the actual animation clip  ��������
    public AnimationName animationName;  // an enum of the animation name like "AnimationName.IdleUp"  ��������
    public CharacterPartAnimator characterPart; // the gameobject name the Animator is on that controls these animation clips e.g. "arms" ����λ�ö���
    public PartVariantColour partVariantColour; // to enable colour variations on a animation type to be specified e.g. "none", "bronze", "silver", "gold" ������ɫ�仯
    public PartVariantType partVariantType; // the variant type to specify what variant this animation type refers to e.g. "none", "carry", "hoe", "pickaxe", "axe"..etc ��������
}