using System;
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

    private int _index;
    public int Index
    {
        get { return _index; }
        set { _index = value; _indexText.text = value.ToString(); }
    }

    public bool _abilityEnabled;
    public bool AbilityEnabled
    {
        get { return _abilityEnabled; }
        set
        {
            _abilityEnabled = value;

            Color tmp = _image.color;
            tmp.a = _abilityEnabled ? 1.0f : 0.5f;
            _image.color = tmp;
        }
    }

    private AbilityPanel _panel;

    [SerializeField]
    private UnityEvent<Action> _event;
    public UnityEvent<Action> Event => _event;

    [SerializeField]
    private AbilityTargetType _targetType;
    public AbilityTargetType TargetType => _targetType;

    void Start()
    {
        _panel = transform.parent.GetComponent<AbilityPanel>();
    }

    public void OnPointerClick(PointerEventData data)
    {
        if (data.clickCount == 1)
        {
            _panel.SelectAbility(this);
        }
        else if (data.clickCount == 2)
        {
            _panel.ActivateAbility();
        }
    }

    public void SetupCameraForTargetEnter()
    {
        switch (_targetType)
        {
            case AbilityTargetType.Enemy:
                GameManagerMap.Instance.FixCameraOnSelectedEnemy();
                break;
            case AbilityTargetType.None:
                GameManagerMap.Instance.EnableFreeCameraMovement();
                break;
        }
    }

    public void SetupCameraForTargetExit()
    {
        GameManagerMap.Instance.EnableFreeCameraMovement();
    }
}
