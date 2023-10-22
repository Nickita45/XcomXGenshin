using UnityEngine;

public class EnemyStats : UnitStats
{
    // Need to be automate in future
    [SerializeField]
    private int _index;

    public int MinDamage => ConfigurationManager.EnemiesDataJson[_index].enemyMinAttackValue;
    public int MaxDamage => ConfigurationManager.EnemiesDataJson[_index].enemyMaxAttackValue;

    public override int MaxHP() => ConfigurationManager.EnemiesDataJson[_index].enemyBaseHealth;
    public override int BaseAimPercent() => ConfigurationManager.EnemiesDataJson[_index].enemyBaseAim;
    public override int BaseActions() => 2;
    public override float Speed() => ConfigurationManager.EnemiesDataJson[_index].enemySpeed;
    public override int MovementDistance() => ConfigurationManager.EnemiesDataJson[_index].enemyMoveDistance;
    public override float VisionDistance() => ConfigurationManager.EnemiesDataJson[_index].enemyRangedTargetDistance;
}