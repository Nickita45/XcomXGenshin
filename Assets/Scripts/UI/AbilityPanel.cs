using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityPanel : MonoBehaviour
{
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
        GameManagerMap.Instance.OnClearMap += UnselectAbility;
        _confirm.onClick.AddListener(ActivateAbility);

        for (int i = 0; i < transform.childCount; i++)
        {
            AbilityIcon icon = transform.GetChild(i).GetComponent<AbilityIcon>();
            _icons.Add(icon);
            icon.Index = i + 1;
        }

        GameManagerMap.Instance.CharacterVisibility.OnVisibilityUpdate += DisableAbilitiesWithoutTargets;
        GameManagerMap.Instance.CharacterMovemovent.OnStartMove += DisableAllAbilites;
        GameManagerMap.Instance.CharacterMovemovent.OnEndMoveToNewTerritory += (territory, info) => UpdateConfirmButton();
        GameManagerMap.Instance.StatusMain.OnStatusChange += OnStatusChange;
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
            UnselectAbility();
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
            UnselectAbility();
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

        _titleText.text = icon.Title;
        _descriptionText.text = icon.Description;
        _abilityDialog.SetActive(true);
        _confirm.interactable = icon.AbilityEnabled;

        _selected = icon;
        if (icon.AbilityEnabled) icon.SetupCameraForTargetEnter();
    }

    // Clears the selection, as well as performs the camera exit transition.
    public void UnselectAbility()
    {
        AbilityIcon wasSelected = ClearSelection();
        if (wasSelected != null) wasSelected.SetupCameraForTargetExit();
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
        if (_selected && _selected.AbilityEnabled)
        {
            _selected.Event.Invoke(UnselectAbility);
        }
    }

    // Disables all abilities that target enemies if there are no visible enemies.
    // Otherwise, enables them. Abilities that do not target enemies become enabled as well.
    //
    // If the selected ability has changed from disabled to enabled, performs the
    // camera enter transition.
    private void DisableAbilitiesWithoutTargets(HashSet<GameObject> visibleEnemies)
    {
        bool enabled = visibleEnemies.Count > 0;

        foreach (AbilityIcon icon in _icons)
        {
            if (icon.TargetType == AbilityTargetType.Enemy)
            {
                if (_selected == icon && !icon.AbilityEnabled && enabled)
                {
                    icon.SetupCameraForTargetEnter();
                }

                icon.AbilityEnabled = enabled;
            }
            else
            {
                icon.AbilityEnabled = true;
            }
        }

        UpdateConfirmButton();
    }

    private void DisableAllAbilites()
    {
        foreach (AbilityIcon icon in _icons)
        {
            icon.AbilityEnabled = false;
        }

        UpdateConfirmButton();
    }

    // Make the OK button interactable if the selected ability is enabled,
    // otherwise disable interactions.
    private void UpdateConfirmButton()
    {
        if (_selected) _confirm.interactable = _selected.AbilityEnabled;
    }

    private void OnStatusChange(HashSet<Permissions> permissions)
    {
        if (permissions.Contains(Permissions.AnimationShooting) || permissions.Contains(Permissions.Waiting))
        {
            gameObject.SetActive(false);
            _abilityDialog.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
            _abilityDialog.SetActive(_selected != null);
        }
    }
}