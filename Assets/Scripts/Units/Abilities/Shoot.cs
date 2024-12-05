using System.Collections;
using System.Collections.Generic;

public class AbilityShoot : Ability, IEnemyList, IPercent
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
    public override int MaxCooldown => 0;

    public override bool IsAvailable => TargetUtils.GetAvailableTargets(Manager.TurnManager.SelectedCharacter).Count > 0;
    public HashSet<Enemy> GetVisibleEnemies() => TargetUtils.GetAvailableTargets(Manager.TurnManager.SelectedCharacter);

    private Element _element;

    public AbilityShoot() { _element = Element.Physical; }
    public AbilityShoot(Element element) { _element = element; }


    public override IEnumerator Activate(Unit unit, object target)
    {
        UnitAnimator animator = unit.Animator;
        var targetUnit = (Unit)target;
        ShootManager.TargetUnit = targetUnit;

        yield return unit.StartCoroutine(animator.RotateLookAt(targetUnit.transform.localPosition));
        yield return unit.StartCoroutine(animator.StopCrouching());
        yield return unit.StartCoroutine(animator.StartAttackRanged());

        IEnumerator IEnumeratorActionMethod()
        {
            Manager.CameraManager.AnimatedCamera.Targets = targetUnit;
            yield return unit.StartCoroutine(animator.StopAttackRanged());
            yield return unit.StartCoroutine(animator.StartCrouching());
            yield return unit.StartCoroutine(animator.CrouchRotateHideBehindShelter(unit.transform.localPosition));
            ShootManager.TargetUnit = null;
            //if(unit is Character)
            //    Manager.StatusMain.SetStatusWaitingWithNonFog();
            //else
            //    Manager.StatusMain.SetStatusWaiting();
            //Maybe here is timer...
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

    public (int percent, ShelterType shelter) GetCalculationProcents(Unit enemy)
    {
       Character character = Manager.TurnManager.SelectedCharacter;

        (int percent, ShelterType shelter) =
            AimUtils.CalculateHitChance(
                character.ActualTerritory,
                enemy.ActualTerritory,
                character.Stats.Weapon,
                character.Stats.BaseAimPercent()
            );

        return (percent, shelter);
    }
}
