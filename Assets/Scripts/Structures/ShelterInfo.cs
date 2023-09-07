using System.Collections.Generic;
using UnityEngine;

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

    public ShelterInfo(ShelterType type)
    {
        SetForEvery(type);
    }

    public void SetForEvery(ShelterType type)
    {
        _shelterInfoBottom = type;
        _shelterInfoFront = type;
        _shelterInfoLeft = type;
        _shelterInfoRight = type;
    }

    public static readonly ShelterInfo EMPTY = new(ShelterType.None);

    public Dictionary<ShelterSide, ShelterType> ToDictionary()
    {
        return new() {
            { ShelterSide.Left, _shelterInfoLeft },
            { ShelterSide.Right, _shelterInfoRight },
            { ShelterSide.Front, _shelterInfoFront },
            { ShelterSide.Back, _shelterInfoBottom }
        };
    }
}

public enum ShelterSide
{
    Left = 0, Right = 1, Front = 2, Back = 3
}

public enum ShelterType
{
    None,
    Semi,
    Full
}