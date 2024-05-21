using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public abstract class EnemyAI : MonoBehaviour
{
    protected Enemy _enemy;

    public virtual void Init(Enemy enemy)
    {
        _enemy = enemy;
        _enemy.Health.OnResistance += OnResistance;
    }

    // Gets a random territory from all the paths,
    // excluding the territory corressponding to the enemy position.
    //
    // Returns null if there's no possible paths.
    public TerritroyReaded FindTerritoryRandom(Dictionary<TerritroyReaded, TerritroyReaded> allPaths)
    {
        Debug.Log("Random move");
        List<TerritroyReaded> possibleTerritories = allPaths.Keys
            .Where(n => n != _enemy.ActualTerritory).ToList();

        if (possibleTerritories.Count == 0) return null;
        return possibleTerritories[UnityEngine.Random.Range(0, possibleTerritories.Count())];
    }

    // Gets a random territory from all the paths,
    // excluding the territory corressponding to the enemy position.
    // Always tries to get to behind a shelter if possible.
    // 
    // Returns null if there's no possible paths.
    public TerritroyReaded FindTerritoryRandomShelter(Dictionary<TerritroyReaded, TerritroyReaded> allPaths)
    {
        List<TerritroyReaded> possibleTerritories = allPaths.Keys
            .Where(n => n != _enemy.ActualTerritory && n.IsNearShelter()).ToList();

        if (possibleTerritories.Count == 0) return FindTerritoryRandom(allPaths);

        Debug.Log("Random move to shelter");
        return possibleTerritories[UnityEngine.Random.Range(0, possibleTerritories.Count())];
    }

    // Get closer to the closest character.
    //
    // Returns null if there's no possible paths.
    public TerritroyReaded FindTerritoryToCharacter(Dictionary<TerritroyReaded, TerritroyReaded> allPaths)
    {
        var character = _enemy.GetClosestCharacter();
        if (character != null)
        {
            Debug.Log("Run to character");
            List<TerritroyReaded> possibleTerritories = allPaths.Keys
                .OrderBy(n => Vector3.Distance(character.transform.localPosition, n.GetCordinats()))
                .Where(n => n != _enemy.ActualTerritory).ToList();

            if (possibleTerritories.Count == 0) return null;
            return possibleTerritories.First();
        }

        return FindTerritoryRandom(allPaths);
    }

    // Run away from the closest character.
    //
    // Returns null if there's no possible paths. (Maybe can be only in Slime AI?)
    public TerritroyReaded FindTerritoryFromCharacter(Dictionary<TerritroyReaded, TerritroyReaded> allPaths)
    {
        var character = _enemy.GetClosestCharacter();
        if (character != null)
        {
            Debug.Log("Run away from character");
            List<TerritroyReaded> possibleTerritories = allPaths
               .Keys
               .OrderBy(n => Vector3.Distance(character.transform.localPosition, n.GetCordinats()))
               .Where(n => n != _enemy.ActualTerritory)
               .ToList();

            if (possibleTerritories.Count == 0) return null;
            return possibleTerritories.Last();
        }

        return FindTerritoryRandom(allPaths);
    }

    public virtual int OnResistance(int hit, Element element) { return hit; }
    public virtual void OnSpawn() { }
    public abstract IEnumerator MakeTurn();
    public abstract IEnumerator Attack(Character character);
    public abstract TerritroyReaded TriggerEnemy(Dictionary<TerritroyReaded, TerritroyReaded> allPaths);
}