[System.Serializable]
public struct CharacterAttribute
{
    public CharacterPartAnimator characterPart;   // 示例: CharacterPartAnimator.arms 动画位置对象
    public PartVariantColour partVariantColour;  // 示例: ParVariantColour.none 动画颜色变化
    public PartVariantType partVariantType;  // 示例: PartVariantType.carry 动画类型

    public CharacterAttribute(CharacterPartAnimator characterPart, PartVariantColour partVariantColour, PartVariantType partVariantType)
    {
        this.characterPart = characterPart;
        this.partVariantColour = partVariantColour;
        this.partVariantType = partVariantType;
    }
}
