using System.Collections;

public class AbilityMeleeAttack : Ability
{
    public override string AbilityName => (_element == null) ?
        "Melee Attack" :
        string.Format("Melee Attack ({0})", _element.ToString());
    public override string Description => (_element == null) ?
        "Attack an adjancent enemy." :
        string.Format("Attack an adjancent enemy and apply [{0}] on hit", _element.ToString());
    public override string Icon => "Melee Attack";
    public override int ActionCost => 2;
    public override TargetType TargetType => TargetType.Enemy;

    private float HitChance = 0.5f;

    private Element? _element = null;

    public AbilityMeleeAttack() { }
    public AbilityMeleeAttack(Element element) { _element = element; }

    public override IEnumerator Activate(Unit unit, object target)
    {
        UnitAnimator animator = unit.Animator;
        Unit targetUnit = (Unit)target;

        yield return unit.StartCoroutine(animator.RotateLookAt(targetUnit.transform.localPosition));
        yield return unit.StartCoroutine(animator.StopCrouching());
        yield return unit.StartCoroutine(animator.AttackMelee());

        if (UnityEngine.Random.value >= HitChance)
            targetUnit.StartCoroutine(targetUnit.Canvas.PanelShow(targetUnit.Canvas.PanelMiss));
        else
        {
            int dmg = UnityEngine.Random.Range(((Enemy)unit).Stats.MinDamage, ((Enemy)unit).Stats.MaxDamage + 1);
            targetUnit.StartCoroutine(targetUnit.Canvas.PanelShow(targetUnit.Canvas.PanelHit(dmg)));
            targetUnit.MakeHit(dmg, _element);
        }

        yield return unit.StartCoroutine(animator.StartCrouching());
        yield return unit.StartCoroutine(animator.CrouchRotateHideBehindShelter(unit.transform.localPosition));
    }
}
