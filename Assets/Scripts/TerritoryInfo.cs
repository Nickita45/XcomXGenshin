using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerritoryInfo : MonoBehaviour
{
    [SerializeField]
    private TerritoryType type;
    [SerializeField]
    private string PathPrefab;

    public string Path => PathPrefab;
    public TerritoryType Type => type;
}

public enum TerritoryType
{
    Undefined,
    Ground,
    Air,
    Shelter,
    ShelterGround,
    Boarder,
    MapObject
     
}
