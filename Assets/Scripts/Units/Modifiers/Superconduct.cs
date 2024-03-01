using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Superconduct : Modifier
{
    public Superconduct()
    {
        _turns = 3;
    }

    public override string Title()
    {
        return "Superconduct";
    }

    public override string Description()
    {
        return "The next [Physical] attack would deal 1.5x damage to this unit.";
    }

    public override string IconName()
    {
        return "Superconduct";
    }

    public override IEnumerator OnBeginRound(Unit unit) { yield return null; }
    public override IEnumerator OnEndRound(Unit unit) { yield return null; }
    public override int OnHit(Unit unit, int hit, Element element)
    {
        if (element == Element.Physical)
        {
            return (int)(hit * 1.5);
        }
        else { return hit; }
    }
}
