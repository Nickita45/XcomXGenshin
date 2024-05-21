using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[System.Serializable]
public class TerritroyReaded
{
    public string Index { get; set; }
    public string PathPrefabBase { get; set; }
    public string PathPrefabAdditional { get; set; }
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
        SetNewPosition(transform);

        Index = XPosition + ReadingMap.SPLITTER + YPosition + ReadingMap.SPLITTER + ZPosition;
    }

    public void SetNewPosition(Transform transform)
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

    public TerritroyReaded GetRandomTerritoryNearBy()
    {
        var allItems = new List<string>();

        allItems.AddRange(IndexBottom.Where(n => Manager.Map[n].TerritoryInfo == TerritoryType.Air));
        allItems.AddRange(IndexFront.Where(n => Manager.Map[n].TerritoryInfo == TerritoryType.Air));
        allItems.AddRange(IndexLeft.Where(n => Manager.Map[n].TerritoryInfo == TerritoryType.Air));
        allItems.AddRange(IndexRight.Where(n => Manager.Map[n].TerritoryInfo == TerritoryType.Air));

        int randomIndex = UnityEngine.Random.Range(0,allItems.Count);

        return Manager.Map[allItems[randomIndex]];
    }

    public bool IsNearIsGround() => DetectSomeBooleans(n => (Manager.Map[n].TerritoryInfo == TerritoryType.Ground || Manager.Map[n].TerritoryInfo == TerritoryType.ShelterGround)
                                           && Manager.Map[Manager.Map[n].IndexUp.OrderBy(i => Vector3.Distance(TerritroyReaded.MakeVectorFromIndex(i), TerritroyReaded.MakeVectorFromIndex(n))).First()].TerritoryInfo == TerritoryType.Air,
                                    1, true, IndexBottom, IndexFront, IndexRight, IndexLeft) ||
                                    DetectSomeBooleans(n => Manager.Map[n].TerritoryInfo == TerritoryType.ShelterGround, 1, true, IndexDown);
    //IndexBottom.Where(n => Manager.Map[n].TerritoryInfo == TerritoryType.Ground || Manager.Map[n].TerritoryInfo == TerritoryType.ShelterGround).Count() > 0 ||
    //IndexFront.Where(n => Manager.Map[n].TerritoryInfo == TerritoryType.Ground || Manager.Map[n].TerritoryInfo == TerritoryType.ShelterGround).Count() > 0 ||
    //IndexRight.Where(n => Manager.Map[n].TerritoryInfo == TerritoryType.Ground || Manager.Map[n].TerritoryInfo == TerritoryType.ShelterGround).Count() > 0 ||
    //IndexLeft.Where(n => Manager.Map[n].TerritoryInfo == TerritoryType.Ground || Manager.Map[n].TerritoryInfo == TerritoryType.ShelterGround).Count() > 0 ||
    //IndexDown.Where(n => Manager.Map[n].TerritoryInfo == TerritoryType.ShelterGround).Count() > 0;

    public bool InACenterOfGronds() => DetectSomeBooleans(n => Manager.Map[n].TerritoryInfo == TerritoryType.Air
                                                            && Manager.Map[n].IndexDown.Where(n => Manager.Map[n].TerritoryInfo == TerritoryType.ShelterGround).Count() == 1,
                                    1, false, IndexBottom, IndexFront, IndexRight, IndexLeft);


    /*        IndexBottom.Where(n => Manager.Map[n].TerritoryInfo == TerritoryType.Ground || Manager.Map[n].TerritoryInfo == TerritoryType.ShelterGround).Count() == 0 &&
                                           IndexFront.Where(n => Manager.Map[n].TerritoryInfo == TerritoryType.Ground || Manager.Map[n].TerritoryInfo == TerritoryType.ShelterGround).Count() == 0 &&
                                           IndexRight.Where(n => Manager.Map[n].TerritoryInfo == TerritoryType.Ground || Manager.Map[n].TerritoryInfo == TerritoryType.ShelterGround).Count() == 0 &&
                                           IndexLeft.Where(n => Manager.Map[n].TerritoryInfo == TerritoryType.Ground || Manager.Map[n].TerritoryInfo == TerritoryType.ShelterGround).Count() == 0;
    */

    public bool IsNearShelter() => DetectSomeBooleans(n => Manager.Map[n].TerritoryInfo == TerritoryType.Shelter
        || Manager.Map[n].TerritoryInfo == TerritoryType.ShelterGround, 1, true, IndexBottom, IndexFront, IndexRight, IndexLeft); //mb is slow???

    public static bool DetectSomeBooleans(Func<string, bool> predicate, int count, bool IsOr, params HashSet<string>[] sides)
    {
        bool and = true;
        foreach (var item in sides)
        {
            if (item.Where(predicate).Count() == count)
            {
                if (IsOr)
                    return true;
            }
            else
            {
                and = false;
            }
        }
        return and;
    }

    public bool HasGround() => this.IndexDown.Where(n => Manager.Map[n].TerritoryInfo == TerritoryType.Ground ||
                Manager.Map[n].TerritoryInfo == TerritoryType.ShelterGround).Count() > 0;

    public static Vector3 MakeVectorFromIndex(string index) => new Vector3(float.Parse(index.Split(ReadingMap.SPLITTER)[0]),
        float.Parse(index.Split(ReadingMap.SPLITTER)[1]),
        float.Parse(index.Split(ReadingMap.SPLITTER)[2]));

    public override string ToString() => $"({Index})";
}
[System.Serializable]
public class GunTypeConfig
{
    public string name;
    public int distanceValue;
    public int baseValue;
    public int minHitValue;
    public int maxHitValue;
    public int countBullets;
    public float minTimeBetweenShooting;
    public float maxTimeBetweenShooting;
}
[System.Serializable]
public class GlobalDataJson
{
    public int bonusAimFromFullCover;
    public int bonusAimFromHalfCover;
    public int bonusAimFromNoneCover; //hm

    public int bonusAimFromHighGround;
    public int bonusAimFromLowGround;
    public int bonusAimFromNoneGround;

    public float timeAfterShooting;

    public float cameraSpeed;
    public float cameraSpeedRotation;
    public float cameraSpeedZoom;
    public float cameraZoomMin;
    public float cameraZoomMax;

    public float secondsEndTurn;
    public float secondsTimerTurnCharacter;

    public string outlineEnemyColor;
    public string outlineEnemyTargetColor;
    public string outlineCharacterColor;
    public string outlineCharacterTargetColor;
    public float outlineWidth;

    public GunTypeConfig[] typeGun;
}
[System.Serializable]
public class CharacterData
{
    public string characterName;
    public float characterSpeed;
    public int characterMoveDistance;
    public float characterRangedTargetDistance;
    public int characterBaseAim;
    public int characterBaseHealth;
    public int characterWeapon;
    public string characterAvatarPath;
    public string element;
}
[System.Serializable]
public class EnemyData
{
    public string enemyName;
    public string enemyDescription;
    public float enemySpeed;
    public int enemyMoveDistance;
    public float enemyRangedTargetDistance;
    public int enemyBaseAim;
    public int enemyBaseHealth;
    public int enemyWeapon;
    public bool isMeleeAttack;
    public int enemyMinAttackValue;
    public int enemyMaxAttackValue;
}