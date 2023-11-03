using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class MatrixMap
{
    public Dictionary<string, TerritroyReaded> _vertex = new();
    public Dictionary<string, TerritroyReaded> _decors = new();
    public Dictionary<string, GameObject> _planeToMovement = new();
    public List<GameObject> _enemy = new();
    public List<GameObject> _characters = new();
    public int width, height;

    public TerritroyReaded AddVertex(TerritroyReaded ter, Dictionary<string, TerritroyReaded> collection)
    {
        collection.Add(ter.Index, ter);
        return ter;
    }

    public void AddAirPlane(TerritroyReaded ter, GameObject obj)
    {
        _planeToMovement.Add(ter.Index, obj);
    }

    public GameObject GetAirPlatform(TerritroyReaded ter)
    {
        _planeToMovement.TryGetValue(ter.Index, out GameObject obj);
        return obj;
    }

    public TerritroyReaded RemoveFromVertex(Vector3 vector)
    {
        var ter = this[vector];
        _vertex.Remove(ter.Index);
        return ter;
    }

    public TerritroyReaded RemoveFromVertex(TerritroyReaded ter)
    {
        _vertex.Remove(ter.Index);
        return ter;
    }

    public List<GameObject> Enemy => _enemy;
    public List<GameObject> Characters => _characters;
    public void AirPlanfromRemove(TerritroyReaded ter) => _planeToMovement.Remove(ter.Index);

    public TerritroyReaded this[string index] => _vertex[index];
    public TerritroyReaded this[Vector3 cordinats] => _vertex[MakeFromVector3ToIndex(cordinats)];

    public bool ContainsVertexByPox(Vector3 vector, out TerritroyReaded game)
    {
        string index = vector.x + ReadingMap.SPLITTER + vector.y + ReadingMap.SPLITTER + vector.z;

        if (_vertex.ContainsKey(index))
        {
            game = _vertex[index];
            return true;
        }

        game = null;
        return false;
    }

    public void DebugToConsole()
    {
        foreach (var item in _vertex)
        {
            Debug.Log($"K: {item.Key} + V: l = {string.Join(',', item.Value.IndexLeft)}, r = {string.Join(',', item.Value.IndexRight)}, " +
                $"b = {string.Join(',', item.Value.IndexBottom)}, f = {string.Join(',', item.Value.IndexFront)} " +
                $"d = {string.Join(',', item.Value.IndexDown)}, u = {string.Join(',', item.Value.IndexUp)}");
        }
        Debug.Log("--------------------------------");
    }

    public IEnumerator<TerritroyReaded> GetEnumerator()
    {
        foreach (var item in _vertex)
        {
            yield return item.Value;
        }
        foreach (var item in _decors)
        {
            yield return item.Value;
        }
    }

    public void ReleaseSpace(TerritroyReaded territory)
    {
       // _vertex.AddRange(territory.TerritoriesInside);

       // foreach(var item in territory.TerritoriesInside)
      //  {
      //      GameObject detecter = Instantiate(GameManagerMap.Instance.PrefabToDetectTerritories, GameManagerMap.Instance.MainParent);
      //  }


       /* _vertex.Remove(territory.Index);

        foreach (var item in territory.MakeNewBranches())
        {
            _vertex.Add(item.Key, item.Value);
            var obj = GameManagerMap.Instance.CreatePlatformMovement(item.Value);
            AddAirPlane(item.Value, obj);
            Debug.Log(item.Value.IndexRight.First() + " (" + item.Value.IndexRight.Count() + ") "+ " " + GameManagerMap.Instance.Map[item.Value.IndexRight.First()].IndexLeft.First());
            Debug.Log(_planeToMovement[item.Key].transform.localPosition);
        }*/
    }

    public static string MakeFromVector3ToIndex(Vector3 vector) => (vector.x + ReadingMap.SPLITTER + vector.y + ReadingMap.SPLITTER + vector.z);
}
