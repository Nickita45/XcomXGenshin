using ParticleSystemFactory;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityBigSlimeJump : Ability
{
    private int _attackRange = 1;

    public override string AbilityName => "Jump";
    public override string Icon => "Shoot";
    public override string Description => "Leaps into the sky and freezes overhead";

    public override int ActionCost => 2;

    public override TargetType TargetType => TargetType.Self;
    private Element _element;

    public bool InAir { get; private set; }

    public override int MaxCooldown => 2;

    public AbilityBigSlimeJump() { _element = Element.Physical; }
    public AbilityBigSlimeJump(Element element) { _element = element; }

    public override IEnumerator Activate(Unit unit, object target)
    {
        if(!InAir)
            yield return Jump(unit);
        else
            yield return Fall(unit, target);
    }

    private IEnumerator Jump(Unit unit)
    {
        unit.StartCoroutine(unit.Canvas.PanelShow(unit.Canvas.PanelActionInfo("Jump"), 2));

        TerritroyReaded airTerritory = unit.ActualTerritory;
        var list = new List<Vector3> { airTerritory.GetCordinats() };
        for (int i = 0; i < 3; i++)
        {
            if (airTerritory.IndexUp.Count == 0) continue;

            airTerritory = Manager.Map[airTerritory.IndexUp.First()];
            list.Add(airTerritory.GetCordinats());
        }

        yield return unit.StartCoroutine(Manager.MovementManager.MoveUnitToTerritory(unit, list, airTerritory));
        InAir = true;
    }

    private IEnumerator Fall(Unit unit, object target)
    {
        unit.StartCoroutine(unit.Canvas.PanelShow(unit.Canvas.PanelActionInfo("Fall"), 2));

        TerritroyReaded findedTerritory = unit.ActualTerritory;
        if(target != null)
        {
            findedTerritory = (target as Unit).ActualTerritory.GetRandomTerritoryNearBy();
        } else
        {
            for (int i = 0; i < 3; i++)
            {
                if (findedTerritory.IndexDown.Count == 0) continue;

                findedTerritory = Manager.Map[findedTerritory.IndexDown.First()];
            }
        }

        var list = new List<Vector3>
        {
            unit.ActualTerritory.GetCordinats(),
            findedTerritory.GetCordinats()
        };

        unit.Stats.SpeedIncreaser = 5; //mb to config
        yield return unit.StartCoroutine(Manager.MovementManager.MoveUnitToTerritory(unit, list, findedTerritory));
        unit.Stats.SpeedIncreaser = 0;

        ParticleSystemFactoryCreator.CreateParticle(ParticleType.AlbedoFlower, new ParticleData
        (
           distance: _attackRange, position: unit.ActualTerritory.GetCordinats(), parent: Manager.MainParent.transform
        ));

        if (target != null)
            unit.Animator.RotateLookAtImmediate((target as Unit).ActualTerritory.GetCordinats());

        var adjancentAUnits = Manager.Map.GetAdjancentUnits(_attackRange, unit);
        foreach(var item in adjancentAUnits)
        {
            if (item is Enemy || item == unit)
                continue;

            int dmg = UnityEngine.Random.Range((unit as Enemy).Stats.MinDamage, (unit as Enemy).Stats.MaxDamage + 1);
            item.Health.MakeHit(dmg, _element, unit);
        }
        InAir = false;
        ActualCooldown = MaxCooldown;
    }

}
