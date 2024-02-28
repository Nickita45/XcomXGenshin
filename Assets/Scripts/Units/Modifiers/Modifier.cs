using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Modifier
{
    public abstract string Title();
    public abstract string Description();
    public abstract string IconName();

    public abstract IEnumerator OnStartRound(Unit unit);
    public abstract IEnumerator OnEndRound(Unit unit);

    protected int _turns = 0;
    public int Turns => _turns;

    protected bool _infinite = false;
    public bool IsInfinite => _infinite;

    public Action onUpdate;

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
}
