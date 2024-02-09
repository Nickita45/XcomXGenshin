using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Modifier
{
    public abstract string Title();
    public abstract string Description();
    public abstract string IconName();
    protected int _turnsLeft = 0;
    protected bool _infinite;

    public void TurnDecrement()
    {
        _turnsLeft--;
    }

    public bool IsModifierActive()
    {
        return _infinite || _turnsLeft > 0;
    }
}
