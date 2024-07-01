using System;
using System.Collections;
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
    public static bool CanTarget(Transform unitA, Transform unitB, float maxDistance, bool debug = false)
    {
        // Can't target if the distance between units is too big
        if (Vector3.Distance(unitA.position, unitB.position) > maxDistance) return false;

        // Try out all tiles around the unit B,
        // to account for being able to see through corners
        foreach (Vector3 direction in DIRECTIONS)
        {
            Vector3 origin = unitA.position + direction;
            Vector3 target = unitB.position;

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
                if (info && IsBlocking(info, sides, unitA))
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
            if (!blocked) return true;
        }

        // If all rays are blocked, targeting is impossible
        return false;
    }

    // Detects whether a TerritoryInfo blocks the target ray.
    private static bool IsBlocking(TerritoryInfo info, List<ShelterSide> sides, Transform unitA)
    {
        // 1. Block through the ground
        if (info.Type == TerritoryType.ShelterGround
            || info.Type == TerritoryType.Ground) return true;

        // 2. Block through full shelters on the targeted sides
        //Dictionary<ShelterSide, ShelterType> shelters = info.ShelterType.ToDictionary();
        //if (sides.Any(side => shelters[side] == ShelterType.Full)) return true;
        TerritroyReaded infoTerritoryReaded = Manager.Map[info.transform.localPosition]; //mb refactoring in future
        Dictionary<ShelterSide, ShelterType> shelters = info.ShelterType.ToDictionary();

        // all these checks 3 blocks (1 of self and 2 others)
        if (infoTerritoryReaded != null) 
        {
            if (sides.Contains(ShelterSide.Back) || sides.Contains(ShelterSide.Front)) // for - (front) and _ (back)
            {
                Dictionary<ShelterSide, ShelterType> sheltersLeft =
                    Manager.Map[infoTerritoryReaded.IndexLeft?.First()].ShelterType.ToDictionary();
                Dictionary<ShelterSide, ShelterType> sheltersRight =
                    Manager.Map[infoTerritoryReaded.IndexRight?.First()].ShelterType.ToDictionary();

                if (CheckSheltersSides(sides, shelters)
                    && CheckSheltersSides(sides, sheltersLeft)
                    && CheckSheltersSides(sides, sheltersRight)) return true;
            }
            else if (sides.Contains(ShelterSide.Left) || sides.Contains(ShelterSide.Right)) // for | (left) and | (right)
            {
                Dictionary<ShelterSide, ShelterType> sheltersFront =
                                        Manager.Map[infoTerritoryReaded.IndexFront?.First()].ShelterType.ToDictionary();
                Dictionary<ShelterSide, ShelterType> sheltersBottom =
                    Manager.Map[infoTerritoryReaded.IndexBottom?.First()].ShelterType.ToDictionary();

                if (CheckSheltersSides(sides, shelters)
                   && CheckSheltersSides(sides, sheltersFront)
                   && CheckSheltersSides(sides, sheltersBottom)) return true;
            }

            if(sides.Contains(ShelterSide.Left) && sides.Contains(ShelterSide.Front)) // for _|
            {
                Dictionary<ShelterSide, ShelterType> sheltersLeft =
                     Manager.Map[infoTerritoryReaded.IndexLeft?.First()].ShelterType.ToDictionary();
                Dictionary<ShelterSide, ShelterType> sheltersFront =
                    Manager.Map[infoTerritoryReaded.IndexFront?.First()].ShelterType.ToDictionary();

                if (CheckSheltersSides(sides, shelters)
                   && CheckSheltersSides(sides, sheltersFront)
                   && CheckSheltersSides(sides, sheltersLeft)) return true;

            } else if(sides.Contains(ShelterSide.Right) && sides.Contains(ShelterSide.Front)) // for |_
            {
                Dictionary<ShelterSide, ShelterType> sheltersRight =
                     Manager.Map[infoTerritoryReaded.IndexRight?.First()].ShelterType.ToDictionary();
                Dictionary<ShelterSide, ShelterType> sheltersFront =
                    Manager.Map[infoTerritoryReaded.IndexFront?.First()].ShelterType.ToDictionary();

                if (CheckSheltersSides(sides, shelters)
                   && CheckSheltersSides(sides, sheltersFront)
                   && CheckSheltersSides(sides, sheltersRight)) return true;

            } else if (sides.Contains(ShelterSide.Right) && sides.Contains(ShelterSide.Back)) // for |-
            {
                Dictionary<ShelterSide, ShelterType> sheltersRight =
                     Manager.Map[infoTerritoryReaded.IndexRight?.First()].ShelterType.ToDictionary();
                Dictionary<ShelterSide, ShelterType> sheltersBottom =
                    Manager.Map[infoTerritoryReaded.IndexBottom?.First()].ShelterType.ToDictionary();

                if (CheckSheltersSides(sides, shelters)
                  && CheckSheltersSides(sides, sheltersBottom)
                  && CheckSheltersSides(sides, sheltersRight)) return true;

            } else if (sides.Contains(ShelterSide.Left) && sides.Contains(ShelterSide.Back)) // for -|
            {
                Dictionary<ShelterSide, ShelterType> sheltersLeft =
                     Manager.Map[infoTerritoryReaded.IndexLeft?.First()].ShelterType.ToDictionary();
                Dictionary<ShelterSide, ShelterType> sheltersBottom =
                    Manager.Map[infoTerritoryReaded.IndexBottom?.First()].ShelterType.ToDictionary();

                if (CheckSheltersSides(sides, shelters)
                  && CheckSheltersSides(sides, sheltersBottom)
                  && CheckSheltersSides(sides, sheltersLeft)) return true;
            }
        }

        // Other territories do not block targeting
        return false;
    }

    private static bool CheckSheltersSides(List<ShelterSide> sides, Dictionary<ShelterSide, ShelterType> shelters)
    {
        return shelters == null || sides.Any(side => shelters[side] == ShelterType.Full);
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
                .Where(e => TargetUtils.CanTarget(character.transform, e.transform, character.Stats.AttackRangedDistance())))
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
        return CanTarget(a.transform, b.transform, a.Stats.VisionDistance(), debug);
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
