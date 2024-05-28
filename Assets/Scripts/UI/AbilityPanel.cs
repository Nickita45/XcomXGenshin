using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Manages the UI panel related to the character abilities.
public class AbilityPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject _iconPrefab;

    [SerializeField]
    private GameObject _abilityDialog;

    [SerializeField]
    private TextMeshProUGUI _titleText;

    [SerializeField]
    private TextMeshProUGUI _descriptionText;

    [SerializeField]
    private Button _confirm;

    private List<AbilityIcon> _icons = new();
    private AbilityIcon _selected;
    public AbilityIcon Selected => _selected;

    void Start()
    {
        Manager.Instance.OnClearMap += () => UnselectAbility(false);
        _confirm.onClick.AddListener(ActivateAbility);

        for (int i = 0; i < transform.childCount; i++)
        {
            AbilityIcon icon = transform.GetChild(i).GetComponent<AbilityIcon>();
            _icons.Add(icon);
            icon.Index = i + 1;
        }

        Manager.StatusMain.OnStatusChange += OnStatusChange;
    }

    void Update()
    {
        foreach (AbilityIcon icon in _icons)
        {
            // Keyboard navigation (buttons 1-9)
            if (Input.GetKeyDown(icon.Index.ToString()))
            {
                // Select ability
                if (_selected != icon)
                {
                    SelectAbility(icon);
                }
                // If already selected, activate
                else
                {
                    ActivateAbility();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnselectAbility(false);
        }
    }

    public void ToggleAbility(AbilityIcon icon)
    {
        if (_selected == icon)
        {
            SelectAbility(icon);
        }
        else
        {
            UnselectAbility(false);
        }
    }

    // Clears the selection and replaces it with a given ability icon.
    // Peforms the camera enter transition.
    public void SelectAbility(AbilityIcon icon)
    {
        ClearSelection();

        Color tmpColor = Color.blue;
        tmpColor.a = icon.Image.color.a;
        icon.Image.color = tmpColor;

        _titleText.text = icon.Ability.AbilityName;
        _descriptionText.text = icon.Ability.Description;
        _abilityDialog.SetActive(true);
        _confirm.interactable = icon.AnyAvailableTargets;

        _selected = icon;
        Manager.StatusMain.SetStatusSelectAction();
        if (icon.AnyAvailableTargets) icon.EnterTargetMode();
    }

    public void SelectShootAbility()
    {
        if (!_selected || _selected.Ability is not AbilityShoot)
        {
            foreach (AbilityIcon icon in _icons)
            {
                if (icon.Ability is AbilityShoot)
                {
                    SelectAbility(icon);
                    return;
                }
            }
        }
    }

    // Clears the selection, as well as performs the camera exit transition.
    public void UnselectAbility(bool keepTargetMode)
    {
        AbilityIcon oldSelected = ClearSelection();
        if (oldSelected != null && !keepTargetMode) oldSelected.ExitTargetMode();
    }

    // Clears the selection. If any ability was selected, returns it.
    public AbilityIcon ClearSelection()
    {
        if (_selected)
        {
            AbilityIcon tmp = _selected;

            Color tmpColor = Color.white;
            tmpColor.a = _selected.Image.color.a;
            _selected.Image.color = tmpColor;

            _abilityDialog.SetActive(false);
            _selected = null;

            return tmp;
        }

        return null;
    }

    public void ActivateAbility()
    {
        if (_selected && _selected.AnyAvailableTargets)
        {
            AbilityIcon icon = _selected;
            Manager.Instance.CharacterActivateAbility(icon);
            UnselectAbility(true);
        }
    }

    // Disables all abilities that target enemies if there are no visible enemies.
    // Otherwise, enables them. Abilities that do not target enemies become enabled as well.
    //
    // If the selected ability has changed from disabled to enabled, performs the
    // camera enter transition.
    private void DisableAbilitiesWithoutTargets(HashSet<Enemy> visibleEnemies)
    {
        foreach (AbilityIcon icon in _icons)
        {
            switch (icon.Ability.TargetType)
            {
                case TargetType.Enemy:
                    icon.AnyAvailableTargets = visibleEnemies.Count > 0;
                    break;
                case TargetType.Self:
                    icon.AnyAvailableTargets = true;
                    break;
                case TargetType.Summon:
                    icon.AnyAvailableTargets = icon.Ability.IsAvailable; //mb???
                    break;
            }

            if (_selected == icon && icon.AnyAvailableTargets)
            {
                icon.EnterTargetMode();
            }
        }

        UpdateConfirmButton();
    }

    private void DisableAllAbilites()
    {
        foreach (AbilityIcon icon in _icons)
        {
            icon.AnyAvailableTargets = false;
        }

        UpdateConfirmButton();
    }

    // Make the OK button interactable if the selected ability is enabled,
    // otherwise disable interactions.
    private void UpdateConfirmButton()
    {
        if (_selected) _confirm.interactable = _selected.AnyAvailableTargets;
    }

    private void OnStatusChange(HashSet<Permissions> permissions)
    {
        Manager.StatusMain.OnStatusChange -= OnStatusChange;
        if (!permissions.Contains(Permissions.ActionSelect))
        {
            gameObject.SetActive(false);
            _abilityDialog.SetActive(false);
            DisableAllAbilites();
        }
        else
        {
            gameObject.SetActive(true);
            _abilityDialog.SetActive(_selected != null);
            DisableAbilitiesWithoutTargets(TargetUtils.GetAvailableTargets(Manager.TurnManager.SelectedCharacter));
        }

        if (permissions.Contains(Permissions.AnimationRunning))
            UnselectAbility(false);

        Manager.StatusMain.OnStatusChange += OnStatusChange;
    }

    public void SetCharacter(Character character)
    {
        UpdateAbilities(character.Abilities);
    }

    public void UpdateAbilities(List<Ability> abilities)
    {
        Ability previouslySelectedAbility = _selected?.Ability;
        ClearAbilities();

        for (int i = 0; i < abilities.Count; i++)
        {
            AbilityIcon icon = Instantiate(_iconPrefab, transform).GetComponent<AbilityIcon>();
            icon.SetAbility(i, abilities[i]);

            _icons.Add(icon);
        }

        if (previouslySelectedAbility != null)
        {
            foreach (AbilityIcon icon in _icons)
            {
                if (previouslySelectedAbility.GetType() == icon.Ability.GetType())
                {
                    SelectAbility(icon);
                }
            }
        }

        DisableAbilitiesWithoutTargets(TargetUtils.GetAvailableTargets(Manager.TurnManager.SelectedCharacter));
    }

    public void ClearAbilities()
    {
        _icons.Clear();
        _selected = null;

        ObjectUtils.DestroyAllChildren(gameObject);
    }
}