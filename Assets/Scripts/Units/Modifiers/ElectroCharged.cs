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
        return "At the end of round, deals 1 [Electro] damage [Hydro]-afflicted allies within 1 square.";
    }

    public override string IconName()
    {
        return "Electro-Charged";
    }

    public override IEnumerator OnEndRound(Unit unit)
    {
        // Iterate over all allies within 1 square 
        foreach (Unit ally in Manager.Map.GetAdjancentAllies(1, unit))
        {
            // Should have either hydro or electro-charged
            if (ally != unit && (
                ally.Modifiers.GetElements().Contains(Element.Hydro) ||
                ally.Modifiers.Modifiers.Any(m => m is ElectroCharged)
            ))
            {
                ally.Health.MakeHit(1, Element.Electro, this);
            }
        }
        yield return null;
    }

    GameObject model;

    public override void SpawnModel(Unit unit)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Modifiers/Electro-Charged");
        model = GameObject.Instantiate(prefab, unit.transform);
    }

    public override void DestroyModel(Unit unit)
    {
        GameObject.Destroy(model);
    }
}
