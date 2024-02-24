using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Modifier
{
    public abstract string Title();
    public abstract string Description();
    public abstract string IconName();

    protected int _turnsLeft = 0;
    public int TurnsLeft => _turnsLeft;

    protected bool _infinite;
    public bool IsInfinite => _infinite;

    public Action onUpdate;

    public void TurnDecrement()
    {
        _turnsLeft--;
        onUpdate();
        // TODO: remove this if over
    }

    public bool IsModifierActive()
    {
        return _infinite || _turnsLeft > 0;
    }
}
