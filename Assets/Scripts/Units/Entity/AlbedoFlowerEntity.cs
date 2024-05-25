using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class AlbedoFlowerEntity : Entity
{
    private int _abilityRange = 5;
    private int _dealDmg = 1;
    private int _basicHealth = 1;
    private int _lifeMaxCooldown = 4;
    private Element _element = Element.Geo;

    public override int ActionsLeft { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override void Resurrect() => _health = new UnitHealth(this, _basicHealth, null, null);

    public override void Activate()
    {
        foreach (var unit in Manager.Map.GetAdjancentUnits(_abilityRange, ActualTerritory)) {
            if(unit is Enemy enemy)
            {
                enemy.Health.MakeHit(_dealDmg, _element, this);
            }
        }
    }

    public override void OnCreate(Unit creater, TerritroyReaded newPosition)
    {
        base.OnCreate(creater, newPosition);
        ActualTerritory = newPosition;
        _lifeTime = _lifeMaxCooldown;
    }
}
