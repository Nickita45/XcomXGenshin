using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class MenuController : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField _inputCharacterSpeed;

    [SerializeField]
    private TMP_InputField _inputCharacterMove;

    [SerializeField]
    private TMP_InputField _inputVisibilityDistance;

    [SerializeField]
    private TMP_Dropdown _dropDownGun;

    [SerializeField]
    private GameObject _panelShootInfo;

    private void Start()
    {
        GameManagerMap.Instance.Gun = GunType.Automatic;
        GameManagerMap.Instance.StatusMain.OnStatusChange += OnStatusChange;

        _inputCharacterMove.text = GameManagerMap.Instance.CharacterMovemovent.CountMoveCharacter.ToString();
        _inputCharacterSpeed.text = GameManagerMap.Instance.CharacterMovemovent.SpeedCharacter.ToString();
        _inputVisibilityDistance.text = GameManagerMap.Instance.CharacterVisibility.MaxVisionDistance.ToString();

        _inputCharacterMove.onValueChanged.AddListener(n =>
        {

            int result = 0;
            int.TryParse(n, out result);
            GameManagerMap.Instance.CharacterMovemovent.CountMoveCharacter = result;
        });

        _inputCharacterSpeed.onValueChanged.AddListener(n =>
        {
            float result = 0;
            float.TryParse(n, out result);
            GameManagerMap.Instance.CharacterMovemovent.SpeedCharacter = result;
        });

        _inputVisibilityDistance.onValueChanged.AddListener(n =>
        {
            float result = 0;
            float.TryParse(n, out result);
            GameManagerMap.Instance.CharacterVisibility.MaxVisionDistance = result;
        });

        var gunTypeOptions = new List<string>();
        foreach (GunType gunType in Enum.GetValues(typeof(GunType)))
        {
            gunTypeOptions.Add(gunType.ToString());
        }
        _dropDownGun.AddOptions(gunTypeOptions);
        _dropDownGun.onValueChanged.AddListener(OnGunTypeDropdownValueChanged);
    }

    private void OnGunTypeDropdownValueChanged(int selectedIndex)
    {

        string selectedGunTypeText = _dropDownGun.options[selectedIndex].text;

        if (Enum.TryParse(selectedGunTypeText, out GunType selectedGunType))
        {
            GameManagerMap.Instance.Gun = selectedGunType;
        }

        var icons = FindObjectsOfType<EnemyIcon>();
        foreach (var icon in icons)
        {
            icon.SetPercent();
        }

        if (GameManagerMap.Instance.CharacterMovemovent.SelectedCharacter != null)
            GameManagerMap.Instance.CharacterMovemovent.SelectedCharacter.SetGunByIndex(selectedIndex);
    }


    public void SetMapByName(string name)
    {
        GameManagerMap.Instance.StatusMain.SetStatusZero();
        GameManagerMap.Instance.OnClearMap();
        GameManagerMap.Instance.DeReadingMap.DeSerelizete(name);
        GameManagerMap.Instance.StatusMain.SetStatusSelectCharacter();
    }

    private void OnStatusChange(HashSet<Permissions> permissions)
    {
        if (permissions.Contains(Permissions.SelectEnemy))
            _panelShootInfo.SetActive(true);
        else
            _panelShootInfo.SetActive(false);
    }
}
