using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ShelterDetecter : MonoBehaviour
{
    private void Start()
    {
        GameManagerMap.Instance.CharacterMovement.OnSelectNewTerritory +=
            (territory, character) => ShowHideShelters(territory.aktualTerritoryReaded, character);
        GameManagerMap.Instance.CharacterMovement.OnEndMoveToNewTerritory += DisableAllShelters;
    }

    private void ShowHideShelters(TerritroyReaded territory, CharacterInfo character)
    {
        Dictionary<ShelterSide, ShelterType> shelters = DetectShelters(territory);
        SetActiveShelters(shelters, character);
    }

    private void DisableAllShelters(TerritroyReaded territory, CharacterInfo character)
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
                // Access the corresponding ShelterType from the GameManagerMap
                ShelterType shelterType = ShelterType.None;
                if (GameManagerMap.Instance.Map.ContainsVertexByPox(TerritroyReaded.MakeVectorFromIndex(indexSet.First()), out var territoryReaded))
                {
                    shelterType = territoryReaded.ShelterType.ToDictionary()[side];
                }

                shelters[side] = shelterType;
            }
        }

        return shelters;
    }

    private void SetActiveShelters(Dictionary<ShelterSide, ShelterType> shelters, CharacterInfo character)
    {
        foreach (ShelterSide side in shelters.Keys.ToList())
        {
            //0 is full, 1 is semi
            character[(int)side].transform.GetChild(0).gameObject.SetActive(shelters[side] == ShelterType.Full);
            character[(int)side].transform.GetChild(1).gameObject.SetActive(shelters[side] == ShelterType.Semi);
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