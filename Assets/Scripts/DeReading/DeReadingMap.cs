using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DeReadingMap : MonoBehaviour
{

    [SerializeField]
    private GameObject _mainObject;

    [SerializeField]
    private string _path;

    private void Start()
    {
        DeSerelizete();
    }


    private void DeSerelizete()
    {
        TextAsset json = Resources.Load<TextAsset>(_path);
        string filePath = json.ToString();

        MatrixMap _matrixMap = JsonConvert.DeserializeObject<MatrixMap>(filePath);
    
        foreach(var item in _matrixMap)
        {
            if(item.TerritoryInfo == TerritoryType.Air)
            {
                continue;
            }
            GameObject prefab = Resources.Load<GameObject>(item.PathPrefab);
            var obj = Instantiate(prefab, _mainObject.transform);
            obj.transform.localPosition = new Vector3(item.XPosition, item.YPosition, item.ZPosition);
            obj.transform.localRotation = new Quaternion(item.XRotation, item.YRotation, item.ZRotation, item.WRotation);
            obj.transform.localScale = new Vector3(item.XSize, item.YSize, item.ZSize);
            obj.GetComponent<TerritoryInfo>().Type = item.TerritoryInfo;
            obj.GetComponent<TerritoryInfo>().ShelterType = item.ShelterType;

            if(item.TerritoryInfo == TerritoryType.MapObject)
            {
                obj.GetComponent<CharacterInfo>().ActualTerritory = item;
            }
        }
        GameManagerMap.Instance.Map = _matrixMap;
    }

}
