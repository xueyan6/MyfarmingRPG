[System.Serializable]
public struct CharacterAttribute
{
    public CharacterPartAnimator characterPart;   // ʾ��: CharacterPartAnimator.arms ����λ�ö���
    public PartVariantColour partVariantColour;  // ʾ��: ParVariantColour.none ������ɫ�仯
    public PartVariantType partVariantType;  // ʾ��: PartVariantType.carry ��������

    public CharacterAttribute(CharacterPartAnimator characterPart, PartVariantColour partVariantColour, PartVariantType partVariantType)
    {
        this.characterPart = characterPart;
        this.partVariantColour = partVariantColour;
        this.partVariantType = partVariantType;
    }
}
