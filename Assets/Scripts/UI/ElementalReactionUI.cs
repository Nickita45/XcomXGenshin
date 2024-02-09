using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ElementalReactionUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI reactionText;
    public void Init(ElementalReaction reaction)
    {
        reactionText.text = reaction.ToString();
        reactionText.color = ElementUtils.ReactionColor(reaction);
    }
}
