using UnityEngine;

public class EnemyStats : UnitStats
{
    [SerializeField]
    private string _configEnemyName;

    [SerializeField]
    private Sprite _icon;
    public Sprite Icon => _icon;

    public int MinDamage => ConfigurationManager.EnemiesDataJson[_configEnemyName].enemyMinAttackValue;
    public int MaxDamage => ConfigurationManager.EnemiesDataJson[_configEnemyName].enemyMaxAttackValue;

    public override int MaxHP() => ConfigurationManager.EnemiesDataJson[_configEnemyName].enemyBaseHealth;
    public override int BaseAimPercent() => ConfigurationManager.EnemiesDataJson[_configEnemyName].enemyBaseAim;
    public override int BaseActions() => 2;
    public override float Speed() => ConfigurationManager.EnemiesDataJson[_configEnemyName].enemySpeed;
    public override int MovementDistance() => ConfigurationManager.EnemiesDataJson[_configEnemyName].enemyMoveDistance;
    public override float VisionDistance() => ConfigurationManager.EnemiesDataJson[_configEnemyName].enemyRangedTargetDistance;
}