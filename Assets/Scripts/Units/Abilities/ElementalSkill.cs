using System.Collections;
using UnityEngine;

// A placeholder for the elemental skills that would be added in future
public class AbilityElementalSkill : Ability
{
    private Element _element;

    public AbilityElementalSkill(Element element)
    {
        _element = element;
    }

    public override string AbilityName => string.Format("Elemental Skill ({0})", _element);
    public override string Description => "Elemental Skill Description";
    public override string Icon => _element.ToString();
    public override int ActionCost => 2;
    public override TargetType TargetType => TargetType.Self;
    public override IEnumerator Activate(Unit unit, object target)
    {
        Debug.Log("Elemental Skill");
        yield return new WaitForSeconds(0.5f);
        yield return null;
    }
}
