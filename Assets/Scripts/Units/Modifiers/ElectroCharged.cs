using System.Collections;
using System.Collections.Generic;
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
        return "At the end of turn, deals 1 [Electro] damage to this unit and [Hydro]-afflicted allies within 1 square.";
    }

    public override string IconName()
    {
        return "Electro-Charged";
    }

    public override IEnumerator OnStartRound(Unit unit) { yield return null; }
    public override IEnumerator OnEndRound(Unit unit) { yield return null; }
}