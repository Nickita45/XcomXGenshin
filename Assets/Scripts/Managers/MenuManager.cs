using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.IO;
using System.Linq;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown _dropdownLevel;

    [SerializeField]
    private GameObject _panelEnemyTurn, _panelCharaterName;

    [SerializeField]
    private ResultPanel _panelResult;

    [SerializeField]
    private TextMeshProUGUI _textCharacter;
    [SerializeField]
    

    private void Start()
    {
        Manager.StatusMain.OnStatusChange += OnStatusChange;

        string[] levelNames = Resources.LoadAll<TextAsset>("Levels").Select(level => level.name).ToArray();

        foreach (string levelName in levelNames)
        {
           _dropdownLevel.options.Add(new TMP_Dropdown.OptionData(levelName));
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
        FindObjectOfType<GrassComputeScript>()?.Reset();
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
    public void SetPanelResult()
    {
        bool set = !_panelResult.gameObject.activeInHierarchy;

        _panelResult.gameObject.SetActive(set);
        _panelResult.GetComponent<ResultPanel>().SetResultPanel();
    }
}
