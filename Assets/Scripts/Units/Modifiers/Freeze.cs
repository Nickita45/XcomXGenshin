using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Freeze : Modifier
{
    public Freeze()
    {
        _turns = 1;
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
        return "Freeze";
    }

    public override IEnumerator OnBeginRound(Unit unit)
    {
        unit.ActionsLeft = 0;
        Debug.Log(unit);
        yield return null;
    }

    public override IEnumerator OnEndRound(Unit unit) { yield return null; }
    public override int OnHit(Unit unit, int hit, Element element) { return hit; }
}
