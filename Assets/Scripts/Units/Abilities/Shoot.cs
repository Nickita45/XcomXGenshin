using System.Collections;

public class AbilityShoot : Ability
{
    public override string AbilityName => (_element == Element.Physical) ?
        "Shoot" :
        string.Format("Shoot ({0})", _element.ToString());
    public override string Description => (_element == Element.Physical) ?
        "Shoot an enemy." :
        string.Format("Shoot an enemy and apply [{0}] on hit", _element.ToString());
    public override string Icon => (_element == Element.Physical) ? 
        "Shoot" : 
        string.Format("{0}Shoot", _element.ToString()); 
        
    public override int ActionCost => 2;
    public override TargetType TargetType => TargetType.Enemy;

    private Element _element;

    public AbilityShoot() { _element = Element.Physical; }
    public AbilityShoot(Element element) { _element = element; }

    public override IEnumerator Activate(Unit unit, object target)
    {
        UnitAnimator animator = unit.Animator;
        Unit targetUnit = (Unit)target;

        yield return unit.StartCoroutine(animator.RotateLookAt(targetUnit.transform.localPosition));
        yield return unit.StartCoroutine(animator.StopCrouching());
        yield return unit.StartCoroutine(animator.StartAttackRanged());

        IEnumerator IEnumeratorActionMethod()
        {
            Manager.CameraManager.AnimatedCamera.Targets = targetUnit;
            yield return unit.StartCoroutine(animator.StopAttackRanged());
            yield return unit.StartCoroutine(animator.StartCrouching());
            yield return unit.StartCoroutine(animator.CrouchRotateHideBehindShelter(unit.transform.localPosition));
            //Maybe here is timer...
            Manager.StatusMain.SetStatusWaiting();
        }

        // Shoot

        GunType gunUsedInShooting = GunType.Automatic;
        if (unit is Character)
            gunUsedInShooting = ((Character)unit).Stats.Weapon;


        Manager.CameraManager.AnimatedCamera.Targets = unit;
        //Manager.StatusMain.SetStatusShooting();

        yield return unit.StartCoroutine(Manager.ShootManager.Shoot(
            unit,
            targetUnit,
            gunUsedInShooting,
            _element,
            IEnumeratorActionMethod()
        ));
    }
}
