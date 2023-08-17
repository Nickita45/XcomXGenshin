using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AbilityDialog : MonoBehaviour
{

    [SerializeField]
    private TextMeshProUGUI _titleText;

    [SerializeField]
    private TextMeshProUGUI _descriptionText;

    private AbilityIcon _selected;

    void Start()
    {

    }

    void Update()
    {

    }

    public void View(AbilityIcon icon)
    {
        _titleText.text = icon.Title;
        _descriptionText.text = icon.Description;
    }
}
