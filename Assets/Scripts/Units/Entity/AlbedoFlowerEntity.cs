using System;
using System.Collections;
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

    [SerializeField]
    private GameObject _mainPartFlower;
    [SerializeField]
    private Vector3 _positionAfterAnimation;
    [SerializeField]
    private Vector3 _positionBeforeAnimation;
    [SerializeField]
    private float _speed = 0.5f;

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

    private void StartingAnimation(TerritroyReaded territory, Character character)
    {
        if (Manager.Map[ActualTerritory.IndexUp.First()].TerritoryInfo == TerritoryType.Character ||
            Manager.Map[ActualTerritory.IndexUp.First()].TerritoryInfo == TerritoryType.Enemy)
            StartCoroutine(Animation(_positionAfterAnimation));
        else
            StartCoroutine(Animation(_positionBeforeAnimation));
    }

    private IEnumerator Animation(Vector3 target)
    {
        var minusKoef = 1;
        //if(target.y < _mainPartFlower.transform.localPosition.y) minusKoef = -1;

        while (Vector3.Distance(_mainPartFlower.transform.localPosition, target) > 0.05f) {
            Debug.Log(Vector3.Lerp(_mainPartFlower.transform.localPosition, target, _speed * Time.deltaTime) * minusKoef);
            _mainPartFlower.transform.localPosition =
             Vector3.Lerp(_mainPartFlower.transform.localPosition, target, _speed * Time.deltaTime) * minusKoef;
            // Vector3.Lerp(Vector3.Min(_mainPartFlower.transform.localPosition, target), Vector3.Max(_mainPartFlower.transform.localPosition, target), _speed * Time.deltaTime) * minusKoef;
            yield return new WaitForFixedUpdate();
        }
        _mainPartFlower.transform.localPosition = target;
    }

    public override void OnCreate(Unit creater, TerritroyReaded newPosition)
    {
        base.OnCreate(creater, newPosition);
        ActualTerritory = newPosition;
        _lifeTime = _lifeMaxCooldown;
        Manager.MovementManager.OnEndMove += StartingAnimation;
    }

    public override void Kill()
    {
        base.Kill();
        Manager.MovementManager.OnEndMove -= StartingAnimation;
        Unit unit = null;
        TerritroyReaded territory = Manager.Map[ActualTerritory.IndexUp.First()];
        if (territory.TerritoryInfo == TerritoryType.Character)
            unit = Manager.Map.Characters.GetList.First(n => n.ActualTerritory == territory);
        else if (territory.TerritoryInfo == TerritoryType.Enemy)
            unit = Manager.Map.Enemies.GetList.First(n => n.ActualTerritory == territory);
        else
        {
            unit = Manager.Map.Entities.GetList.FirstOrDefault(n => n.ActualTerritory == territory);
            if(unit != null)
                (unit as Entity).Kill();
            return;
        }


        var list = new List<Vector3>
        {
            unit.ActualTerritory.GetCordinats(),
            ActualTerritory.GetCordinats()
        };

        unit.StartCoroutine(Manager.MovementManager.MoveUnitToTerritory(unit, list, ActualTerritory));
    }
}
