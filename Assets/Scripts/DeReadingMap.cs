using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DeReadingMap : MonoBehaviour
{
    private MatrixMap _matrixMap;

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
        string filePath = Application.dataPath + _path;
        if (!File.Exists(filePath))
        {
            return;
        }

        string jsonContent = File.ReadAllText(filePath);
        string jsonFile = jsonContent.ToString();
        _matrixMap = JsonConvert.DeserializeObject<MatrixMap>(jsonFile);
    
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
        }
        _matrixMap.DebugToConsole();
    }

}
