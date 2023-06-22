using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class MatrixMap 
{
    public Dictionary<string, TerritroyReaded> _vertex = new Dictionary<string, TerritroyReaded>();

    public TerritroyReaded AddVertex(TerritroyReaded ter)
    {
        _vertex.Add(ter.Index,ter);
        return ter;
    }

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
    }
}
