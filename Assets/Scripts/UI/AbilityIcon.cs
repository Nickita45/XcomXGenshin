using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityIcon : MonoBehaviour, IPointerClickHandler
{
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

    private AbilityPanel _panel;

    private Ability _ability;
    public Ability Ability => _ability;

    private object _target;
    public object Target => _target;
    
    void Start()
    {
        _panel = transform.parent.GetComponent<AbilityPanel>();
    }

    public void SetAbility(int index, Ability ability)
    {
        Index = index + 1;
        _image.sprite = Manager.UIIcons.GetIconByName(ability.Icon)?.sprite;
        _ability = ability;
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
