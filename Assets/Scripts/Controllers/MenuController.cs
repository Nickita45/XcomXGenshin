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
    private GameObject _panelEnemyTurn;

    private void Start()
    {
        GameManagerMap.Instance.Gun = GunType.Automatic;

        _inputCharacterMove.text = GameManagerMap.Instance.CharacterMovemovent.CountMoveCharacter.ToString();
        _inputCharacterSpeed.text = GameManagerMap.Instance.CharacterMovemovent.SpeedCharacter.ToString();
        _inputVisibilityDistance.text = GameManagerMap.Instance.CharacterVisibility.MaxVisionDistance.ToString();

        _inputCharacterMove.onValueChanged.AddListener(n =>
        {
            int.TryParse(n, out int result);
            GameManagerMap.Instance.CharacterMovemovent.CountMoveCharacter = result;
        });

        _inputCharacterSpeed.onValueChanged.AddListener(n =>
        {
            float.TryParse(n, out float result);
            GameManagerMap.Instance.CharacterMovemovent.SpeedCharacter = result;
        });

        _inputVisibilityDistance.onValueChanged.AddListener(n =>
        {
            float.TryParse(n, out float result);
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

        foreach (var icon in FindObjectsOfType<EnemyIcon>())
        {
            icon.SetPercent();
        }

        GameManagerMap.Instance.CharacterMovemovent.SelectedCharacter?.SetGunByIndex(selectedIndex);
    }

    public void SetPanelEnemy(bool set)
    {
        _panelEnemyTurn.SetActive(set);

    }

    public void SetMapByName(string name)
    {
        GameManagerMap.Instance.StatusMain.SetStatusZero();
        GameManagerMap.Instance.OnClearMap();
        GameManagerMap.Instance.DeReadingMap.DeSerelizete(name);
        StartCoroutine(AfterNewMap());
    }

    private IEnumerator AfterNewMap()
    {
        yield return new WaitForSeconds(1);
        GameManagerMap.Instance.TurnController.BeginOfTheTurn();
    }
}
