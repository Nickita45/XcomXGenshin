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

    public override int OnHit(Unit unit, int hit, Element element)
    {
        if (element == Element.Physical)
        {
            _turns = 0;
            return (int)(hit * 1.5);
        }

        return hit;
    }

    GameObject model;

    public override void SpawnModel(Unit unit)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Modifiers/Superconduct");
        model = GameObject.Instantiate(prefab, unit.transform);
    }

    public override void DestroyModel(Unit unit)
    {
        GameObject.Destroy(model);
    }
}
