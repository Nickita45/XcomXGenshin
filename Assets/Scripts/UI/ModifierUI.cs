using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModifierUI : MonoBehaviour
{
    [SerializeField]
    private Image _icon;

    [SerializeField]
    private TextMeshProUGUI _durationText;

    private Modifier _modifier;

    public void Init(Modifier modifier)
    {
        _modifier = modifier;

        UpdateUI();
        modifier.onUpdate += UpdateUI;
    }

    public void UpdateUI()
    {
        _icon.sprite = Manager.UIIcons.GetIconByName(_modifier.IconName())?.sprite;
        if (_modifier.IsInfinite)
        {
            _durationText.gameObject.SetActive(false);
        }
        else
        {
            _durationText.gameObject.SetActive(true);
            _durationText.text = _modifier.TurnsLeft.ToString();
        }
    }
}
