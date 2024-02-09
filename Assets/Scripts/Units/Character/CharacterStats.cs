using System;

public class CharacterStats : UnitStats
{
    //Config atributes
    public int Index { get; set; }
    public string CharacterName() => ConfigurationManager.CharactersData[Index].characterName;
    public override float Speed() => ConfigurationManager.CharactersData[Index].characterSpeed;
    public override int MovementDistance() => ConfigurationManager.CharactersData[Index].characterMoveDistance;
    public override float VisionDistance() => 10f; // might change when the vision system is fully implemented
    public float AttackRangedDistance() => Math.Max(VisionDistance(), ConfigurationManager.CharactersData[Index].characterRangedTargetDistance);
    public int BaseAimCharacter => ConfigurationManager.CharactersData[Index].characterBaseAim;
    public override int MaxHP() => (ConfigurationManager.CharactersData != null) ? ConfigurationManager.CharactersData[Index].characterBaseHealth : 0;
    public override int BaseAimPercent() => 50;
    public GunType Weapon => (GunType)ConfigurationManager.CharactersData[Index].characterWeapon;
    public override int BaseActions() => 2;
    public Element Element => (Element)Enum.Parse(typeof(Element), ConfigurationManager.CharactersData[Index].element);
}
