using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystallize : Modifier
{
    private Element _element;
    public Crystallize()
    {
        _turns = 2;
    }

    public override string Title()
    {
        return "Crystallize";
    }

    public override string Description()
    {
        return "Decrease incoming damage by 1.";
    }

    public override string IconName()
    {
        return "Crystallize";
    }

    public override ElementalReaction? CheckReaction(Element element)
    { return null; }
    public override IEnumerator OnBeginRound(Unit unit) { yield return null; }
    public override IEnumerator OnEndRound(Unit unit) { yield return null; }
    public override int OnHit(Unit unit, int hit, Element element) { return hit - 1; }
}
