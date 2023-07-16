using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

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

        foreach (var item in IndexDown)
        {
            yield return item;
        }

        if (TerritoryInfo == TerritoryType.ShelterGround)
        {
            foreach (var item in IndexUp)
            {
                yield return item;
            }

        }
    }


    public IEnumerable<string> GetEnumeratorByOne(string index)
    {
        if(IndexBottom.Any())
        yield return IndexBottom.OrderBy(n => Vector3.Distance(TerritroyReaded.MakeVectorFromIndex(index), TerritroyReaded.MakeVectorFromIndex(n))).FirstOrDefault();

        if (IndexFront.Any())
            yield return IndexFront.OrderBy(n => Vector3.Distance(TerritroyReaded.MakeVectorFromIndex(index), TerritroyReaded.MakeVectorFromIndex(n))).FirstOrDefault();

        if (IndexLeft.Any())
            yield return IndexLeft.OrderBy(n => Vector3.Distance(TerritroyReaded.MakeVectorFromIndex(index), TerritroyReaded.MakeVectorFromIndex(n))).FirstOrDefault();
        
        if (IndexRight.Any())
            yield return IndexRight.OrderBy(n => Vector3.Distance(TerritroyReaded.MakeVectorFromIndex(index), TerritroyReaded.MakeVectorFromIndex(n))).FirstOrDefault();

        if (IndexDown.Any())
            yield return IndexDown.OrderBy(n => Vector3.Distance(TerritroyReaded.MakeVectorFromIndex(index), TerritroyReaded.MakeVectorFromIndex(n))).FirstOrDefault();

        if (TerritoryInfo == TerritoryType.ShelterGround && IndexUp.Any())
        {
           yield return IndexUp.OrderBy(n => Vector3.Distance(TerritroyReaded.MakeVectorFromIndex(index), TerritroyReaded.MakeVectorFromIndex(n))).FirstOrDefault();
        }
    }

    public static bool IsNotShelter(TerritroyReaded territory) => territory.TerritoryInfo != TerritoryType.ShelterGround && territory.TerritoryInfo != TerritoryType.Shelter;
    public static Vector3 MakeVectorFromIndex(string index) => new Vector3(float.Parse(index.Split(ReadingMap.SPLITTER)[0]), 
        float.Parse(index.Split(ReadingMap.SPLITTER)[1]), 
        float.Parse(index.Split(ReadingMap.SPLITTER)[2]));

    public override string ToString() => $"({Index})";

  
}
