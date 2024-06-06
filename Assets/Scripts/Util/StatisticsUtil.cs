using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class StatisticsUtil
{
    private List<Sprite> _enemiesKilledList = new List<Sprite>();

    public int EnemiesDeathCount { get; set; }
    public int EnemiesTotalCount { get; set; }
    public int SoldierDeathCount { get; set; }
    public int SoldierTotalCount { get; set; }
    private HashSet<Character> _charactersWonded = new HashSet<Character>();
    public void SetEnemiesKilledList(Sprite enemyName)
    {
        _enemiesKilledList.Add(enemyName);
    }
    public IEnumerable<Sprite> GetEnemiesKilledList() => _enemiesKilledList;

    public void AddCharactersWonded(Character character) => _charactersWonded.Add(character);

    public int GetCountCharacterWonded => _charactersWonded.Count;
    /**
    5 point for every soldier survival, -2 if wonded, -3 if killed
    10 - max for killing enemies, formula = killed_enemies / max_enemies * 100 

    S+ - 19
    A - 17 
    B - 15 
    C - 13 
    D - 10
    E - 8
    F - Not passed 
    **/
    public string GetGrade()
    {
        int calculatingWantedKilled = SoldierTotalCount*5-(GetCountCharacterWonded*2)-(SoldierDeathCount*3);

        int scoreSoldiers = CalculateScore(calculatingWantedKilled,SoldierTotalCount*5,10);
        int scoreEnemies = CalculateScore(EnemiesDeathCount,EnemiesTotalCount,10);

        int countScore = scoreSoldiers + scoreEnemies;
        string grade = "F";
        if (countScore >= 19)
        {
            grade = "S+";
        }
        else if (countScore >= 17)
        {
            grade = "A";
        }
        else if (countScore >= 15)
        {
            grade = "B";
        }
        else if (countScore >= 13)
        {
            grade = "C";
        }
        else if (countScore >= 10)
        {
            grade = "D";
        }
        else if (countScore >= 8)
        {
            grade = "E";
        }
        return grade;
    }
    public static int CalculateScore(int killedEnemies, int totalEnemies, int maxScoreValue)
    {
        
        if (totalEnemies == 0)
        {
            return 0; //
        }

        double percentage = (double)killedEnemies / totalEnemies * 100;

        int score = (int)Math.Round(percentage / maxScoreValue);

        return score;
    }
}