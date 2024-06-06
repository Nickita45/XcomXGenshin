using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilitiesHelper
{
    public static List<Ability> GetAllAbilities(AbilitiesList[] abilitiesLists)
    {
        var _abilities = new List<Ability>();
        foreach (AbilitiesList list in abilitiesLists)
        {
            if (!string.IsNullOrEmpty(list.childFunction))
            {
                var parentAbility = _abilities.Find(item => item.GetType().Name == list.childFunction);
                _abilities.Add(GetAbilityFromList(list.name, parentAbility));
            }
            else
            {
                var element = Enum.Parse<Element>(list.element);
                _abilities.Add(GetAbilityFromList(list.name, element));
            }
        }
        return _abilities;
    }
    public static Ability GetAbilityFromList(string abilityName, Element element)
    {
        var cls = GetAbilityType(abilityName);
        return (Ability)CreateInstance(cls, new object[] { element });
    }

    public static Ability GetAbilityFromList(string abilityName, Ability ability)
    {
        var cls = GetAbilityType(abilityName);
        return (Ability)CreateInstance(cls, new object[] { ability });
    }

    private static Type GetAbilityType(string abilityName)
    {
        var cls = Type.GetType(abilityName);
        if (cls == null)
        {
            throw new ArgumentException($"Ability type '{abilityName}' not found.");
        }
        return cls;
    }

    private static object CreateInstance(Type cls, params object[] args)
    {
        if (args.Length == 0 || cls.GetConstructor(GetConstructorParameterTypes(args)) == null)
        {
            return Activator.CreateInstance(cls);
        }

        return cls.GetConstructor(GetConstructorParameterTypes(args)).Invoke(args);
    }

    private static Type[] GetConstructorParameterTypes(object[] args)
    {
        var types = new Type[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            types[i] = args[i].GetType();
        }
        return types;
    }
}
