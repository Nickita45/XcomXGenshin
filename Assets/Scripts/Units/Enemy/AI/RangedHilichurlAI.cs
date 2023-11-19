using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RangedHilichurlAI : EnemyAI
{
    private TerritroyReaded FindBestTerritoryForRangedAttack(Dictionary<TerritroyReaded, TerritroyReaded> allPaths)
    {
        List<Character> characters = _enemy.GetVisibleCharacters();
    
        (TerritroyReaded territory, float percent) minimum = (null, Int32.MinValue);
        foreach (var item in allPaths)
        {
            int procGetHit = characters.Sum(ch => AimUtils.CalculateHitChance(item.Key, ch.ActualTerritory, ch.Stats.Weapon, ch.Stats.BaseAimCharacter).percent);
            int procMakeHit = characters.Sum(ch => AimUtils.CalculateHitChance(ch.ActualTerritory, item.Key, GunType.Automatic, 50).percent); //???
            float proc = (2f * (100 - procGetHit) + 0.4f * procMakeHit) / (2f + 0.4f);
            if (proc > minimum.percent)
            {
                minimum = (item.Key, proc);
            }
            Debug.Log($"get hit procnet: {procGetHit}; make hit proc:{procMakeHit}; proc:{proc}; ter {item.Key}; count vis {string.Join(",", characters.Select(n => n.Stats.CharacterName()))}");
        }
        Debug.Log(minimum.percent + " " + allPaths.Count());
        return minimum.territory;
    }

    private AbilityShoot _shoot = new();
    private AbilityOverwatch _overwatch = new();

    public override IEnumerator MakeTurn()
    {
        var characters = _enemy.GetVisibleCharacters();
        var character = _enemy.GetClosestVisibleCharacter();

        if (character != null)
        {
            if (_enemy.ActionsLeft == 2 && characters.Select(ch =>
                        AimUtils.CalculateHitChance(_enemy.ActualTerritory, ch.ActualTerritory, ch.Stats.Weapon, _enemy.Stats.BaseAimPercent()).percent).Max() > 50)
            {
                _enemy.ActionsLeft -= 1;
                yield return StartCoroutine(_enemy.MoveEnemy(FindBestTerritoryForRangedAttack));
            }
            else
            {
                _enemy.ActionsLeft -= 2;
                yield return StartCoroutine(Attack(character));
            }
        }
        else
        {
            _enemy.ActionsLeft -= 1;

            if (_enemy.ActionsLeft == 0 && UnityEngine.Random.Range(1, 100 + 1) > 75)
                yield return StartCoroutine(_overwatch.Activate(_enemy, null));
            else
                yield return StartCoroutine(_enemy.MoveEnemy(FindTerritoryRandomShelter));
        }
    }

    public IEnumerator MakeOverwatch(Enemy enemy, Action onFinish)
    {
        Debug.Log("Enemy overwatch");
        enemy.ActionsLeft -= 2;
        enemy.StartCoroutine(enemy.Canvas.PanelShow(enemy.Canvas.PanelActionInfo("Overwatch"), 4));
        yield return new WaitForSeconds(2);
        onFinish();
    }

    public override IEnumerator Attack(Character character)
    {
        yield return _shoot.Activate(_enemy, character);
        yield return new WaitForSeconds(2);
    }
    public override TerritroyReaded TriggerEnemy(Dictionary<TerritroyReaded, TerritroyReaded> allPaths)
    {
        return FindBestTerritoryForRangedAttack(allPaths);
    }
}