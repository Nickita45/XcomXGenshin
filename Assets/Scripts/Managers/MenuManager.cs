using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.IO;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown _dropdownLevel;

    [SerializeField]
    private GameObject _panelEnemyTurn, _panelCharaterName;

    [SerializeField]
    private TextMeshProUGUI _textCharacter;

    private void Start()
    {
        Manager.StatusMain.OnStatusChange += OnStatusChange;

        // Scan levels folder and add levels to dropdown
        string[] files = Directory.GetFiles("Assets/Resources/Levels");
        foreach (string file in files)
        {
            if (Path.GetExtension(file) == ".json")
            {
                string level = Path.GetFileNameWithoutExtension(file);
                _dropdownLevel.options.Add(new TMP_Dropdown.OptionData(level));
            }
        }
    }

    public void OnLevelDropdownValueChanged()
    {
        SetMapByName("Levels/" + _dropdownLevel.options[_dropdownLevel.value].text);
    }

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
        Manager.TurnManager.LoadCharacterData();
        yield return StartCoroutine(Manager.TurnManager.BeginRound());
    }

    private void OnStatusChange(HashSet<Permissions> permissions)
    {
        if (permissions.Contains(Permissions.ActionSelect) || permissions.Contains(Permissions.SelectEnemy))
        {
            _textCharacter.text = Manager.TurnManager.SelectedCharacter.Stats.Name();
            _panelCharaterName.gameObject.SetActive(true);
        }
        else
            _panelCharaterName.gameObject.SetActive(false);
    }
}
