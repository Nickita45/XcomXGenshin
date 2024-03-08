using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitInfoDialogModifier : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _modifierName, _modifierDescription;

    [SerializeField]
    private Image _icon;

    public void Init(Modifier modifier)
    {
        _modifierName.text = modifier.Title();
        _modifierDescription.text = FormatDescription(modifier.Description());
        _icon.sprite = Manager.UIIcons.GetIconByName(modifier.IconName()).sprite;
    }

    // Format description with colored text (e.g [Electro] or [Crystallize])
    // Works with elements and elemental reactions
    public string FormatDescription(string input)
    {
        // Regular expression pattern to match substrings of format [substringA]
        string pattern = @"\[(.*?)\]";

        // Use Regex.Replace to find and replace substrings matching the pattern
        string result = Regex.Replace(input, pattern, match =>
        {
            string substring = match.Groups[1].Value;
            foreach (Element elem in Element.GetValues(typeof(Element)))
            {
                if (elem.ToString() == substring)
                {
                    return $"<color=#{ColorUtility.ToHtmlStringRGB(ElementUtils.ElementColor(elem))}>{substring}</color>";
                }
            }

            foreach (ElementalReaction reaction in ElementalReaction.GetValues(typeof(ElementalReaction)))
            {
                if (ElementUtils.ReactionName(reaction) == substring)
                {
                    return $"<color=#{ColorUtility.ToHtmlStringRGB(ElementUtils.ReactionColor(reaction))}>{substring}</color>";
                }
            }

            return $"<color=\"black\">{substring}</color>";
        });

        return result;
    }
}
