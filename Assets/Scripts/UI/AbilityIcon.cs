using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityIcon : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private string _title;
    public string Title => _title;

    [SerializeField]
    private string _description;
    public string Description => _description;

    [SerializeField]
    private Image _image;
    public Image Image => _image;

    private AbilityPanel _panel;

    void Start()
    {
        _panel = transform.parent.GetComponent<AbilityPanel>();
    }

    public void OnPointerClick(PointerEventData data)
    {
        _panel.SelectAbility(this);
    }
}
