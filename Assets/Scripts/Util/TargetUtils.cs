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
    public static bool CanTarget(Transform unitA, Transform unitB, float maxDistance)
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
                if (info && IsBlocking(info, sides))
                {
                    blocked = true;
                    break;
                }
            }

            //Color color = blocked ? Color.blue : Color.red;
            //Debug.DrawRay(origin, delta, color, 60f);

            // If the ray wasn't blocked, targeting is possible
            if (!blocked) return true;
        }

        // If all rays are blocked, targeting is impossible
        return false;
    }

    // Detects whether a TerritoryInfo blocks the target ray.
    private static bool IsBlocking(TerritoryInfo info, List<ShelterSide> sides)
    {
        // 1. Block through the ground
        if (info.Type == TerritoryType.ShelterGround) return true;

        // 2. Block through full shelters on the targeted sides
        Dictionary<ShelterSide, ShelterType> shelters = info.ShelterType.ToDictionary();
        if (sides.Any(side => shelters[side] == ShelterType.Full)) return true;

        // Other territories do not block targeting
        return false;
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
            foreach (Enemy enemy in Manager.Map.Enemies
                .Where(e => TargetUtils.CanTarget(character.transform, e.transform, character.Stats.AttackRangedDistance())))
            {
                // Add a suitable enemy to the set
                _availableTargets.Add(enemy);
            }
        }

        return _availableTargets;
    }

    // Determines whether the unit A can see the unit B.
    public static bool CanSee(Unit a, Unit b)
    {
        return CanTarget(a.transform, b.transform, a.Stats.VisionDistance());
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
