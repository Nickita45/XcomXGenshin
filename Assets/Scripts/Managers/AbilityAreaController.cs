using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityAreaController : MonoBehaviour
{
    [SerializeField]
    private GameObject _prefabArea;

    [SerializeField]
    private float _width = 0.3f;
    
    [SerializeField]
    private float _yPosition = -0.3f;

    private List<GameObject> _areas = new();

    private void Start()
    {
        Manager.StatusMain.OnStatusChange += OnStatusChange;
    }

    public void AddOrEditAreas(params (Vector3 position, int distance)[] items)
    {
        for (int i = 0; i < items.Length; i++)
        {
            (Vector3 position, int distance) item = items[i];
            GameObject obj = null;
            if (i + 1 > _areas.Count)
                obj = Instantiate(_prefabArea, Manager.MainParent.transform);
            else
                obj = _areas[i];
            
            obj.transform.localPosition = item.position; //get sides from blocks of territory info 
            obj.transform.GetChild(0).localPosition = new Vector3(item.distance, _yPosition, 0); //Front
            obj.transform.GetChild(0).localScale = new Vector3(_width, _width, item.distance * 2 + _width); //Front

            obj.transform.GetChild(1).localPosition = new Vector3(-item.distance, _yPosition, 0); //Bottom
            obj.transform.GetChild(1).localScale = new Vector3(_width, _width, item.distance * 2 + _width); //Bottom

            obj.transform.GetChild(2).localPosition = new Vector3(0, _yPosition, item.distance); //Right
            obj.transform.GetChild(2).localScale = new Vector3(item.distance * 2 + _width, _width, _width); //Right

            obj.transform.GetChild(3).localPosition = new Vector3(0, _yPosition, -item.distance); //Left
            obj.transform.GetChild(3).localScale = new Vector3(item.distance * 2 + _width, _width, _width); //Left
            
            if(i + 1 > _areas.Count)
             _areas.Add(obj);
        }
    }
    private void OnStatusChange(HashSet<Permissions> permissions) => ClearAreas();

    public void ClearAreas()
    {
        foreach (var item in _areas.ToList())
            Destroy(item);

        _areas.Clear();
    }
}
