using ParticleSystemFactory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class AbillityAlbedoUltimate : AbilityUltimate, IAbilityArea
{
    private int _abilityRange = 5;
    private int _dealDmg = 2;
    private int _dealDmgFlower = 1;
    private AbilityAlbedoElementalSkill _albedoFlowerSkill;

    public override string AbilityName => "Albedo Ultimate";
    public override string Description => "Albedo ultimate";
    public override string Icon => ElementAbility().ToString();

    public override int MaxCooldown => 0;

    public override int ActionCost => 2;

    public override TargetType TargetType => TargetType.Self;
    public int RangeArea() => 5;
    public override int MaxEnergy() => 60;

    public override Element ElementAbility() => Element.Geo;

    public AbillityAlbedoUltimate(Ability albedoFlowerSkill)
    {
        _albedoFlowerSkill = albedoFlowerSkill as AbilityAlbedoElementalSkill;
    }

    public void SummonArea()
    {
        if (_albedoFlowerSkill.FlowerEntity != null && !_albedoFlowerSkill.FlowerEntity.IsKilled)
        {
            Manager.AbilityAreaController.AddOrEditAreas(
                (Manager.TurnManager.SelectedCharacter.ActualTerritory.GetCordinats(), RangeArea()),
                (_albedoFlowerSkill.FlowerEntity.ActualTerritory.GetCordinats(), _albedoFlowerSkill.RangeArea()));
        } else
            Manager.AbilityAreaController.AddOrEditAreas((Manager.TurnManager.SelectedCharacter.ActualTerritory.GetCordinats(), RangeArea()));
    }


    public override IEnumerator Activate(Unit unit, object target)
    {
        Debug.Log("Albedo ultimate");
        ParticleSystemFactoryCreator.CreateParticle(ParticleType.AlbedoUltimate, new ParticleData
        (
            position: unit.ActualTerritory.GetCordinats()
        ));
        yield return new WaitForSeconds(0.5f);
        foreach (var detectedUnit in Manager.Map.GetAdjancentUnits(_abilityRange, unit.ActualTerritory)) {
            if(detectedUnit is Enemy) Debug.Log(detectedUnit.Stats.Name());
            if (detectedUnit is Enemy enemy) enemy.Health.MakeHit(_dealDmg, ElementAbility(), unit);
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
