using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TargetUtils
{
    // Determines whether the unit A can target unit B with their ranged abilities.
    // This works with both characters and enemies, as long as the correct maxDistance is supplied.
    //
    // This means the units can see each other, are at an appropriate distance from one another,
    // and there are no major obstacles inbetween.
    public static bool CanTarget(TerritroyReaded unitA, TerritroyReaded unitB, float maxDistance, bool debug = false)
    {
        // Can't target if the distance between units is too big
        if (Math.Round(Vector3.Distance(unitA.GetCordinats(), unitB.GetCordinats())) > maxDistance) return false;

        if (IsCanSeeMakeRayCastDetect(unitA.GetCordinats(), unitB.GetCordinats())) return true;

        if(unitA.IndexLeft.Count > 0 && unitB.IndexLeft.Count > 0)
            if(ValidateShelter(Manager.Map[unitA.IndexLeft.First()].TerritoryInfo) && ValidateShelter(Manager.Map[unitB.IndexLeft.First()].TerritoryInfo))
                if (IsCanSeeMakeRayCastDetect(Manager.Map[unitA.IndexLeft.First()].GetCordinats(), Manager.Map[unitB.IndexLeft.First()].GetCordinats())) return true;

        if (unitA.IndexRight.Count > 0 && unitB.IndexRight.Count > 0)
            if (ValidateShelter(Manager.Map[unitA.IndexRight.First()].TerritoryInfo) && ValidateShelter(Manager.Map[unitB.IndexRight.First()].TerritoryInfo))
                if (IsCanSeeMakeRayCastDetect(Manager.Map[unitA.IndexRight.First()].GetCordinats(), Manager.Map[unitB.IndexRight.First()].GetCordinats())) return true;

        if (unitA.IndexBottom.Count > 0 && unitB.IndexBottom.Count > 0)
            if (ValidateShelter(Manager.Map[unitA.IndexBottom.First()].TerritoryInfo) && ValidateShelter(Manager.Map[unitB.IndexBottom.First()].TerritoryInfo))
                if (IsCanSeeMakeRayCastDetect(Manager.Map[unitA.IndexBottom.First()].GetCordinats(), Manager.Map[unitB.IndexBottom.First()].GetCordinats())) return true;
        
        if (unitA.IndexFront.Count > 0 && unitB.IndexFront.Count > 0)
            if (ValidateShelter(Manager.Map[unitA.IndexFront.First()].TerritoryInfo) && ValidateShelter(Manager.Map[unitB.IndexFront.First()].TerritoryInfo))
                if (IsCanSeeMakeRayCastDetect(Manager.Map[unitA.IndexFront.First()].GetCordinats(), Manager.Map[unitB.IndexFront.First()].GetCordinats())) return true;
        
        // If all rays are blocked, targeting is impossible
        return false;
    }

    public static bool CanTarget(Transform unitA, Transform unitB, float maxDistance, bool debug = false)
    {
        // Can't target if the distance between units is too big
        if (Math.Round(Vector3.Distance(unitA.position, unitB.position)) > maxDistance) return false;

        return IsCanSeeMakeRayCastDetect(unitA.position, unitB.position);
    }

    // Detects whether a TerritoryInfo blocks the target ray.
    private static bool IsBlocking(TerritoryInfo info, List<ShelterSide> sides)
    {
        // 1. Block through the ground
        if (!ValidateShelter(info.Type)) return true;
        //if (info.Type == TerritoryType.ShelterGround
        //    || info.Type == TerritoryType.Ground) return true;

        // 2. Block through full shelters on the targeted sides
        Dictionary<ShelterSide, ShelterType> shelters = info.ShelterType.ToDictionary();
        if (sides.Any(side => shelters[side] == ShelterType.Full)) return true;

        // Other territories do not block targeting
        return false;
    }

    private static bool ValidateShelter(TerritoryType territory) => territory != TerritoryType.Shelter && territory != TerritoryType.ShelterGround;

    private static bool IsCanSeeMakeRayCastDetect(Vector3 origin, Vector3 target, bool debug = false)
    {
        // Find difference vector between target and the origin
        Vector3 delta = target - origin;

        // Create a raycast from the target to the origin tile and
        // detect all territories along the ray.
        // 
        // The ray is done in inverse direction because it is more reliable
        // at detecting colliders at the end of the ray.
        RaycastHit[] hits = Physics.RaycastAll(target, -delta.normalized, delta.magnitude);
        bool blocked = false;

        // Find out which sides of the shelter are targeted
        List<ShelterSide> sides = new();

        if (delta.z < 0) sides.Add(ShelterSide.Front);
        else if (delta.z < 0) sides.Add(ShelterSide.Back);

        if (delta.x > 0) sides.Add(ShelterSide.Right);
        else if (delta.x < 0) sides.Add(ShelterSide.Left);

        foreach (RaycastHit hit in hits)
        {
            TerritoryInfo info = hit.transform.GetComponent<TerritoryInfo>();

            // Check if the hit is blocking
            if (info && IsBlocking(info, sides))
            {
                blocked = true;
                break;
            }
        }

        if (debug)
        {
            Color color = blocked ? Color.blue : Color.red;
            Debug.DrawRay(origin, delta, color, 60f);
        }
        // If the ray wasn't blocked, targeting is possible
        return !blocked;
        //if (!blocked) return true;

    }

    // Gets the list of all enemies
    // that can be targeted by the character's ranged attack.
    //
    // If the character is null, returns an empty list.
    public static HashSet<Enemy> GetAvailableTargets(Character character)
    {
        HashSet<Enemy> _availableTargets = new();

        // If a character is selected, look for available targets
        if (character)
        {
            foreach (Enemy enemy in Manager.Map.Enemies.GetList
                .Where(e => TargetUtils.CanTarget(character.ActualTerritory, e.ActualTerritory, character.Stats.AttackRangedDistance())))
            {
                // Add a suitable enemy to the set
                _availableTargets.Add(enemy);
            }
        }

        return _availableTargets;
    }

    /// <summary>
    /// Determines whether the unit A can see the unit B.
    /// With stats from A.
    /// </summary>
    /// <param name="a">Watcher</param>
    /// <param name="b">Target</param>
    /// <returns>Is unit A see unit B</returns>
    public static bool CanSee(Unit a, Unit b, bool debug = false)
    {
        return CanTarget(a.ActualTerritory, b.ActualTerritory, a.Stats.VisionDistance(), debug);
    }

    /// <summary>
    /// Determines whether the unit A can see the unit B.
    /// </summary>
    /// <param name="a">Watcher</param>
    /// <param name="b">Target</param>
    /// <param name="distance">Max Distance</param>
    /// <returns>Is unit A see unit B</returns>
    public static bool CanSee(Transform a, Transform b, float distance, bool debug = false)
    {
        return CanTarget(a, b, distance, debug);
    }

    private static readonly Vector3[] DIRECTIONS = {
        Vector3.zero,
        Vector3.forward,
        Vector3.back,
        Vector3.right,
        Vector3.left,
        Vector3.forward + Vector3.right,
        Vector3.forward + Vector3.left,
        Vector3.back + Vector3.right,
        Vector3.back + Vector3.left
    };
}
