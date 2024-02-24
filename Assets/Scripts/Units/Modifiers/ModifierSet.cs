using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ModifierSet
{
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

    // Get a HashSet of currently applied elements
    public HashSet<Element> GetElements()
    {
        HashSet<Element> elements = new();

        foreach (Modifier modifier in _modifiers)
        {
            if (modifier is ElementModifier m)
            {
                elements.Add(m.Element);
            }
        }
        return elements;
    }

    // Add an element modifier.
    // 
    // This is a private function. For public use, there is an ApplyElement function.
    private void AddElement(Element element)
    {
        foreach (Modifier modifier in _modifiers)
        {
            if (modifier is ElementModifier m && m.Element == element)
            {
                // If a modifier with this elements exists, ignore
                return;
            }
        }

        // Otherwise, create a new modifier
        _modifiers.Add(new ElementModifier(element));
    }

    // Remove an element modifier.
    private void RemoveElement(Element element)
    {
        foreach (Modifier modifier in _modifiers)
        {
            if (modifier is ElementModifier m && m.Element == element)
            {
                _modifiers.Remove(modifier);
                return;
            }
        }
    }

    public List<ElementalReaction> ApplyElement(Element element)
    {
        List<ElementalReaction> reactions = new();

        List<Element> toRemove = new();
        foreach (Element other in GetElements())
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
            RemoveElement(other);
        }

        if (reactions.Count == 0 && element != Element.Physical) AddElement(element);
        return reactions;
    }

    public void ApplyModifier(Modifier modifier)
    {
        _modifiers.Add(modifier);
    }
}
