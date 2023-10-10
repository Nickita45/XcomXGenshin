using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ShelterDetectUtils
{
    public static void UpdateShelterObjects(TerritroyReaded territory, Character character)
    {
        Dictionary<ShelterSide, ShelterType> shelters = DetectShelters(territory);
        SetActiveShelters(shelters, character);
    }

    public static void DisableShelterObjects(Character character)
    {
        SetActiveShelters(ShelterInfo.EMPTY.ToDictionary(), character);
    }

    public static Dictionary<ShelterSide, ShelterType> DetectShelters(TerritroyReaded territory)
    {
        Dictionary<ShelterSide, ShelterType> shelters = ShelterInfo.EMPTY.ToDictionary();

        foreach (ShelterSide side in shelters.Keys.ToList())
        {
            HashSet<string> indexSet = null;

            // Choose the corresponding index set based on the side
            switch (side)
            {
                case ShelterSide.Left:
                    indexSet = territory.IndexLeft;
                    break;
                case ShelterSide.Right:
                    indexSet = territory.IndexRight;
                    break;
                case ShelterSide.Front:
                    indexSet = territory.IndexFront;
                    break;
                case ShelterSide.Back:
                    indexSet = territory.IndexBottom;
                    break;
            }

            // Check if the index set has elements
            if (indexSet?.Count > 0)
            {
                // Access the corresponding ShelterType from the Manager
                ShelterType shelterType = ShelterType.None;
                if (Manager.Map.ContainsVertexByPox(TerritroyReaded.MakeVectorFromIndex(indexSet.First()), out var territoryReaded))
                {
                    shelterType = territoryReaded.ShelterType.ToDictionary()[side];
                }

                shelters[side] = shelterType;
            }
        }

        return shelters;
    }

    private static void SetActiveShelters(Dictionary<ShelterSide, ShelterType> shelters, Character character)
    {
        foreach (ShelterSide side in shelters.Keys.ToList())
        {
            //0 is full, 1 is semi
            // TODO is this right?
            Transform shelter = character.Mover.transform.GetChild((int)side).transform;
            shelter.GetChild(0).gameObject.SetActive(shelters[side] == ShelterType.Full);
            shelter.GetChild(1).gameObject.SetActive(shelters[side] == ShelterType.Semi);
        }
    }

    public static Vector3 ShelterSideToDirectionVector(ShelterSide side)
    {
        Vector3[] directions = new[] {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right
        };
        return directions[(int)side];
    }
}