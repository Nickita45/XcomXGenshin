using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementUtils : MonoBehaviour
{
    public static Color ElementColor(Element element)
    {
        return element switch
        {
            Element.Pyro => new Color(0.93f, 0.69f, 0.54f),
            Element.Cryo => new Color(0.85f, 0.95f, 0.96f),
            Element.Hydro => new Color(0.13f, 0.84f, 0.92f),
            Element.Electro => new Color(0.85f, 0.74f, 0.98f),
            Element.Anemo => new Color(0.72f, 0.93f, 0.81f),
            Element.Geo => new Color(0.85f, 0.79f, 0.49f),
            Element.Dendro => new Color(0.69f, 0.91f, 0.18f),
            Element.Physical => new Color(0.8f, 0.8f, 0.8f),
            _ => Color.black,
        };
    }

    public static string ReactionName(ElementalReaction reaction)
    {
        return reaction switch
        {
            ElementalReaction.MeltStrong => "Melt",
            ElementalReaction.MeltWeak => "Melt",
            ElementalReaction.VaporizeStrong => "Vaporize",
            ElementalReaction.VaporizeWeak => "Vaporize",
            ElementalReaction.ElectroCharged => "Electro-Charged",
            ElementalReaction.Overloaded => "Overloaded",
            ElementalReaction.Superconduct => "Superconduct",
            ElementalReaction.Freeze => "Freeze",
            ElementalReaction.Shatter => "Shatter",
            ElementalReaction.SwirlPyro => "Swirl",
            ElementalReaction.SwirlCryo => "Swirl",
            ElementalReaction.SwirlHydro => "Swirl",
            ElementalReaction.SwirlElectro => "Swirl",
            ElementalReaction.CrystallizePyro => "Crystallize",
            ElementalReaction.CrystallizeCryo => "Crystallize",
            ElementalReaction.CrystallizeHydro => "Crystallize",
            ElementalReaction.CrystallizeElectro => "Crystallize",
            _ => "Unknown Reaction",
        };
    }

    public static Color ReactionColor(ElementalReaction reaction)
    {
        return reaction switch
        {
            ElementalReaction.MeltStrong => ElementColor(Element.Cryo),
            ElementalReaction.MeltWeak => ElementColor(Element.Cryo),
            ElementalReaction.VaporizeStrong => ElementColor(Element.Hydro),
            ElementalReaction.VaporizeWeak => ElementColor(Element.Hydro),
            ElementalReaction.ElectroCharged => ElementColor(Element.Electro),
            ElementalReaction.Overloaded => ElementColor(Element.Pyro),
            ElementalReaction.Superconduct => ElementColor(Element.Cryo),
            ElementalReaction.Freeze => ElementColor(Element.Cryo),
            ElementalReaction.Shatter => ElementColor(Element.Physical),
            ElementalReaction.SwirlPyro => ElementColor(Element.Anemo),
            ElementalReaction.SwirlCryo => ElementColor(Element.Anemo),
            ElementalReaction.SwirlHydro => ElementColor(Element.Anemo),
            ElementalReaction.SwirlElectro => ElementColor(Element.Anemo),
            ElementalReaction.CrystallizePyro => ElementColor(Element.Geo),
            ElementalReaction.CrystallizeCryo => ElementColor(Element.Geo),
            ElementalReaction.CrystallizeHydro => ElementColor(Element.Geo),
            ElementalReaction.CrystallizeElectro => ElementColor(Element.Geo),
            _ => Color.black,
        };
    }
}
