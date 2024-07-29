using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AbilitiesFactory
{
    interface IAbilityCreator
    {
        Ability Create(AbilitiesCreateData data, List<Ability> abilities);
    }

    public class ShootCreator : IAbilityCreator
    {
        public Ability Create(AbilitiesCreateData data, List<Ability> abilities) => new AbilityShoot(data.Element);
    }
    public class OverwatchCreator : IAbilityCreator
    {
        public Ability Create(AbilitiesCreateData data, List<Ability> abilities) => new AbilityOverwatch();
    }
    public class HunkerDownCreator : IAbilityCreator
    {
        public Ability Create(AbilitiesCreateData data, List<Ability> abilities) => new AbilityHunkerDown(data.Creator);
    }

    public class AlbedoElementalSkillCreator : IAbilityCreator
    {
        public Ability Create(AbilitiesCreateData data, List<Ability> abilities) => new AbilityAlbedoElementalSkill(data.Element);
    }
    public class AbillityAlbedoUltimateCreator : IAbilityCreator
    {
        public Ability Create(AbilitiesCreateData data, List<Ability> abilities) => new AbillityAlbedoUltimate(abilities.First(n => n is AbilityAlbedoElementalSkill));
    }

    public class AbillityAlbedoMeleeAtackCreator : IAbilityCreator
    {
        public Ability Create(AbilitiesCreateData data, List<Ability> abilities) => new AbilityMeleeAttack(data.Element, data.Creator);
    }

}