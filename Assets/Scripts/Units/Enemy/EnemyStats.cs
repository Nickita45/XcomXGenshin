using UnityEngine;

public class EnemyStats : UnitStats
{
    [SerializeField]
    private float _speed, _visionDistance;

    [SerializeField]
    private int _movementDistance, _minDmg, _maxDmg;

    public int MinDamage => _minDmg;
    public int MaxDamage => _maxDmg;

    public override int MaxHP() => 5;
    public override int BaseAimPercent() => 50;
    public override int BaseActions() => 2;
    public override float Speed() => _speed;
    public override int MovementDistance() => _movementDistance;
    public override float VisionDistance() => _visionDistance;
}
