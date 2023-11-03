using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DeReadingMap : MonoBehaviour
{


    [Header("Sctipt Settings")]
    [SerializeField]
    private GameObject _mainObject;

    [SerializeField]
    private string _path;
    [SerializeField]
    private string _path2;
    [SerializeField]
    private bool path2;

    public void DeSerelizete(string nameFile)
    {
        TextAsset json = Resources.Load<TextAsset>(nameFile);
        string filePath = json.ToString();

        MatrixMap _matrixMap = JsonConvert.DeserializeObject<MatrixMap>(filePath);

        foreach (var item in _matrixMap)
        {
            if (item.TerritoryInfo == TerritoryType.Air || item.TerritoryInfo == TerritoryType.Undefined || item.TerritoryInfo == TerritoryType.Enemy)
            {
                var obj = Manager.Instance.CreatePlatformMovement(item);
                _matrixMap.AddAirPlane(item, obj);

            }
            //make switch?
            if (item.TerritoryInfo == TerritoryType.Air)
                continue;

            var objMap = CreateMapObject(item);

            if (item.TerritoryInfo == TerritoryType.Decor)
                objMap.GetComponent<BoxCollider>().enabled = false;

            if (item.TerritoryInfo == TerritoryType.Enemy)
                _matrixMap.Enemies.Add(objMap.GetComponent<Enemy>());

            if (item.TerritoryInfo == TerritoryType.Undefined) //Characters now
                _matrixMap.Characters.Add(objMap.GetComponent<Character>());
        }

        Manager.Map = _matrixMap;
        Manager.CameraManager.EnableFreeCameraMovement();
    }

    public GameObject CreateMapObject(TerritroyReaded item)
    {
        GameObject prefab = Resources.Load<GameObject>(item.PathPrefab);
        var obj = Instantiate(prefab, _mainObject.transform);
        obj.transform.localPosition = new Vector3(item.XPosition, item.YPosition, item.ZPosition);
        obj.transform.localRotation = new Quaternion(item.XRotation, item.YRotation, item.ZRotation, item.WRotation);
        obj.transform.localScale = new Vector3(item.XSize, item.YSize, item.ZSize);

        TerritoryInfo territoryInfo = obj.GetComponent<TerritoryInfo>();
        if (territoryInfo == null)
        {
            territoryInfo = obj.GetComponentInChildren<TerritoryInfo>();
        }

        territoryInfo.Type = item.TerritoryInfo;
        territoryInfo.ShelterType = item.ShelterType;

        return obj;
    }

}


