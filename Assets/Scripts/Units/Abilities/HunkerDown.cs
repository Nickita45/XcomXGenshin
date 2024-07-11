using System.Collections;
using UnityEngine;
using ChanceResistance;

public class AbilityHunkerDown : Ability
{
    private Unit _creator;
    public AbilityHunkerDown(Unit creator)
    {
        _creator = creator;
    }
    public AbilityHunkerDown()
    {
    }
    public override string AbilityName => "Hunker Down";
    public override string Description => "Hunker Down Description";
    public override int ActionCost => 2;
    public override TargetType TargetType => TargetType.Self;
    public override int MaxCooldown => 0;
    public override IEnumerator Activate(Unit unit, object target)
    {
        Debug.Log("Hunker Down");
        unit.ChanceResistance.AddResistance(this.GetHashCode(), new HunkerDownChanceResistance());
        Manager.TurnManager.RoundBeginEvent += RemoveHunkerDown;
        yield return new WaitForSeconds(0.5f);
        yield return null;
    }

    private void RemoveHunkerDown()
    {
        _creator.ChanceResistance.RemoveResistance(this.GetHashCode());
        Manager.TurnManager.RoundBeginEvent -= RemoveHunkerDown;
    }
}