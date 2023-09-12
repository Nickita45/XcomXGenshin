using System;
using System.Collections;
using UnityEngine;

public static class TargetingHelpers
{
    // Determines whether the entity A can target entity B with their abilities, and vice versa.
    // This works with both characters and enemies, as long as the correct maxDistance is supplied.
    //
    // This means the entities can see each other, are at an appropriate distance from one another,
    // and there are no major obstacles inbetween.
    public static bool CanTarget(Transform entityA, Transform entityB, float maxDistance)
    {
        // Can't target if the distance between entities is too big
        if (Vector3.Distance(entityA.position, entityB.position) > maxDistance) return false;

        // Try out all tiles around the entity B,
        // to account for being able to see through corners
        foreach (Vector3 direction in DIRECTIONS)
        {

            Vector3 origin = entityA.position;
            Vector3 target = entityB.position + direction;

            // Find difference vector between target and the origin
            Vector3 delta = target - origin;

            // Create a raycast from the entity A to the target tile
            RaycastHit[] hits = Physics.RaycastAll(origin, delta.normalized, delta.magnitude);
            bool anyShelter = false;

            foreach (RaycastHit hit in hits)
            {
                // Check if any hit is a full shelter
                TerritoryInfo info = hit.transform.GetComponent<TerritoryInfo>();
                if (info && info.Type == TerritoryType.Shelter && info.ShelterType.Left == ShelterType.Full &&
                    info.ShelterType.Right == ShelterType.Full && info.ShelterType.Bottom == ShelterType.Full
                        && info.ShelterType.Front == ShelterType.Full)
                {
                    //Debug.DrawRay(info.gameObject.transform.position, Vector3.up * 100f, Color.blue, 60f);
                    anyShelter = true;
                    break;
                }
            }

            // If there weren't any full shelters, the entities can target each other
            if (!anyShelter) return true;
        }

        return false;
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
