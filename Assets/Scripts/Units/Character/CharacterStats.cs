using System;

public class CharacterStats : UnitStats, IShooter, IMelee
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

    public int MinShootDmg() => ConfigurationManager.GlobalDataJson.typeGun[(int)Weapon].minHitValue;

    public int MaxShootDmg() => ConfigurationManager.GlobalDataJson.typeGun[(int)Weapon].maxHitValue;

    public int RandomShootDmg() => UnityEngine.Random.Range(MinShootDmg(), MaxShootDmg() + 1);
    public int MinMeleeDmg() => ConfigurationManager.CharactersData[Index].characterMinMeleeDmg;
    public int MaxMeleeDmg() => ConfigurationManager.CharactersData[Index].characterMaxMeleeDmg;
    public int RandomMeleeDmg() => UnityEngine.Random.Range(MinMeleeDmg(), MaxMeleeDmg() + 1);
    public int BaseMeleeAim() => ConfigurationManager.CharactersData[Index].characterBaseAimMelee;

    public Element Element => (Element)Enum.Parse(typeof(Element), ConfigurationManager.CharactersData[Index].element);
    public AbilitiesList[] AbilitiesLists => ConfigurationManager.CharactersData[Index].abilities;
}
