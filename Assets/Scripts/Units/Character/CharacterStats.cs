using System;

public class CharacterStats : UnitStats
{
    //Config atributes
    public int Index { get; set; }
    public string CharacterName() => ConfigurationManager.CharactersData.characters[Index].characterName;
    public override float Speed() => ConfigurationManager.CharactersData.characters[Index].characterSpeed;
    public override int MovementDistance() => ConfigurationManager.CharactersData.characters[Index].characterMoveDistance;
    public override float VisionDistance() => 10f; // might change when the vision system is fully implemented
    public float AttackRangedDistance() => Math.Max(VisionDistance(), ConfigurationManager.CharactersData.characters[Index].characterRangedTargetDistance);
    public int BaseAimCharacter => ConfigurationManager.CharactersData.characters[Index].characterBaseAim;
    public override int MaxHP() => (ConfigurationManager.CharactersData != null) ? ConfigurationManager.CharactersData.characters[Index].characterBaseHealth : 0;
    public override int BaseAimPercent() => 50;
    public GunType Weapon => (GunType)ConfigurationManager.CharactersData.characters[Index].characterWeapon;
    public override int BaseActions() => 2;
    public string Element => ConfigurationManager.CharactersData.characters[Index].element;
}
