using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerritroyReaded
{
    public string Index { get; set; }
    public string PathPrefab { get; set; }
    public TerritoryType TerritoryInfo { get; set; }

    [SerializeField]
    public ShelterInfo ShelterType { get; set; }
    
    public float XSize, YSize, ZSize;
    public float XPosition, YPosition, ZPosition;
    public float XRotation, YRotation, ZRotation, WRotation;

    public HashSet<string> IndexLeft = new HashSet<string>(), IndexRight = new HashSet<string>(), IndexUp = new HashSet<string>(), 
        IndexDown = new HashSet<string>(), IndexFront = new HashSet<string>(), IndexBottom = new HashSet<string>();

    public TerritroyReaded() { }

    public TerritroyReaded(Transform transform)
    {
        XSize = transform.localScale.x;
        YSize = transform.localScale.y;
        ZSize = transform.localScale.z;

        XPosition = transform.localPosition.x;
        YPosition = transform.localPosition.y;
        ZPosition = transform.localPosition.z;
        
        XRotation = transform.localRotation.x;
        YRotation = transform.localRotation.y;
        ZRotation = transform.localRotation.z;
        WRotation = transform.localRotation.w;

        Index = XPosition + ReadingMap.SPLITTER + YPosition + ReadingMap.SPLITTER + ZPosition;
    }

    public Vector3 GetCordinats() => new Vector3(XPosition, YPosition, ZPosition);
    
    public IEnumerator<string> GetEnumerator()
    {
        foreach (var item in IndexBottom)
        {
            yield return item;
        }

        foreach (var item in IndexFront)
        {
            yield return item;
        }
        foreach (var item in IndexLeft)
        {
            yield return item;
        }

        foreach (var item in IndexRight)
        {
            yield return item;
        }

    }

    public bool HasGround() => this.IndexDown.Where(n => GameManagerMap.Instance.Map[n].TerritoryInfo == TerritoryType.Ground ||
                GameManagerMap.Instance.Map[n].TerritoryInfo == TerritoryType.ShelterGround).Count() > 0;

    public static Vector3 MakeVectorFromIndex(string index) => new Vector3(float.Parse(index.Split(ReadingMap.SPLITTER)[0]), 
        float.Parse(index.Split(ReadingMap.SPLITTER)[1]), 
        float.Parse(index.Split(ReadingMap.SPLITTER)[2]));

    public override string ToString() => $"({Index})";

  
}
