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



    private void Start()
    {
        GameManagerMap.Instance.Gun = GunType.Automatic;

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

        var icons = FindObjectsOfType<EnemyIconClick>();
        foreach (var icon in icons)
        {
            icon.SetProcent();
        }

        if (GameManagerMap.Instance.CharacterMovemovent.SelectedCharacter != null)
            GameManagerMap.Instance.CharacterMovemovent.SelectedCharacter.SetGunByIndex(selectedIndex);
    }


    public void SetMapByName(string name)
    {
        GameManagerMap.Instance.OnClearMap();
        GameManagerMap.Instance.DeReadingMap.DeSerelizete(name);

    }
}
