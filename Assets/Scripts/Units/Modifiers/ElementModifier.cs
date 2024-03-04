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
        _turns = turnsLeft;
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

    public override ModifierStackBehavior HandleDuplicate(Modifier other)
    {
        if (other is ElementModifier em && em.Element == _element) return ModifierStackBehavior.Stack;
        else return ModifierStackBehavior.Duplicate;
    }

    public static Dictionary<(Element, Element), ElementalReaction> ELEMENTAL_REACTIONS = new()
        {
            { (Element.Pyro, Element.Hydro), ElementalReaction.VaporizeWeak },
            { (Element.Hydro, Element.Pyro), ElementalReaction.VaporizeStrong },

            { (Element.Pyro, Element.Cryo), ElementalReaction.MeltStrong },
            { (Element.Cryo, Element.Pyro), ElementalReaction.MeltWeak },

            { (Element.Hydro, Element.Cryo), ElementalReaction.Freeze },
            { (Element.Cryo, Element.Hydro), ElementalReaction.Freeze },

            { (Element.Pyro, Element.Electro), ElementalReaction.Overloaded },
            { (Element.Electro, Element.Pyro), ElementalReaction.Overloaded },

            { (Element.Hydro, Element.Electro), ElementalReaction.ElectroCharged },
            { (Element.Electro, Element.Hydro), ElementalReaction.ElectroCharged },

            { (Element.Cryo, Element.Electro), ElementalReaction.Superconduct },
            { (Element.Electro, Element.Cryo), ElementalReaction.Superconduct },

            { (Element.Anemo, Element.Pyro), ElementalReaction.SwirlPyro },
            { (Element.Pyro, Element.Anemo), ElementalReaction.SwirlPyro },

            { (Element.Anemo, Element.Cryo), ElementalReaction.SwirlCryo },
            { (Element.Cryo, Element.Anemo), ElementalReaction.SwirlCryo },

            { (Element.Anemo, Element.Hydro), ElementalReaction.SwirlHydro },
            { (Element.Hydro, Element.Anemo), ElementalReaction.SwirlHydro },

            { (Element.Anemo, Element.Electro), ElementalReaction.SwirlElectro },
            { (Element.Electro, Element.Anemo), ElementalReaction.SwirlElectro },

            { (Element.Geo, Element.Pyro), ElementalReaction.CrystallizePyro },
            { (Element.Pyro, Element.Geo), ElementalReaction.CrystallizePyro },

            { (Element.Geo, Element.Cryo), ElementalReaction.CrystallizeCryo },
            { (Element.Cryo, Element.Geo), ElementalReaction.CrystallizeCryo },

            { (Element.Geo, Element.Hydro), ElementalReaction.CrystallizeHydro },
            { (Element.Hydro, Element.Geo), ElementalReaction.CrystallizeHydro },

            { (Element.Geo, Element.Electro), ElementalReaction.CrystallizeElectro },
            { (Element.Electro, Element.Geo), ElementalReaction.CrystallizeElectro },
        };

    public override ElementalReaction? CheckReaction(Element element)
    {
        var key = (_element, element);
        if (ELEMENTAL_REACTIONS.ContainsKey(key))
        {
            return ELEMENTAL_REACTIONS[key];
        }
        return null;
    }

    public override IEnumerator OnBeginRound(Unit unit) { yield return null; }
    public override IEnumerator OnEndRound(Unit unit) { yield return null; }
    public override int OnHit(Unit unit, int hit, Element element) { return hit; }
    public override void SpawnModel(Unit unit) { }
    public override void DestroyModel(Unit unit) { }
}
