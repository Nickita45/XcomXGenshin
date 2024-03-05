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
        return "This unit is frozen and can't act. \n\n- Apply [Physical] or [Geo] to break the ice and trigger [Shatter].\n\n- Apply [Pyro] to trigger [Melt].";
    }

    public override string IconName()
    {
        return "Freeze";
    }

    public override ModifierStackBehavior HandleDuplicate(Modifier other)
    {
        return ModifierStackBehavior.Stack;
    }

    public override ElementalReaction? CheckReaction(Element element)
    {
        if (element == Element.Geo || element == Element.Physical)
        {
            return ElementalReaction.Shatter;
        }
        else if (element == Element.Pyro)
        {
            return ElementalReaction.MeltStrong;
        }
        return null;
    }

    public override IEnumerator OnBeginRound(Unit unit)
    {
        unit.ActionsLeft = 0;
        yield return null;
    }

    public override IEnumerator OnEndRound(Unit unit) { yield return null; }
    public override int OnHit(Unit unit, int hit, Element element) { return hit; }

    GameObject model;

    public override void SpawnModel(Unit unit)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Modifiers/Freeze");
        model = GameObject.Instantiate(prefab, unit.transform);
    }
    public override void DestroyModel(Unit unit)
    {
        GameObject.Destroy(model);
    }
}
