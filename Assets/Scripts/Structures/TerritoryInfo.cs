using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerritoryInfo : MonoBehaviour
{

    [Header("Basic Settings")]
    [SerializeField]
    private TerritoryType _type;
    [SerializeField]
    private string _pathPrefab;

    [Header("Shelter Settings")]
    [SerializeField]
    private ShelterInfo _shelterInfo;
    [SerializeField]
    private ShelterType _setOnStartToEveryone;//exept "Nope"
    public string Path => _pathPrefab;
    public TerritoryType Type { get => _type; set => _type = value; }
    public ShelterInfo ShelterType { get => _shelterInfo; set => _shelterInfo = value; }



    private void Start()
    {


        if (_setOnStartToEveryone == global::ShelterType.Full || _setOnStartToEveryone == global::ShelterType.Semi)
        {
            _shelterInfo.SetForEvery(_setOnStartToEveryone);
        }
    }

}

[System.Serializable]
public class ShelterInfo
{
    [SerializeField]
    private ShelterType _shelterInfoLeft;
    [SerializeField]
    private ShelterType _shelterInfoRight;
    [SerializeField]
    private ShelterType _shelterInfoBottom;
    [SerializeField]
    private ShelterType _shelterInfoFront;

    public ShelterType Left { get => _shelterInfoLeft; set => _shelterInfoLeft = value; }
    public ShelterType Right { get => _shelterInfoRight; set => _shelterInfoRight = value; }
    public ShelterType Bottom { get => _shelterInfoBottom; set => _shelterInfoBottom = value; }
    public ShelterType Front { get => _shelterInfoFront; set => _shelterInfoFront = value; }

    public void SetForEvery(ShelterType shelterType)
    {
        _shelterInfoBottom = shelterType;
        _shelterInfoFront = shelterType;
        _shelterInfoLeft = shelterType;
        _shelterInfoRight = shelterType;
    }
}

public enum ShelterType
{
    Nope,
    Semi,
    Full
}

public enum TerritoryType
{
    Undefined,
    Ground,
    Air,
    Shelter,
    ShelterGround,
    Boarder,
    Enemy,
    Decor,
    MapObject,//didnt work

}
