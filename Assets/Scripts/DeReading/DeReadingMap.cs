using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.VisualScripting;

public class DeReadingMap : MonoBehaviour
{
    [Header("Sctipt Settings")]
    [SerializeField]
    private GameObject _mainObject;  //

    [SerializeField]
    private string _path;
    [SerializeField]
    private string _path2;
    [SerializeField]
    private bool path2;

    public void DeSerelizete(string nameFile)
    {
        TextAsset json = Resources.Load<TextAsset>(nameFile); //get json file
        string filePath = json.ToString(); //get path of this file

        MatrixMap _matrixMap = JsonConvert.DeserializeObject<MatrixMap>(filePath); //deserialization json file

        foreach (var item in _matrixMap)
        {
            if (item.TerritoryInfo == TerritoryType.Air || item.TerritoryInfo == TerritoryType.Undefined || item.TerritoryInfo == TerritoryType.Enemy)
            { //if this is air or enemy, create block of air
                var obj = Manager.Instance.CreatePlatformMovement(item); //create place for moving
                _matrixMap.AddAirPlane(item, obj);

            }


            if (item.TerritoryInfo == TerritoryType.Air)
                continue;

            var (baseObject, additionalObject) = CreateMapObject(item); //create block as real object

            if (item.TerritoryInfo == TerritoryType.Decor)
            {
                baseObject.GetComponent<BoxCollider>().enabled = false;
            }

            if (item.TerritoryInfo == TerritoryType.Enemy)
                _matrixMap.Enemies.Add(additionalObject.GetComponent<Enemy>());

            if (item.TerritoryInfo == TerritoryType.Undefined) //Characters now
                _matrixMap.Characters.Add(baseObject.GetComponent<Character>());
        }

        Manager.Map = _matrixMap;
        Manager.CameraManager.EnableFreeCameraMovement();
    }

    public (GameObject, GameObject) CreateMapObject(TerritroyReaded item)
    {
        string pathPrefabBase = item.PathPrefabBase;
        if (pathPrefabBase == "RANDOM_ENEMY")
            pathPrefabBase = HubData.Instance.GetRandomEnemyPath();

        GameObject prefab = Resources.Load<GameObject>(pathPrefabBase);
        var obj = Instantiate(prefab, _mainObject.transform);
        obj.transform.localPosition = new Vector3(item.XPosition, item.YPosition, item.ZPosition);
        obj.transform.localRotation = new Quaternion(item.XRotation, item.YRotation, item.ZRotation, item.WRotation);
        obj.transform.localScale = new Vector3(item.XSize, item.YSize, item.ZSize);

        TerritoryInfo territoryInfo = baseObject.GetComponent<TerritoryInfo>() ?? baseObject.GetComponentInChildren<TerritoryInfo>();
        territoryInfo.Type = item.TerritoryInfo;
        territoryInfo.ShelterType = item.ShelterType;

        GameObject additionalObject = null;

        // Spawn an additional prefab for enemies
        if (territoryInfo.Type == TerritoryType.Enemy)
        {
            GameObject additionalPrefab = Resources.Load<GameObject>(item.PathPrefabAdditional);

            if (additionalPrefab != null)
            {
                additionalObject = Instantiate(additionalPrefab, baseObject.transform);

                // Connect canvas to enemy
                additionalObject.GetComponent<Enemy>().SetCanvas(baseObject.GetComponent<EnemyCanvas>());

                // Add outline
                GameObject model = additionalObject.transform.GetChild(0).gameObject;
                UnitOutline outline = model.AddComponent<UnitOutline>();
                outline.SetOutlineColor(OutlineColor.Enemy);

                // Add animation
                EnemyAnimator animator = model.AddComponent<EnemyAnimator>();
                animator.SetOutline(outline);
                animator.SetAnimator(model.transform.GetChild(0).GetComponent<Animator>());
                additionalObject.GetComponent<Enemy>().SetAnimator(animator);

                // TODO: clean this up
            }
            else
            {
                Debug.LogError("Unknown Enemy: " + item.PathPrefabAdditional);
            }
        }

        return (baseObject, additionalObject);
    }

}


