using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: make virtual instead of abstract?
public abstract class Modifier
{
    public abstract string Title();
    public abstract string Description();
    public abstract string IconName();

    // Checks if the modifier can react to a certain element.
    public abstract ElementalReaction? CheckReaction(Element element);

    // Checks what behavior to expect when we try to add another
    // modifier of the same type
    public abstract ModifierStackBehavior HandleDuplicate(Modifier other);

    public abstract IEnumerator OnBeginRound(Unit unit);
    public abstract IEnumerator OnEndRound(Unit unit);
    public abstract int OnHit(Unit unit, int hit, Element element);

    protected int _turns = 0;
    public int Turns => _turns;

    protected bool _infinite = false;
    public bool IsInfinite => _infinite;

    public Action onUpdate;

    // Increases the number of turns left by n
    public void ExtendDuration(int n)
    {
        onUpdate();

        _turns += n;
    }

    // Decreases the number of turns left by 1.
    //
    // Returns true if the modifier is still active.
    public bool TurnDecrement()
    {
        onUpdate();

        if (_infinite) return true;

        _turns--;
        return _turns > 0;
    }

    // Spawn visual effects
    public abstract void SpawnModel(Unit unit);

    // Destroy visual effects
    public abstract void DestroyModel(Unit unit);
}

public enum ModifierStackBehavior
{
    Stack,
    Duplicate,
    // more might be added later
}