using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

[System.Serializable]
public class TerritroyReaded
{
    public string Index { get; set; }
    public string PathPrefab { get; set; }
    public TerritoryType TerritoryInfo { get; set; }
    public int CountHp { get; set; }

    [SerializeField]
    public ShelterInfo ShelterType { get; set; }

    public float XSize, YSize, ZSize;
    public float XPosition, YPosition, ZPosition;
    public float XRotation, YRotation, ZRotation, WRotation;

    public HashSet<string> IndexLeft = new HashSet<string>(), IndexRight = new HashSet<string>(), IndexUp = new HashSet<string>(),
        IndexDown = new HashSet<string>(), IndexFront = new HashSet<string>(), IndexBottom = new HashSet<string>();

    public Dictionary<string, TerritroyReaded> TerritoriesInside = new Dictionary<string, TerritroyReaded>();

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

    public Dictionary<string, TerritroyReaded> MakeNewBranches()
    {
        List<HashSet<string>> allSides = new List<HashSet<string>>(){ IndexLeft, IndexRight, IndexUp, IndexDown, IndexBottom, IndexFront };
        foreach(TerritroyReaded item in TerritoriesInside.Values)
        {
            //Left
            if (IndexLeft.Count() > 0)
            {
                var leftItem = GameManagerMap.Instance.Map[IndexLeft.OrderBy(n => Vector3.Distance(GameManagerMap.Instance.Map[n].GetCordinats(), item.GetCordinats())).First()];
                if (Vector3.Distance(item.GetCordinats(), leftItem.GetCordinats()) > 1)
                {
                    leftItem = TerritoriesInside[MatrixMap.MakeFromVector3ToIndex(item.GetCordinats() + new Vector3(0, 0, -1))];
                }
                item.IndexLeft.Add(leftItem.Index);
                leftItem.IndexRight.Remove(Index);
                leftItem.IndexRight.Add(item.Index);
                Debug.Log(leftItem + " " + leftItem.IndexRight.First());
            }
            //right
            if (IndexRight.Count() > 0)
            {
                var rightItem = GameManagerMap.Instance.Map[IndexRight.OrderBy(n => Vector3.Distance(GameManagerMap.Instance.Map[n].GetCordinats(), item.GetCordinats())).First()];
                if (Vector3.Distance(item.GetCordinats(), rightItem.GetCordinats()) > 1)
                {
                    rightItem = TerritoriesInside[MatrixMap.MakeFromVector3ToIndex(item.GetCordinats() + new Vector3(0, 0, 1))];
                }
                item.IndexRight.Add(rightItem.Index);
                rightItem.IndexLeft.Remove(Index);
                rightItem.IndexLeft.Add(item.Index);
                Debug.Log(rightItem + " " + rightItem.IndexLeft.First());
            }
            //bottom
            var bottomItem = GameManagerMap.Instance.Map[IndexBottom.OrderBy(n => Vector3.Distance(GameManagerMap.Instance.Map[n].GetCordinats(), item.GetCordinats())).First()];
            if (Vector3.Distance(item.GetCordinats(), bottomItem.GetCordinats()) > 1)
            {
                bottomItem = TerritoriesInside[MatrixMap.MakeFromVector3ToIndex(item.GetCordinats() + new Vector3(-1, 0, 0))];
            }
            item.IndexBottom.Add(bottomItem.Index);
            bottomItem.IndexFront.Remove(Index);
            bottomItem.IndexFront.Add(item.Index);


            //front
            var frontItem = GameManagerMap.Instance.Map[IndexFront.OrderBy(n => Vector3.Distance(GameManagerMap.Instance.Map[n].GetCordinats(), item.GetCordinats())).First()];
            if (Vector3.Distance(item.GetCordinats(), frontItem.GetCordinats()) > 1)
            {
                frontItem = TerritoriesInside[MatrixMap.MakeFromVector3ToIndex(item.GetCordinats() + new Vector3(1, 0, 0))];
            }
            item.IndexFront.Add(frontItem.Index);
            frontItem.IndexBottom.Remove(Index);
            frontItem.IndexBottom.Add(item.Index);

            //down
           /* var downItem = GameManagerMap.Instance.Map[IndexDown.OrderBy(n => Vector3.Distance(GameManagerMap.Instance.Map[n].GetCordinats(), item.GetCordinats())).First()];
            if (Vector3.Distance(item.GetCordinats(), downItem.GetCordinats()) > 1 && )
            {
                downItem = TerritoriesInside[MatrixMap.MakeFromVector3ToIndex(item.GetCordinats() + new Vector3(0, -1, 0))];
            }
            item.IndexDown.Add(downItem.Index);
            downItem.IndexUp.Remove(Index);
            downItem.IndexUp.Add(item.Index);
           */
            //up
            var upItem = GameManagerMap.Instance.Map[IndexUp.OrderBy(n => Vector3.Distance(GameManagerMap.Instance.Map[n].GetCordinats(), item.GetCordinats())).First()];
            if (Vector3.Distance(item.GetCordinats(), upItem.GetCordinats()) > 1)
            {
                upItem = TerritoriesInside[MatrixMap.MakeFromVector3ToIndex(item.GetCordinats() + new Vector3(0, 1, 0))];
            }
            item.IndexUp.Add(upItem.Index);
            upItem.IndexDown.Remove(Index);
            upItem.IndexDown.Add(item.Index);
        }
        return TerritoriesInside;
    }
 
    public bool IsNearIsGround() => DetectSomeBooleans(n => (GameManagerMap.Instance.Map[n].TerritoryInfo == TerritoryType.Ground || GameManagerMap.Instance.Map[n].TerritoryInfo == TerritoryType.ShelterGround)
                                           && GameManagerMap.Instance.Map[GameManagerMap.Instance.Map[n].IndexUp.OrderBy(i => Vector3.Distance(TerritroyReaded.MakeVectorFromIndex(i), TerritroyReaded.MakeVectorFromIndex(n))).First()].TerritoryInfo == TerritoryType.Air,
                                    1, true, IndexBottom, IndexFront, IndexRight, IndexLeft) ||
                                    DetectSomeBooleans(n => GameManagerMap.Instance.Map[n].TerritoryInfo == TerritoryType.ShelterGround, 1, true, IndexDown);
    public bool InACenterOfGronds() => DetectSomeBooleans(n => GameManagerMap.Instance.Map[n].TerritoryInfo == TerritoryType.Air
                                                            && GameManagerMap.Instance.Map[n].IndexDown.Where(n => GameManagerMap.Instance.Map[n].TerritoryInfo == TerritoryType.ShelterGround).Count() == 1,
                                    1, false, IndexBottom, IndexFront, IndexRight, IndexLeft);

    public bool IsNearShelter() => DetectSomeBooleans(n => GameManagerMap.Instance.Map[n].TerritoryInfo == TerritoryType.Shelter
        || GameManagerMap.Instance.Map[n].TerritoryInfo == TerritoryType.ShelterGround, 1, true, IndexBottom, IndexFront, IndexRight, IndexLeft); //mb is slow???

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


    public bool HasGround() => this.IndexDown.Where(n => GameManagerMap.Instance.Map[n].TerritoryInfo == TerritoryType.Ground ||
                GameManagerMap.Instance.Map[n].TerritoryInfo == TerritoryType.ShelterGround).Count() > 0;

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
public class CharactersData
{
    public CharacterData[] characters;
}