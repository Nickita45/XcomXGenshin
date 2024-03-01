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

    // TODO: implement without the unit parameter
    public IEnumerator OnBeginRound(Unit unit)
    {
        foreach (Modifier modifier in Modifiers)
        {
            yield return unit.StartCoroutine(modifier.OnBeginRound(unit));
        }
    }

    // TODO: implement without the unit parameter
    public IEnumerator OnEndRound(Unit unit)
    {
        List<Modifier> toDelete = new();
        foreach (Modifier modifier in Modifiers)
        {
            if (modifier.TurnDecrement())
            {
                yield return unit.StartCoroutine(modifier.OnEndRound(unit));
            }
            else
            {
                toDelete.Add(modifier);
            }
        }
        foreach (Modifier modifier in toDelete) { Modifiers.Remove(modifier); }
    }

    public int OnHit(Unit unit, int hit, Element element)
    {
        foreach (Modifier modifier in Modifiers)
        {
            hit = modifier.OnHit(unit, hit, element);
        }
        return hit;
    }

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

        List<Modifier> toRemove = new();
        foreach (Modifier modifier in _modifiers)
        {
            ElementalReaction? reaction = modifier.CheckReaction(element);
            if (reaction.HasValue)
            {
                reactions.Add(reaction.Value);
                toRemove.Add(modifier);
            }
        }

        foreach (Modifier m in toRemove)
        {
            Modifiers.Remove(m);
        }

        if (reactions.Count == 0 && element != Element.Physical) AddElement(element);
        return reactions;
    }

    public void ApplyModifier(Modifier modifier)
    {
        _modifiers.Add(modifier);
    }
}
