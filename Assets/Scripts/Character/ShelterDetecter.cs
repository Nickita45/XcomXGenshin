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
        GameManagerMap.Instance.CharacterMovemovent.OnSelectNewTerritory += ShowHideShelters;
        GameManagerMap.Instance.CharacterMovemovent.OnEndMoveToNewTerritory += DisableAllShelters;
    }

    private void ShowHideShelters((TerritroyReaded, List<Vector3>) territory, CharacterInfo character)
    {
        ShelterType[] shelters = DetectShelters(territory, character);
        SetActiveShelters(shelters, character);
    }

    private void DisableAllShelters(TerritroyReaded territory, CharacterInfo character)
    {
        SetActiveShelters(NO_SHELTERS, character);
    }

    public ShelterType[] DetectShelters((TerritroyReaded aktualTerritoryReaded, List<Vector3> path) territory, CharacterInfo character)
    {
        ShelterType[] shelters = NO_SHELTERS;

        foreach (SidesShelter side in SIDES)
        {
            HashSet<string> indexSet = null;

            // Choose the corresponding index set based on the side
            switch (side)
            {
                case SidesShelter.Left:
                    indexSet = territory.aktualTerritoryReaded.IndexLeft;
                    break;
                case SidesShelter.Right:
                    indexSet = territory.aktualTerritoryReaded.IndexRight;
                    break;
                case SidesShelter.Front:
                    indexSet = territory.aktualTerritoryReaded.IndexFront;
                    break;
                case SidesShelter.Bottom:
                    indexSet = territory.aktualTerritoryReaded.IndexBottom;
                    break;
            }

            // Check if the index set has elements
            if (indexSet?.Count > 0)
            {
                // Access the corresponding ShelterType from the GameManagerMap
                ShelterType shelterType = ShelterType.Nope;
                if (GameManagerMap.Instance.Map.ContainsVertexByPox(TerritroyReaded.MakeVectorFromIndex(indexSet.First()), out var territoryReaded))
                {
                    switch (side)
                    {
                        case SidesShelter.Left:
                            shelterType = territoryReaded.ShelterType.Right;
                            break;
                        case SidesShelter.Right:
                            shelterType = territoryReaded.ShelterType.Left;
                            break;
                        case SidesShelter.Front:
                            shelterType = territoryReaded.ShelterType.Bottom;
                            break;
                        case SidesShelter.Bottom:
                            shelterType = territoryReaded.ShelterType.Front;
                            break;
                    }
                }

                shelters[(int)side] = shelterType;
            }
        }

        return shelters;
    }

    private void SetActiveShelters(ShelterType[] shelters, CharacterInfo character)
    {
        foreach (SidesShelter side in SIDES)
        {
            //0 is full, 1 is semi
            character[(int)side].transform.GetChild(0).gameObject.SetActive(shelters[(int)side] == ShelterType.Full);
            character[(int)side].transform.GetChild(1).gameObject.SetActive(shelters[(int)side] == ShelterType.Semi);
        }
    }

    enum SidesShelter
    {
        Left = 0, Right = 1, Front = 2, Bottom = 3
    }

    private static readonly SidesShelter[] SIDES = {
        SidesShelter.Left,
        SidesShelter.Right,
        SidesShelter.Front,
        SidesShelter.Bottom
    };

    private static readonly ShelterType[] NO_SHELTERS = {
        ShelterType.Nope,
        ShelterType.Nope,
        ShelterType.Nope,
        ShelterType.Nope
    };
}