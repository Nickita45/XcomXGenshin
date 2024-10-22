using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ModifierList
{
    private List<Modifier> _modifiers = new();
    public List<Modifier> Modifiers => _modifiers;

    private Unit _unit;

    public ModifierList(Unit unit)
    {
        _unit = unit;
    }

    public IEnumerator OnBeginRound()
    {
        foreach (Modifier modifier in Modifiers)
        {
            yield return _unit.StartCoroutine(modifier.OnBeginRound(_unit));
        }

        _unit?.Canvas.UpdateModifiersUI(this);
    }

    public IEnumerator OnEndRound()
    {
        List<Modifier> toDelete = new();
        foreach (Modifier modifier in Modifiers)
        {
            if (modifier.TurnDecrement())
            {
                yield return _unit.StartCoroutine(modifier.OnEndRound(_unit));
            }
            else
            {
                toDelete.Add(modifier);
            }
        }
        foreach (Modifier modifier in toDelete) { RemoveModifier(modifier); }

        _unit?.Canvas.UpdateModifiersUI(this);
    }

    public int OnHit(int hit, Element element)
    {
        List<Modifier> toDelete = new();
        foreach (Modifier modifier in Modifiers)
        {
            hit = modifier.OnHit(_unit, hit, element);
            if (!modifier.IsActive()) toDelete.Add(modifier);
        }
        foreach (Modifier modifier in toDelete) { RemoveModifier(modifier); }

        _unit?.Canvas.UpdateModifiersUI(this);

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

    public List<ElementalReaction> AddElement(Element element)
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
            RemoveModifier(m);
        }

        if (reactions.Count == 0 && element != Element.Physical)
        {
            foreach (Modifier modifier in _modifiers)
            {
                if (modifier is ElementModifier m && m.Element == element)
                {
                    // If a modifier with this element exists, ignore
                    continue;
                }
            }

            // Otherwise, create a new modifier
            AddModifier(new ElementModifier(element));
        }
        return reactions;
    }

    public void AddModifier(Modifier modifier)
    {
        // Check if any existing modifiers of the same type stack with this one
        Modifier m = _modifiers.Find(m => m.GetType() == modifier.GetType() &&
            modifier.HandleDuplicate(m) == ModifierStackBehavior.Stack);

        // Extend the existing modifier instead of creating a new one
        if (m != null)
        {
            m.ExtendDuration(modifier.Turns);
        }
        // Or create a new modifier along with a respective 3d effect
        else
        {
            _modifiers.Add(modifier);
            modifier.SpawnModel(_unit);
        }

        _unit?.Canvas.UpdateModifiersUI(this);
    }

    public void RemoveModifier(Modifier modifier)
    {
        _modifiers.Remove(modifier);
        modifier.DestroyModel(_unit);

        _unit?.Canvas.UpdateModifiersUI(this);
    }
}