using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class EnemyAI : MonoBehaviour
{
    protected Enemy _enemy;

    public virtual void Init(Enemy enemy)
    {
        _enemy = enemy;
    }

    // Get closer to the closest character
    public TerritroyReaded FindTerritoryToCharacter(Dictionary<TerritroyReaded, TerritroyReaded> allPaths)
    {
        var character = _enemy.GetClosestCharacter();
        if (character != null)
            return allPaths.Keys.OrderBy(n => Vector3.Distance(character.transform.localPosition, n.GetCordinats())).Where(n => n != _enemy.ActualTerritory).First();
        else
        {
            List<TerritroyReaded> keysList = new List<TerritroyReaded>(allPaths.Keys.Where(n => n != _enemy.ActualTerritory));
            Debug.Log("Random move");
            return keysList[UnityEngine.Random.Range(0, keysList.Count())];
        }
    }

    // Run away from the closest character
    public TerritroyReaded FindTerritoryFromCharacter(Dictionary<TerritroyReaded, TerritroyReaded> allPaths)
    {
        var character = _enemy.GetClosestCharacter();
        if (character != null)
        {
            Debug.Log("RUUUNN");
            return allPaths.Keys.OrderBy(n => Vector3.Distance(character.transform.localPosition, n.GetCordinats())).Where(n => n != _enemy.ActualTerritory).Last();
        }
        else
        {
            List<TerritroyReaded> keysList = new List<TerritroyReaded>(allPaths.Keys.Where(n => n != _enemy.ActualTerritory));
            Debug.Log("Random move");
            return keysList[UnityEngine.Random.Range(0, keysList.Count())];
        }
    }

    public abstract IEnumerator MakeTurn();
    public abstract IEnumerator Attack(Character character);
    public abstract TerritroyReaded TriggerEnemy(Dictionary<TerritroyReaded, TerritroyReaded> allPaths);
}