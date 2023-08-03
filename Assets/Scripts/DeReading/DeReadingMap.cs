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
    [SerializeField]
    private string _path2;
    [SerializeField]
    private bool path2;

    private void Start()
    {
        if (path2 == true)
            _path = _path2;
        
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
            CreateMapObject(item);
        }

        foreach (var item in _matrixMap._decors)
        {
            CreateMapObject(item.Value);
        }

        GameManagerMap.Instance.Map = _matrixMap;
    }

    public void CreateMapObject(TerritroyReaded item)
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

        if(item.TerritoryInfo == TerritoryType.Decor)
            obj.GetComponent<BoxCollider>().enabled = false;

    }

}


