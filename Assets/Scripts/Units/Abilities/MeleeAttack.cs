using System.Collections;

public class AbilityMeleeAttack : Ability
{
    public override string AbilityName => (_element == Element.Physical) ?
        "Melee Attack" :
        string.Format("Melee Attack ({0})", _element.ToString());
    public override string Description => (_element == Element.Physical) ?
        "Attack an adjancent enemy." :
        string.Format("Attack an adjancent enemy and apply [{0}] on hit", _element.ToString());
    public override string Icon => "Melee Attack";
    public override int ActionCost => 2;
    public override TargetType TargetType => TargetType.Enemy;

    private Element _element;

    public AbilityMeleeAttack() { _element = Element.Physical; }
    public AbilityMeleeAttack(Element element) { _element = element; }

    public override IEnumerator Activate(Unit unit, object target)
    {
        UnitAnimator animator = unit.Animator;
        Unit targetUnit = (Unit)target;

        yield return unit.StartCoroutine(animator.RotateLookAt(targetUnit.transform.localPosition));
        yield return unit.StartCoroutine(animator.StopCrouching());
        yield return unit.StartCoroutine(animator.AttackMelee());

        if (!RandomExtensions.GetChance(unit.Stats.BaseAimPercent()))
            targetUnit.StartCoroutine(targetUnit.Canvas.PanelShow(targetUnit.Canvas.PanelMiss));
        else
        {
            int dmg = UnityEngine.Random.Range(((Enemy)unit).Stats.MinDamage, ((Enemy)unit).Stats.MaxDamage + 1);
            targetUnit.Health.MakeHit(dmg, _element, unit);
        }

        yield return unit.StartCoroutine(animator.StartCrouching());
        yield return unit.StartCoroutine(animator.CrouchRotateHideBehindShelter(unit.transform.localPosition));
    }
}
