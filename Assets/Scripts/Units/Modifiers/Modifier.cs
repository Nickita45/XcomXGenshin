using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Modifier
{
    public abstract string Title();
    public abstract string Description();
    public abstract string IconName();

    // Checks if the modifier can react to a certain element.
    public virtual ElementalReaction? CheckReaction(Element element) { return null; }

    // Checks what behavior to expect when we try to add another
    // modifier of the same type
    public virtual ModifierStackBehavior HandleDuplicate(Modifier other) { return ModifierStackBehavior.Stack; }

    public virtual IEnumerator OnBeginRound(Unit unit) { yield return null; }
    public virtual IEnumerator OnEndRound(Unit unit) { yield return null; }
    public virtual int OnHit(Unit unit, int hit, Element element) { return hit; }

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

    public bool IsActive()
    {
        return _infinite || _turns > 0;
    }

    // Spawn visual effects
    public virtual void SpawnModel(Unit unit) { }

    // Destroy visual effects
    public virtual void DestroyModel(Unit unit) { }
}

public enum ModifierStackBehavior
{
    Stack,
    Duplicate,
    // more might be added later
}