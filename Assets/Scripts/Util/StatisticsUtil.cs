using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public  class StatisticsUtil {
    private List<Sprite> _enemiesKilledList = new List<Sprite>();

    public int EnemiesDeathCount {get;set;}
    public int EnemiesTotalCount {get;set;}
    public int SoldierDeathCount {get;set;}
    public int SoldierTotalCount {get;set;}
    private HashSet<Character> _charactersWonded = new HashSet<Character>();
    public void SetEnemiesKilledList(Sprite enemyName)
    {
        _enemiesKilledList.Add(enemyName);
    }
    public IEnumerable<Sprite> GetEnemiesKilledList() => _enemiesKilledList;

    public void AddCharactersWonded(Character character) => _charactersWonded.Add(character);
    
    public int GetCountCharacterWonded => _charactersWonded.Count;
    public string GetGrade()
    {
        string grade = "F";
        if (SoldierDeathCount == 0 && GetCountCharacterWonded == 0 && EnemiesDeathCount == EnemiesTotalCount)
        {
            grade = "S+";
        }
        else if (SoldierDeathCount == 0 && GetCountCharacterWonded < 3 && EnemiesDeathCount > EnemiesTotalCount / 2)
        {
            grade = "A";
        }
        else if (SoldierDeathCount < 2 && GetCountCharacterWonded < 4)
        {
            grade = "B";
        }
        else if (SoldierDeathCount < 3 && GetCountCharacterWonded < 4)
        {
            grade = "C";
        }
        else if (SoldierDeathCount < 4 && GetCountCharacterWonded < 4)
        {
            grade = "D";
        }
        else if (SoldierDeathCount < 3 && GetCountCharacterWonded < 5)
        {
            grade = "E";
        }
        return grade;
    }
}