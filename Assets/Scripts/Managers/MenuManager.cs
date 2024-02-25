using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown _dropDownGun;

    [SerializeField]
    private GameObject _panelEnemyTurn, _panelCharaterName;

    [SerializeField]
    private TextMeshProUGUI _textCharacter;

    private void Start()
    {
        //Manager.Gun = GunType.Automatic;
        Manager.StatusMain.OnStatusChange += OnStatusChange;

        /*_inputCharacterMove.text = Manager.CharacterMovement.CountMoveCharacter.ToString();
        _inputCharacterSpeed.text = Manager.CharacterMovement.SpeedCharacter.ToString();
        _inputVisibilityDistance.text = Manager.CharacterVisibility.MaxVisionDistance.ToString();

        _inputCharacterMove.onValueChanged.AddListener(n =>
        {
            int.TryParse(n, out int result);
            Manager.CharacterMovement.CountMoveCharacter = result;
        });

        _inputCharacterSpeed.onValueChanged.AddListener(n =>
        {
            float.TryParse(n, out float result);
            Manager.CharacterMovement.SpeedCharacter = result;
        });

        _inputVisibilityDistance.onValueChanged.AddListener(n =>
        {
            float.TryParse(n, out float result);
            Manager.CharacterVisibility.MaxVisionDistance = result;
        });
        */

        /* var gunTypeOptions = new List<string>();
         foreach (GunType gunType in Enum.GetValues(typeof(GunType)))
         {
             gunTypeOptions.Add(gunType.ToString());
         }
         _dropDownGun.AddOptions(gunTypeOptions);
         _dropDownGun.onValueChanged.AddListener(OnGunTypeDropdownValueChanged);
     */
    }

    /*  private void OnGunTypeDropdownValueChanged(int selectedIndex)
      {
          string selectedGunTypeText = _dropDownGun.options[selectedIndex].text;

          if (Enum.TryParse(selectedGunTypeText, out GunType selectedGunType))
          {
              Manager.Gun = selectedGunType;
          }

          foreach (var icon in FindObjectsOfType<EnemyIcon>())
          {
              icon.SetPercent();
          }

          Manager.CharacterMovement.SelectedCharacter?.SetGunByIndex(selectedIndex);
      }
    */
    public void SetPanelEnemy(bool set)
    {
        _panelEnemyTurn.SetActive(set);
    }

    public void SetMapByName(string name)
    {
        Manager.StatusMain.SetStatusZero();
        Manager.Instance.OnClearMap(); //Clear all elements which have different information in their memory
        Manager.DeReadingMap.DeSerelizete(name);
        StartCoroutine(AfterNewMap());
    }

    private IEnumerator AfterNewMap()
    {
        yield return new WaitForSeconds(1);
        Manager.TurnManager.BeginOfTheTurn();
    }

    private void OnStatusChange(HashSet<Permissions> permissions)
    {
        if (permissions.Contains(Permissions.ActionSelect) || permissions.Contains(Permissions.SelectEnemy))
        {
            _textCharacter.text = Manager.TurnManager.SelectedCharacter.Stats.CharacterName();
            _panelCharaterName.gameObject.SetActive(true);
        }
        else
            _panelCharaterName.gameObject.SetActive(false);
    }
}
