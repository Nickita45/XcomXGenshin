using System.Collections;

public class AbilityMeleeAttack : Ability
{
    public override string AbilityName => "Melee Attack";
    public override string Description => "Melee Attack Decription";
    public override int ActionCost => 2;
    public override TargetType TargetType => TargetType.Enemy;

    private float HitChance = 0.5f;

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
            targetUnit.MakeHit(dmg);
        }

        yield return unit.StartCoroutine(animator.StartCrouching());
        yield return unit.StartCoroutine(animator.CrouchRotateHideBehindShelter(unit.transform.localPosition));
    }
}
