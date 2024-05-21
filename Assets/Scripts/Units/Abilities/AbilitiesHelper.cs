using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AbilitiesHelper
{
    public static Ability GetAbilityFromList(string abilityName, Element element){

        Type cls = Type.GetType(abilityName);
        object instance = new object();
        if (cls != null) {
            instance = Activator.CreateInstance(cls);
        }
        if(element != Element.Physical)
        {
            var constructor = cls.GetConstructor(new Type[] { typeof(Element) });
            instance = constructor.Invoke(new object[] { element });
        }

        return (Ability)instance;
    }
}