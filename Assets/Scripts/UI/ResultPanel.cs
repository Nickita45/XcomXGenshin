using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanel : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _textMissionTitleResult; //Failed, Victory
    [SerializeField]
    private TextMeshProUGUI _textCountKilledEnemies, _textCountKilledSoldiers, _textCountWondedSoldiers, _textGrade;
    [SerializeField]
    private GameObject _prefabIconsEnemies;
    [SerializeField]
    private GameObject _objectPrefabRoot;
    private int _scrollMaxEnemiesIcons = 13;
    private StatisticsUtil _statisticsUtil;
    
    public void SetResultPanel()
    {
        _statisticsUtil = Manager.Instance.StatisticsUtil;

        if (_statisticsUtil.SoldierDeathCount == _statisticsUtil.SoldierTotalCount)
        {
            _textMissionTitleResult.text = "Mission failed";
        }
        else
        {
            _textMissionTitleResult.text = "Mission completed";
        }

        _textCountKilledEnemies.text = _statisticsUtil.EnemiesDeathCount + "/" + _statisticsUtil.EnemiesTotalCount;

        _textCountKilledSoldiers.text = _statisticsUtil.SoldierDeathCount + "/" + _statisticsUtil.SoldierTotalCount;
        _textCountWondedSoldiers.text = _statisticsUtil.GetCountCharacterWonded + "/" + _statisticsUtil.SoldierTotalCount;

        for (int i = 0; i < _statisticsUtil.EnemiesDeathCount && i < _scrollMaxEnemiesIcons; i++)
        {
            GameObject generatedIconEnemy = Instantiate(_prefabIconsEnemies, _objectPrefabRoot.transform);
            generatedIconEnemy.GetComponent<Image>().sprite = Manager.Instance.StatisticsUtil.GetEnemiesKilledList().ElementAt(i);
        }

        _textGrade.text = _statisticsUtil.GetGrade();
    }

   
}