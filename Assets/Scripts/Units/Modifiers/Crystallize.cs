using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Crystallize : Modifier
{
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

    public override int OnHit(Unit unit, int hit, Element element) { return hit - 1; }

    GameObject model;

    public override void SpawnModel(Unit unit)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Modifiers/Crystallize");
        model = GameObject.Instantiate(prefab, unit.transform);
    }

    public override void DestroyModel(Unit unit)
    {
        GameObject.Destroy(model);
    }
}
