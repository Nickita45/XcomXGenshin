using System.Collections;

public class AbilityShoot : Ability
{
    public override string AbilityName => "Shoot";
    public override string Description => "Shoot Description";
    public override int ActionCost => 2;
    public override TargetType TargetType => TargetType.Enemy;
    public override IEnumerator Activate(Unit unit, object target)
    {
        UnitAnimator animator = unit.Animator;
        Unit targetUnit = (Unit)target;

        yield return unit.StartCoroutine(animator.RotateLookAt(targetUnit.transform.localPosition));
        yield return unit.StartCoroutine(animator.StopCrouching());
        yield return unit.StartCoroutine(animator.StartAttackRanged());

        IEnumerator IEnumeratorActionMethod()
        {
            yield return unit.StartCoroutine(animator.StopAttackRanged());
            yield return unit.StartCoroutine(animator.StartCrouching());
            yield return unit.StartCoroutine(animator.CrouchRotateHideBehindShelter(unit.transform.localPosition));
        }

        // Shoot
        yield return unit.StartCoroutine(Manager.ShootManager.Shoot(
            unit,
            targetUnit,
            GunType.Automatic,
            IEnumeratorActionMethod()
        ));
    }
}
