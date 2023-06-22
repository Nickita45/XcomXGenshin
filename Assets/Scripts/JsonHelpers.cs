using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
