using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElectroCharged : Modifier
{
    public ElectroCharged()
    {
        _turns = 3;
    }

    public override string Title()
    {
        return "Electro-Charged";
    }

    public override string Description()
    {
        return "At the end of round, deals 1 [Electro] damage to this unit and [Hydro]-afflicted allies within 1 square.";
    }

    public override string IconName()
    {
        return "Electro-Charged";
    }

    public override ElementalReaction? CheckReaction(Element element)
    { return null; }
    public override IEnumerator OnBeginRound(Unit unit) { yield return null; }
    public override IEnumerator OnEndRound(Unit unit)
    {
        // Iterate over all allies within 1 square 
        foreach (Unit ally in unit.GetAdjancentAllies(1))
        {
            // Should have either hydro or electro-charged
            if (ally.Modifiers.GetElements().Contains(Element.Hydro) ||
            ally.Modifiers.Modifiers.Any(m => m is ElectroCharged))
            {
                ally.MakeHit(1, Element.Electro, this);
            }
        }
        yield return null;
    }
    public override int OnHit(Unit unit, int hit, Element element) { return hit; }
}
