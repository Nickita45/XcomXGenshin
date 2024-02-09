using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModifierUI : MonoBehaviour
{
    [SerializeField]
    private Image _icon;


    public void Init(string title, string description, string iconName)
    {
        _icon.sprite = Manager.UIIcons.GetIconByName(iconName)?.sprite;
    }
}
