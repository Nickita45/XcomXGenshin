using System.Collections;
using UnityEngine;

public class AbilityHunkerDown : Ability
{
    public override string AbilityName => "Hunker Down";
    public override string Description => "Hunker Down Description";
    public override int ActionCost => 2;
    public override TargetType TargetType => TargetType.Self;
    public override IEnumerator Activate(Unit unit, object target)
    {
        Debug.Log("Hunker Down");
        yield return new WaitForSeconds(0.5f);
        yield return null;
    }
}