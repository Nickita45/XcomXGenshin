using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityIcon : MonoBehaviour, IPointerClickHandler
{
    private const float SIZE_OF_ICON = -70.0f; 

    [SerializeField]
    private Image _image;

    [SerializeField]
    private TextMeshProUGUI _indexText, _cooldownText;

    [SerializeField]
    private GameObject _cooldownPanel;

    private int _index;
    private AbilityPanel _panel;
    private Ability _ability;
    public Ability Ability => _ability;

    private object _target;
    public bool _abilityEnabled; //why public?
    public object Target => _target;
    public Image Image => _image;
    public int Index
    {
        get { return _index; }
        set { _index = value; _indexText.text = value.ToString(); }
    }

    public bool AnyAvailableTargets
    {
        get { return _abilityEnabled; }
        set
        {
            _abilityEnabled = value;

            Color tmp = _image.color;
            tmp.a = _abilityEnabled ? 1.0f : 0.3f;
            _image.color = tmp;
        }
    }
    
    void Start()
    {
        _panel = transform.parent.GetComponent<AbilityPanel>();
    }

    public void SetAbility(int index, Ability ability)
    {
        Index = index + 1;
        _image.sprite = Manager.UIIcons.GetIconByName(ability.Icon)?.sprite;
        _ability = ability;
        _cooldownText.text = string.Empty;
        _cooldownPanel.gameObject.SetActive(false);

        if (ability.ActualCooldown > 0 || ability is AbilityUltimate)
        {
            if(ability is AbilityUltimate ultimate)
            {
                _cooldownPanel.gameObject.SetActive(true);
                var rectTransform = _cooldownPanel.GetComponent<RectTransform>();
                rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, ability.ActualCooldown * SIZE_OF_ICON / 100);
                _cooldownText.text = ability.ActualCooldown.ToString() + "%";
            }
            else
            {
                _cooldownText.text = ability.ActualCooldown.ToString();
            }
            

        }
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

    // Enters the target mode to allow the player to select the target for an ability.
    public void EnterTargetMode()
    {
        switch (_ability.TargetType)
        {
            case TargetType.Enemy:
                Manager.TargetSelectManager.TargetEnemy(enemy => _target = enemy);
                break;
            case TargetType.Self:
                Manager.TargetSelectManager.TargetSelf(_ => { });
                if (_ability is IAbilityArea area) area.SummonArea();
                break;
            case TargetType.Summon:
                Manager.TargetSelectManager.TargetSummon();
                break;
               
        }
    }

    // Exits the target mode, returning to free movement.
    public void ExitTargetMode()
    {
        Manager.CameraManager.EnableFreeCameraMovement();
        Manager.OutlineManager.ClearTargets();
        Manager.StatusMain.SetStatusSelectAction();
    }
}
