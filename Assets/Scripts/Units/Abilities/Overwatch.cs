using System.Collections;
using UnityEngine;

public class AbilityOverwatch : Ability
{
    public override string AbilityName => "Overwatch";
    public override string Description => "Overwatch Description";
    public override int ActionCost => 2;
    public override TargetType TargetType => TargetType.Self;
    public override int MaxCooldown => 0;
    public override IEnumerator Activate(Unit unit, object target)
    {
        Debug.Log("Overwatch");
        Manager.TurnManager.UnitOverwatched.Add(unit);
        yield return new WaitForSeconds(0.5f);
        yield return null;
    }

}
