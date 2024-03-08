using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ElementalReactionUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _reactionText;
    public TextMeshProUGUI Text => _reactionText;

    public void Init(ElementalReaction reaction)
    {
        _reactionText.text = ElementUtils.ReactionName(reaction);
        _reactionText.color = ElementUtils.ReactionColor(reaction);
    }
}
