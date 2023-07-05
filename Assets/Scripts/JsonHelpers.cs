using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class TerritroyReaded
{
    public string Index { get; set; }
    public string PathPrefab { get; set; }
    public TerritoryType TerritoryInfo { get; set; }
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

    public static bool DetectSampleShelters(TerritroyReaded first, TerritroyReaded second)
    {
        HashSet<TerritroyReaded> allSheltersFirst = new HashSet<TerritroyReaded>();
        
        foreach (var item in first)
        {
            TerritroyReaded itemTerritory = GameManagerMap.Instance.Map[item];
            if (itemTerritory.TerritoryInfo == TerritoryType.Shelter)
            {
                allSheltersFirst.Add(itemTerritory);
            }
        }

        foreach (var item in second)
        {
            TerritroyReaded itemTerritory = GameManagerMap.Instance.Map[item];
            if (itemTerritory.TerritoryInfo == TerritoryType.Shelter && allSheltersFirst.Contains(itemTerritory))
            {
                return true;
            }
        }
        return false;
    }

    public override string ToString() => $"({Index})";
}
