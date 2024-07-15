using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AbilitiesFactory
{
    public class AbilitiesFactoryCreator
    {
        private static Dictionary<string, IAbilityCreator> _generator = new Dictionary<string, IAbilityCreator>()
        {
            {nameof(AbilityShoot), new ShootCreator() },
            {nameof(AbilityOverwatch), new OverwatchCreator() },
            {nameof(AbilityHunkerDown), new HunkerDownCreator() },
            {nameof(AbilityAlbedoElementalSkill), new AlbedoElementalSkillCreator() },
            {nameof(AbillityAlbedoUltimate), new AbillityAlbedoUltimateCreator() },
            {nameof(AbilityMeleeAttack), new AbillityAlbedoMeleeAtackCreator() },
        };

        public static List<Ability> GetAllAbilities(AbilitiesList[] abilitiesLists, Unit creator)
        {
            var _abilities = new List<Ability>();
            foreach (AbilitiesList list in abilitiesLists)
            {
                Element element = Element.Physical;

                if (!string.IsNullOrEmpty(list.element))
                    element = Enum.Parse<Element>(list.element);

                Debug.Log(list.name + " " + list.element);
                
                Ability ability = _generator[list.name].Create(new AbilitiesCreateData(
                        element: element,
                        creator: creator),
                    _abilities);

                _abilities.Add(ability);
            }
            return _abilities;
        }
    }
}