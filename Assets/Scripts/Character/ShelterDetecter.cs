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
        GameManagerMap.Instance.CharacterMovemovent.OnSelectNewTerritory += DetectShelters;
        GameManagerMap.Instance.CharacterMovemovent.OnEndMoveToNewTerritory += DisableAll;
    }

    public void DetectShelters((TerritroyReaded aktualTerritoryReaded, List<Vector3> path) territory, CharacterInfo character)
    {
        SidesShelter[] sides = { SidesShelter.Left, SidesShelter.Right, SidesShelter.Front, SidesShelter.Bottom };

        foreach (SidesShelter side in sides)
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

                // Call the DetecterAndSeter method with the corresponding ShelterType
                DetecterAndSeter(side, shelterType, character);
            }
            else
            {
                // If the index set is empty, call DetecterAndSeter with ShelterType.Nope
                DetecterAndSeter(side, ShelterType.Nope, character);
            }
        }
    }
    public void DisableAll(TerritroyReaded territory, CharacterInfo character)
    {
        foreach (SidesShelter side in Enum.GetValues(typeof(SidesShelter)))
        {
            DetecterAndSeter(side, ShelterType.Nope, character);
        }
    }

    private void DetecterAndSeter(SidesShelter side, ShelterType type, CharacterInfo character)
    {
        //0 is full, 1 is semi
        character[(int)side].transform.GetChild(0).gameObject.SetActive(type == ShelterType.Full);
        character[(int)side].transform.GetChild(1).gameObject.SetActive(type == ShelterType.Semi);

    }
}

enum SidesShelter
{
    Left = 0, Right = 1, Front = 2, Bottom = 3
}
