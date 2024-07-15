using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityMeleeAttack : Ability, IEnemyList, IPercent
{
    private IMelee _creator;


    public override string AbilityName => (_element == Element.Physical) ?
        "Melee Attack" :
        string.Format("Melee Attack ({0})", _element.ToString());
    public override string Description => (_element == Element.Physical) ?
        $"Attack with {_creator.BaseMeleeAim()}% an adjancent enemy." :
        string.Format($"Attack with {_creator.BaseMeleeAim()}% an adjancent enemy and apply {_element} on hit");
    public override string Icon => "Melee Attack";
    public override int ActionCost => 2;
    public override TargetType TargetType => TargetType.Enemy;
    public override bool IsAvailable => TargetUtils.GetAvailableTargets(Manager.TurnManager.SelectedCharacter)
                            .Any(n => Vector3.Distance(Manager.TurnManager.SelectedCharacter.ActualTerritory.GetCordinats(),
                                n.ActualTerritory.GetCordinats()) < 2);

    public override int MaxCooldown => 0;

    private Element _element;
    public HashSet<Enemy> GetVisibleEnemies() => TargetUtils.GetAvailableTargets(Manager.TurnManager.SelectedCharacter)
                                                    .Where(n => Vector3.Distance(Manager.TurnManager.SelectedCharacter.ActualTerritory.GetCordinats(),
                                                     n.ActualTerritory.GetCordinats()) < 2).ToHashSet();

    public AbilityMeleeAttack(Element element, Unit creator = null) 
    {
        Debug.Log(element);
        _element = element;
        
        if(creator != null) 
            _creator = creator.Stats as IMelee;
    }

    public override IEnumerator Activate(Unit unit, object target)
    {
        UnitAnimator animator = unit.Animator;
        Unit targetUnit = (Unit)target;

        yield return unit.StartCoroutine(animator.RotateLookAt(targetUnit.transform.localPosition));
        yield return unit.StartCoroutine(animator.StopCrouching());
        yield return unit.StartCoroutine(animator.AttackMelee());

        IMelee melee = unit.Stats as IMelee;
        if (target is not Entity && !RandomExtensions.GetChance(melee.BaseMeleeAim())) //mb problems in future for new entities
            targetUnit.StartCoroutine(targetUnit.Canvas.PanelShow(targetUnit.Canvas.PanelMiss));
        else
        {
            int dmg = melee.RandomMeleeDmg();
            targetUnit.Health.MakeHit(dmg, _element, unit);
        }

        yield return unit.StartCoroutine(animator.StartCrouching());
        yield return unit.StartCoroutine(animator.CrouchRotateHideBehindShelter(unit.transform.localPosition));
    }

    public (int percent, ShelterType shelter) GetCalculationProcents(Unit enemy)
    {
        return (_creator.BaseMeleeAim(), ShelterType.None);
    }
}
