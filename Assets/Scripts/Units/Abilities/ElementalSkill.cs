using System.Collections;
using UnityEngine;

// A placeholder for the elemental skills that would be added in future
public class AbilityElementalSkill : Ability, IAbilitySummon, IAbilityArea
{
    private Element _element;
    private AlbedoFlowerEntity _entity;
    public AbilityElementalSkill(Element element) //mb not nessary?
    {
        _element = element;
    }
    public override string AbilityName => string.Format("Elemental Skill ({0})", _element);
    public override string Description => "Elemental Skill Description";
    public override string Icon => _element.ToString();
    public override int ActionCost => 1;
    public override TargetType TargetType => TargetType.Summon;
    public override int MaxCooldown => 2;
    public int GetMaxCooldown() => 2;
    public int RangeSummon() => 2;
    public string PathSummonedObject() => "Prefabs/Entity/AlbedoFlower";
    public int RangeArea() => 5;
    public void SummonArea() =>
        Manager.AbilityAreaController.AddOrEditAreas((Manager.MovementManager.GetSelectedTerritory.GetCordinats(), RangeArea()));

    public AlbedoFlowerEntity FlowerEntity => _entity;
    public override IEnumerator Activate(Unit unit, object target)
    {
        Debug.Log("Elemental Skill");
        if(_entity != null && !_entity.IsKilled)
            _entity.Kill();
        
        (GameObject obj, TerritroyReaded ter) = Manager.SummonUnitManager.SummonEntity(PathSummonedObject());
        _entity = obj.GetComponent<AlbedoFlowerEntity>();
        _entity.OnCreate(unit, ter);
        yield return new WaitForSeconds(1f);
        ActualCooldown = GetMaxCooldown();
        yield return null;
    }

    
}
