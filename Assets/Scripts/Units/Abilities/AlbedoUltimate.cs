using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbillityAlbedoUltimate : Ability
{
    private Element _element = Element.Geo;
    private int _abilityRange = 5;
    private int _dealDmg = 2;
    private int _dealDmgFlower = 1;
    private AbilityElementalSkill _albedoFlowerSkill;

    public AbillityAlbedoUltimate(AbilityElementalSkill albedoFlowerSkill)
    {
        _albedoFlowerSkill = albedoFlowerSkill;
    }

    public override string AbilityName => "Albedo Ultimate";

    public override string Description => "Albedo ultimate";
    public override string Icon => _element.ToString();

    public override int MaxCooldown => 0;

    public override int ActionCost => 2;

    public override TargetType TargetType => TargetType.Self;

    public override IEnumerator Activate(Unit unit, object target)
    {
        Debug.Log("Albedo ultimate");
        HubData.Instance.ParticleSystemFactory.CreateAlbedoUltimate(unit.ActualTerritory.GetCordinats());
        yield return new WaitForSeconds(0.5f);
        foreach (var detectedUnit in Manager.Map.GetAdjancentUnits(_abilityRange, unit.ActualTerritory)) {
            if(detectedUnit is Enemy) Debug.Log(detectedUnit.Stats.Name());
            if (detectedUnit is Enemy enemy) enemy.Health.MakeHit(_dealDmg, _element, unit);
        }
        
        if (_albedoFlowerSkill.FlowerEntity != null && !_albedoFlowerSkill.FlowerEntity.IsKilled)
        {
            _albedoFlowerSkill.FlowerEntity.MakeDamageForAdjancentUnits(_dealDmgFlower);
            _albedoFlowerSkill.FlowerEntity.Kill();
        }

        yield return new WaitForSeconds(2.0f);
        yield return null;
    }
}
