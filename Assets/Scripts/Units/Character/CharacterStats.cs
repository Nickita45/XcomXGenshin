using System;

public class CharacterStats : UnitStats
{
    //Config atributes
    public int Index { get; set; }
    public override string Name() => ConfigurationManager.CharactersData[Index].characterName;
    public override string Description() => "Here will be character description.";
    public override float Speed() => ConfigurationManager.CharactersData[Index].characterSpeed;
    public override int MovementDistance() => ConfigurationManager.CharactersData[Index].characterMoveDistance;
    public override float VisionDistance() => 10f; // might change when the vision system is fully implemented
    public float AttackRangedDistance() => Math.Max(VisionDistance(), ConfigurationManager.CharactersData[Index].characterRangedTargetDistance);
    public override int MaxHP() => (ConfigurationManager.CharactersData != null) ? ConfigurationManager.CharactersData[Index].characterBaseHealth : 0;
    public override int BaseAimPercent() => ConfigurationManager.CharactersData[Index].characterBaseAim;
    public GunType Weapon => (GunType)ConfigurationManager.CharactersData[Index].characterWeapon;
    public override int BaseActions() => 2;
    public Element Element => (Element)Enum.Parse(typeof(Element), ConfigurationManager.CharactersData[Index].element);
}
