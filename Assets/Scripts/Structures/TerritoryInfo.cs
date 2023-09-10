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
    Character,
    MapObject,//didnt work

}