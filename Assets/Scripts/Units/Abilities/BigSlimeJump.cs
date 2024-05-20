using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityBigSlimeJump : Ability
{
    public override string AbilityName => "Jump";

    public override string Description => "Leaps into the sky and freezes overhead";

    public override int ActionCost => 2;

    public override TargetType TargetType => TargetType.Self;
    private Element _element;

    private int _maxCooldown = 2;
    public int Cooldown { get; private set; }
    public bool Active => Cooldown <= 0;
    public bool InAir { get; private set; }
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
        TerritroyReaded airTerritory = unit.ActualTerritory;
        var list = new List<Vector3> { airTerritory.GetCordinats() };
        for (int i = 0; i < 3; i++)
        {
            if (airTerritory.IndexBottom.Count == 0) continue;

            airTerritory = Manager.Map[airTerritory.IndexUp.First()];
            list.Add(airTerritory.GetCordinats());
        }

        yield return unit.StartCoroutine(Manager.MovementManager.MoveEnemyToTerritory((Enemy)unit, list, airTerritory));
        InAir = true;
    }

    private IEnumerator Fall(Unit unit, object target)
    {
        TerritroyReaded findedTerritory = unit.ActualTerritory;
        if(target != null)
        {
            findedTerritory = (target as Unit).ActualTerritory.GetRandomTerritoryNearBy();
        } else
        {
            for (int i = 0; i < 3; i++)
            {
                if (findedTerritory.IndexBottom.Count == 0) continue;

                findedTerritory = Manager.Map[findedTerritory.IndexBottom.First()];
            }
        }

        var list = new List<Vector3>
        {
            unit.ActualTerritory.GetCordinats(),
            //new Vector3(findedTerritory.XPosition, unit.ActualTerritory.YPosition, findedTerritory.ZPosition),
            findedTerritory.GetCordinats()
        };

        unit.Stats.SpeedIncreaser = 5; //mb to config
        yield return unit.StartCoroutine(Manager.MovementManager.MoveEnemyToTerritory((Enemy)unit, list, findedTerritory));
        unit.Stats.SpeedIncreaser = 0;

        Debug.Log(target);
        if(target != null)
            unit.Animator.RotateLookAtImmediate((target as Unit).ActualTerritory.GetCordinats());

        var adjancentAUnits = Manager.Map.GetAdjancentUnits(1, unit);
        foreach(var item in adjancentAUnits)
        {
            if (item is Enemy || item == unit)
                continue;

            int dmg = UnityEngine.Random.Range((unit as Enemy).Stats.MinDamage, (unit as Enemy).Stats.MaxDamage + 1);
            item.Health.MakeHit(dmg, _element, unit);
        }
        InAir = false;
        CooldownStart();
    }

    public void CooldownDecreaser() => Cooldown--;

    public void CooldownStart() => Cooldown = _maxCooldown; //+ 1? becouse next turn 
    
}
