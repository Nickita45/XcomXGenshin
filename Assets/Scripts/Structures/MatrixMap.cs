using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MatrixMap
{
    public Dictionary<string, TerritroyReaded> _vertex = new Dictionary<string, TerritroyReaded>();
    public Dictionary<string, TerritroyReaded> _decors = new Dictionary<string, TerritroyReaded>();
    public Dictionary<string, GameObject> _planeToMovement = new Dictionary<string, GameObject>();
    public List<GameObject> _enemy = new List<GameObject>();
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
        GameObject obj;
        _planeToMovement.TryGetValue(ter.Index, out obj);
        return obj;
    }

    public List<GameObject> Enemy => _enemy;
    public Dictionary<string, TerritroyReaded> Decors => _decors;
    public Dictionary<string, TerritroyReaded> Vertex => _vertex;

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
        foreach(var item in _vertex)
        {
            Debug.Log($"K: {item.Key} + V: l = {string.Join(',',item.Value.IndexLeft)}, r = {string.Join(',', item.Value.IndexRight)}, " +
                $"b = {string.Join(',', item.Value.IndexBottom)}, f = {string.Join(',', item.Value.IndexFront)} " +
                $"d = {string.Join(',', item.Value.IndexDown)}, u = {string.Join(',', item.Value.IndexUp)}");
        }
        Debug.Log("--------------------------------");
    }

    public IEnumerator<TerritroyReaded> GetEnumerator()
    {
        foreach(var item in _vertex)
        {
            yield return item.Value;
        }
        foreach (var item in _decors)
        {
            yield return item.Value;
        }
    }

    public static string MakeFromVector3ToIndex(Vector3 vector) => (vector.x + ReadingMap.SPLITTER + vector.y + ReadingMap.SPLITTER + vector.z);

}
