using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystallize : Modifier
{
    private Element _element;
    public Crystallize()
    {
        _turnsLeft = 2;
    }

    public override string Title()
    {
        return "Crystallize";
    }

    public override string Description()
    {
        return "Decreases incoming damage by 1.";
    }

    public override string IconName()
    {
        return "Geo";
    }
}
