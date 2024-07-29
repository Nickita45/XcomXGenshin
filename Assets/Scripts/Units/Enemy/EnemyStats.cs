using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class EnemyStats : UnitStats, IShooter, IMelee
{
    [SerializeField]
    private string _configEnemyName;

    [SerializeField]
    private Sprite _icon;
    public Sprite Icon => _icon;

    public override int MaxHP() => ConfigurationManager.EnemiesDataJson[_configEnemyName].enemyBaseHealth;
    public override int BaseAimPercent() => ConfigurationManager.EnemiesDataJson[_configEnemyName].enemyBaseAim;
    public override int BaseActions() => 2;
    public override float Speed() => ConfigurationManager.EnemiesDataJson[_configEnemyName].enemySpeed + SpeedIncreaser;
    public override int MovementDistance() => ConfigurationManager.EnemiesDataJson[_configEnemyName].enemyMoveDistance;
    public override float VisionDistance() => ConfigurationManager.EnemiesDataJson[_configEnemyName].enemyRangedTargetDistance;
    public override string Name() => _configEnemyName;
    public override string Description() => ConfigurationManager.EnemiesDataJson[_configEnemyName].enemyDescription;

    public int MinShootDmg() => ConfigurationManager.EnemiesDataJson[_configEnemyName].enemyMinAttackValue;
    public int MaxShootDmg() => ConfigurationManager.EnemiesDataJson[_configEnemyName].enemyMaxAttackValue;
    public int RandomShootDmg() => UnityEngine.Random.Range(MinShootDmg(), MaxShootDmg() + 1);


    //in future to subclass, something like EnemyMeleeStats
    public int MinMeleeDmg() => ConfigurationManager.EnemiesDataJson[_configEnemyName].enemyMinAttackValue;

    public int MaxMeleeDmg() => ConfigurationManager.EnemiesDataJson[_configEnemyName].enemyMinAttackValue;

    public int RandomMeleeDmg() => UnityEngine.Random.Range(MinMeleeDmg(), MaxMeleeDmg() + 1);
    public int BaseMeleeAim() => BaseAimPercent();
}