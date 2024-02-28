using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Freeze : Modifier
{
    public Freeze()
    {
        _turnsLeft = 1;
    }

    public override string Title()
    {
        return "Freeze";
    }

    public override string Description()
    {
        return "This unit is frozen and can't act.";
    }

    public override string IconName()
    {
        return "Cryo";
    }
}
