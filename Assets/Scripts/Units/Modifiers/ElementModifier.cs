using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementModifier : Modifier
{
    private Element _element;
    public Element Element => _element;

    public ElementModifier(Element element)
    {
        _element = element;
        _infinite = true;
    }

    public ElementModifier(Element element, int turnsLeft)
    {
        _element = element;
        _turnsLeft = turnsLeft;
        _infinite = false;
    }

    public override string Title()
    {
        return _element.ToString();
    }

    public override string Description()
    {
        string description = string.Format("The [{0}] element was applied to this unit.", _element.ToString());
        switch (_element)
        {
            case Element.Pyro:
                description += "\n\n- Apply [Hydro] to trigger [Vaporize].";
                description += "\n\n- Apply [Cryo] to trigger [Melt].";
                description += "\n\n- Apply [Electro] to trigger [Overloaded].";
                break;
            case Element.Hydro:
                description += "\n\n- Apply [Pyro] to trigger [Vaporize].";
                description += "\n\n- Apply [Electro] to trigger [Electro-Charged].";
                description += "\n\n- Apply [Cryo] to trigger [Frozen].";
                description += "\n\n- Apply [Anemo] to trigger [Swirl].";
                description += "\n\n- Apply [Geo] to trigger [Crystallize].";
                break;
            case Element.Electro:
                description += "\n\n- Apply [Pyro] to trigger [Overloaded].";
                description += "\n\n- Apply [Hydro] to trigger [Electro-Charged].";
                description += "\n\n- Apply [Cryo] to trigger [Superconduct].";
                description += "\n\n- Apply [Anemo] to trigger [Swirl].";
                description += "\n\n- Apply [Geo] to trigger [Crystallize].";
                break;
            case Element.Cryo:
                description += "\n\n- Apply [Pyro] to trigger [Melt].";
                description += "\n\n- Apply [Hydro] to trigger [Frozen].";
                description += "\n\n- Apply [Electro] to trigger [Superconduct].";
                description += "\n\n- Apply [Anemo] to trigger [Swirl].";
                description += "\n\n- Apply [Geo] to trigger [Crystallize].";
                break;
            case Element.Dendro:
                break;
            case Element.Anemo:
                description += "\n\n- Apply [Pyro], [Hydro], [Cryo] or [Electro] to trigger [Swirl].";
                break;
            case Element.Geo:
                description += "\n\n- Apply [Pyro], [Hydro], [Cryo] or [Electro] to trigger [Crystallize].";
                break;
        }

        return description;
    }

    public override string IconName()
    {
        return _element.ToString();
    }
}
