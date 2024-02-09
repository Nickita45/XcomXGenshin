using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ModifierSet
{
    private HashSet<Element> _appliedElements = new();
    public HashSet<Element> AppliedElements => _appliedElements;

    private HashSet<Modifier> _modifiers = new();
    public HashSet<Modifier> Modifiers => _modifiers;

    public static Dictionary<(Element, Element), ElementalReaction> ELEMENTAL_REACTIONS = new()
        {
            { (Element.Pyro, Element.Hydro), ElementalReaction.VaporizeWeak },
            { (Element.Hydro, Element.Pyro), ElementalReaction.VaporizeStrong },

            { (Element.Pyro, Element.Cryo), ElementalReaction.MeltStrong },
            { (Element.Cryo, Element.Pyro), ElementalReaction.MeltWeak },

            { (Element.Hydro, Element.Cryo), ElementalReaction.Freeze },
            { (Element.Cryo, Element.Hydro), ElementalReaction.Freeze },

            { (Element.Pyro, Element.Electro), ElementalReaction.Overloaded },
            { (Element.Electro, Element.Pyro), ElementalReaction.Overloaded },

            { (Element.Hydro, Element.Electro), ElementalReaction.ElectroCharged },
            { (Element.Electro, Element.Hydro), ElementalReaction.ElectroCharged },

            { (Element.Cryo, Element.Electro), ElementalReaction.Superconduct },
            { (Element.Electro, Element.Cryo), ElementalReaction.Superconduct },

            { (Element.Anemo, Element.Pyro), ElementalReaction.Swirl },
            { (Element.Anemo, Element.Hydro), ElementalReaction.Swirl },
            { (Element.Anemo, Element.Cryo), ElementalReaction.Swirl },
            { (Element.Anemo, Element.Electro), ElementalReaction.Swirl },
            { (Element.Pyro, Element.Anemo), ElementalReaction.Swirl },
            { (Element.Hydro, Element.Anemo), ElementalReaction.Swirl },
            { (Element.Cryo, Element.Anemo), ElementalReaction.Swirl },
            { (Element.Electro, Element.Anemo), ElementalReaction.Swirl },

            { (Element.Geo, Element.Pyro), ElementalReaction.Crystallize },
            { (Element.Geo, Element.Hydro), ElementalReaction.Crystallize },
            { (Element.Geo, Element.Cryo), ElementalReaction.Crystallize },
            { (Element.Geo, Element.Electro), ElementalReaction.Crystallize },
            { (Element.Pyro, Element.Geo), ElementalReaction.Crystallize },
            { (Element.Hydro, Element.Geo), ElementalReaction.Crystallize },
            { (Element.Cryo, Element.Geo), ElementalReaction.Crystallize },
            { (Element.Electro, Element.Geo), ElementalReaction.Crystallize },
        };

    public List<ElementalReaction> ApplyElement(Element element)
    {
        List<ElementalReaction> reactions = new();

        List<Element> toRemove = new();
        foreach (Element other in _appliedElements)
        {
            (Element, Element) key = (element, other);
            if (ELEMENTAL_REACTIONS.ContainsKey(key))
            {
                reactions.Add(ELEMENTAL_REACTIONS[key]);
                toRemove.Add(other);
            }
        }

        foreach (Element other in toRemove)
        {
            _appliedElements.Remove(other);
        }

        if (reactions.Count == 0) _appliedElements.Add(element);
        return reactions;
    }

    public void ApplyModifier(Modifier modifier)
    {
        _modifiers.Add(modifier);
    }
}
