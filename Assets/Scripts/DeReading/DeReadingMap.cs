using Newtonsoft.Json;
using System.Linq;
using UnityEngine;

public class DeReadingMap : MonoBehaviour
{
    [Header("Script Settings")]
    [SerializeField]
    private GameObject _mainObject;

    public void DeSerelizete(string nameFile)
    {
        TextAsset json = Resources.Load<TextAsset>(nameFile); //get json file
        Debug.Log(nameFile);
        string filePath = json.ToString(); //get path of this file

        MatrixMap _matrixMap = JsonConvert.DeserializeObject<MatrixMap>(filePath); //deserialization json file

        foreach (var item in _matrixMap)
        {
            if (item.TerritoryInfo == TerritoryType.Air || item.TerritoryInfo == TerritoryType.Character || item.TerritoryInfo == TerritoryType.Enemy)
            { //if this is air or enemy, create block of air
                var obj = Manager.Instance.CreatePlatformMovement(item); //create place for moving
                _matrixMap.AddAirPlane(item, obj);

            }


            if (item.TerritoryInfo == TerritoryType.Air)
                continue;

            var objMap = CreateMapObject(item); //create block as real object

            if (item.TerritoryInfo == TerritoryType.Decor)
                objMap.GetComponent<BoxCollider>().enabled = false;

            if (item.TerritoryInfo == TerritoryType.Enemy)
                _matrixMap.Enemies.Add(objMap.GetComponent<Enemy>());

            if (item.TerritoryInfo == TerritoryType.Character) //Characters now
                _matrixMap.Characters.Add(objMap.GetComponent<Character>());
        }

        Manager.Map = _matrixMap;
        Manager.CameraManager.EnableFreeCameraMovement();
    }

    public GameObject CreateMapObject(TerritroyReaded item)
    {
        GameObject basePrefab = Resources.Load<GameObject>(item.PathPrefabBase);
        GameObject obj = Instantiate(basePrefab, _mainObject.transform);
        obj.transform.localPosition = new Vector3(item.XPosition, item.YPosition, item.ZPosition);
        obj.transform.localRotation = new Quaternion(item.XRotation, item.YRotation, item.ZRotation, item.WRotation);
        obj.transform.localScale = new Vector3(item.XSize, item.YSize, item.ZSize);

        TerritoryInfo territoryInfo = obj.GetComponent<TerritoryInfo>() ?? obj.GetComponentInChildren<TerritoryInfo>();
        territoryInfo.Type = item.TerritoryInfo;
        territoryInfo.ShelterType = item.ShelterType;

        // Spawn an additional prefab for enemies
        if (territoryInfo.Type == TerritoryType.Enemy)
        {
            string pathPrefabAdditional = item.PathPrefabAdditional;
            if (pathPrefabAdditional == "RANDOM_ENEMY")
                pathPrefabAdditional = HubData.Instance.GetRandomEnemyPath();

            GameObject additionalPrefab = Resources.Load<GameObject>(pathPrefabAdditional);

            if (additionalPrefab != null)
            {
                GameObject additionalObj = Instantiate(additionalPrefab, obj.transform);

                // Connect stats to enemy
                Enemy enemy = obj.GetComponent<Enemy>();
                enemy.SetStats(additionalObj.GetComponent<EnemyStats>());

                // Connect AI
                enemy.SetAI(additionalObj.GetComponent<Enemies.AI.EnemyAI>());

                // Connect animation
                Transform avatar = additionalObj.transform.GetChild(0);
                Animator animator = avatar.GetComponent<Animator>();
                if (!animator) animator = avatar.gameObject.AddComponent<Animator>();
                enemy.SetAnimator(animator);
                enemy.SetBulletSpawner(ObjectUtils.FindDescendantByName(additionalObj.transform, "BulletSpawner"));
            }
            else
            {
                Debug.LogError("Unknown Enemy: " + item.PathPrefabAdditional);
            }
        }

        if (territoryInfo.Type == TerritoryType.Ground)
        {
            foreach (Transform mount in obj.transform)
            {
                if (mount.tag == "Mountains")
                    mount.gameObject.SetActive(true);
            } 
        }

        return obj;
    }

}


