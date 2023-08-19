using TMPro;
using UnityEngine;
using UnityEngine.Events;
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

    [SerializeField]
    private TextMeshProUGUI _indexText;

    private int index;
    public int Index
    {
        get { return index; }
        set { index = value; _indexText.text = value.ToString(); }
    }

    private AbilityPanel _panel;

    [SerializeField]
    private UnityEvent _event;
    public UnityEvent Event => _event;

    [SerializeField]
    private AbilityTargetType _targetType;
    public AbilityTargetType TargetType => _targetType;

    void Start()
    {
        _panel = transform.parent.GetComponent<AbilityPanel>();
    }

    public void OnPointerClick(PointerEventData data)
    {
        _panel.SelectAbility(this);
    }
}
