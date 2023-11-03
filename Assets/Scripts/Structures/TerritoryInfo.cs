using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    private int _countHp;

    [SerializeField]
    private ShelterType _setOnStartToEveryone;//exept "Nope"
    public string Path => _pathPrefab;
    public TerritoryType Type { get => _type; set => _type = value; }
    public ShelterInfo ShelterType { get => _shelterInfo; set => _shelterInfo = value; }
    public int SetHp { get => _countHp; set => _countHp = value; }

    private Dictionary<Sides, (HashSet<string>, Vector3 vector)> _sideAndCordinates;
    private void Start()
    {
        TerritroyReaded territory = GameManagerMap.Instance.Map[gameObject.transform.localPosition];
        _sideAndCordinates = new Dictionary<Sides, (HashSet<string>, Vector3 vector)> {
          { Sides.Left, (territory.IndexLeft, new Vector3(0, 0, -1)) },
          { Sides.Right, (territory.IndexRight, new Vector3(0, 0, 1)) },
          { Sides.Bottom , (territory.IndexBottom, new Vector3(-1, 0, 0)) },
          { Sides.Front , (territory.IndexFront, new Vector3(1, 0, 0)) },
          { Sides.Down , (territory.IndexDown, new Vector3(0, -1, 0)) },
          { Sides.Up , (territory.IndexUp, new Vector3(0, 1, 0)) },
        };

        SetHp = 10;
        if (_setOnStartToEveryone == global::ShelterType.Full || _setOnStartToEveryone == global::ShelterType.Semi)
        {
            _shelterInfo.SetForEvery(_setOnStartToEveryone);
        }
    }

    public void DestroyIt()
    {
        if (_type == TerritoryType.Enemy || _type == TerritoryType.Ground)
            return;

        if (name != "NoParent")
            StartCoroutine(ReleaseSpace(GameManagerMap.Instance.Map[transform.localPosition]));
        else
            transform.parent.gameObject.SetActive(false);
    }

    public void MakeDmg(int count)
    {
        if (_countHp == 0)
            return;

        _countHp -= count;
        if (_countHp <= 0)
            DestroyIt();
    }

    private (HashSet<string> itemSide, HashSet<string> findedSide) ReturnerSide(Sides side, TerritroyReaded item, TerritroyReaded finded) //vector for side, can be changed
    { 
        switch(side)
        {
            case Sides.Left:
                return (item.IndexLeft, finded.IndexRight);
            case Sides.Right:
                return (item.IndexRight, finded.IndexLeft);
            case Sides.Front:
                return (item.IndexFront, finded.IndexBottom);
            case Sides.Bottom:
                return (item.IndexBottom, finded.IndexFront);
            case Sides.Up:
                return (item.IndexUp, finded.IndexDown);
            case Sides.Down:
                return (item.IndexDown, finded.IndexUp);
            default:
                return (null, null);
        }
        
    }

    private IEnumerator ReleaseSpaceForOneSide((Sides side, (HashSet<string> sideHashSet, Vector3 vector) key) itemSide, TerritroyReaded item, TerritroyReaded territory)
    {
        if (itemSide.key.sideHashSet.Count > 0)
        {
            GameObject detecter = Instantiate(GameManagerMap.Instance.PrefabToDetectTerritories, GameManagerMap.Instance.MainParent.transform);
            detecter.transform.localPosition = item.GetCordinats();
            GameObject lastFindedBlock = null;
            detecter.GetComponent<DetectBlock>().OnDetectItem += (detectedItem) =>
            {
                lastFindedBlock = detectedItem;
            };
            detecter.transform.localPosition = item.GetCordinats() + itemSide.key.vector;

            yield return new WaitForSeconds(0.2f);

            TerritroyReaded findedTerritroy = (lastFindedBlock == null || lastFindedBlock == gameObject) ?
                GameManagerMap.Instance.Map[detecter.transform.localPosition] :
                GameManagerMap.Instance.Map[lastFindedBlock.transform.localPosition];

            var resultReturner = ReturnerSide(itemSide.side, item, findedTerritroy);
            resultReturner.itemSide.Add(findedTerritroy.Index); 
            resultReturner.findedSide.Remove(territory.Index);
            resultReturner.findedSide.Add(item.Index);
            Destroy(detecter);
        }
    }

    public IEnumerator ReleaseSpace(TerritroyReaded territory)
    {
        GameManagerMap.Instance.Map._vertex.Remove(territory.Index);
        GameManagerMap.Instance.Map._vertex.AddRange(territory.TerritoriesInside);
      
        foreach (var item in territory.TerritoriesInside)
        {
            var obj = GameManagerMap.Instance.CreatePlatformMovement(item.Value);
            GameManagerMap.Instance.Map.AddAirPlane(item.Value, obj);
            foreach(var itemSide in _sideAndCordinates)
            {
                StartCoroutine(ReleaseSpaceForOneSide(itemSide: (itemSide.Key, itemSide.Value), item: item.Value, territory: territory));
            }
        }
        yield return new WaitForSeconds(0.2f);
        gameObject.SetActive(false);

    }
}

enum Sides
{
    Left, Right, Front, Bottom, Down, Up 
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