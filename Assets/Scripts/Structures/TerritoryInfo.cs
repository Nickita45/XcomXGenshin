using UnityEngine;
using UnityEngine.Serialization;

public class TerritoryInfo : MonoBehaviour
{
    [Header("Basic Settings")]
    [SerializeField]
    private TerritoryType _type;

    [FormerlySerializedAs("_pathPrefab")]
    [SerializeField]
    private string _pathPrefabBase;

    // An additional path, used for the prefab of a chosen enemy type
    [SerializeField]
    private string _pathPrefabAdditional;

    [Header("Shelter Settings")]
    [SerializeField]
    private ShelterInfo _shelterInfo;
    [SerializeField]
    private ShelterType _setOnStartToEveryone;//Automatically set all sides of _shelterInfo to this value on the start of scene, but exept is "Nope"
    public string PathBase => _pathPrefabBase;
    public string PathAdditional => _pathPrefabAdditional;
    public TerritoryType Type { get => _type; set => _type = value; }
    public ShelterInfo ShelterType { get => _shelterInfo; set => _shelterInfo = value; }

    private void Start()
    {
        if (_setOnStartToEveryone == global::ShelterType.Full || _setOnStartToEveryone == global::ShelterType.Semi)
        {
            _shelterInfo.SetForEvery(_setOnStartToEveryone); //set all sides of _shelterInfo to value of _setOnStartToEveryone
        }
    }

    public bool CanBeHiddenByCamera()
    {
        return _type == TerritoryType.Shelter;
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