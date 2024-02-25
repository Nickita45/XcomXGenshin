using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MatrixMap
{
    private Dictionary<string, TerritroyReaded> _vertex = new();

    private Dictionary<string, TerritroyReaded> _decors = new();

    private Dictionary<string, GameObject> _planeToMovement = new(); //dictionary of platforms for movement cells(actually they are air blocks)
    public List<Enemy> _enemy = new();
    public List<Character> _characters = new();
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

    public List<Enemy> Enemies => _enemy;
    public List<Character> Characters => _characters;
    public Dictionary<string, TerritroyReaded> Decors { get { return _decors; } }
    public Dictionary<string, TerritroyReaded> Vertex { get { return _vertex; } }

    public void AirPlatformRemove(TerritroyReaded ter) => _planeToMovement.Remove(ter.Index);

    public TerritroyReaded this[string index] => _vertex[index];
    public TerritroyReaded this[Vector3 cordinats] => _vertex[MakeFromVector3ToIndex(cordinats)];

    public bool ContainsVertexByPos(Vector3 vector, out TerritroyReaded game, bool isDecor = false) 
    {
        string index = MatrixMap.MakeFromVector3ToIndex(vector); //make index from Vector3

        if (!isDecor) 
        {
            if (_vertex.ContainsKey(index)) //check if such index exists
            {
                game = _vertex[index]; //return true and ref of this block
                return true;
            }
        } else //find in decor dictionary
        {
            if (_decors.ContainsKey(index)) //check if such index exists
            {
                game = _decors[index]; //return true and ref of this block
                return true;
            }
        }

        game = null; //return false and null in ref
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

    public static string MakeFromVector3ToIndex(Vector3 vector) => (vector.x + ReadingMap.SPLITTER + vector.y + ReadingMap.SPLITTER + vector.z);
}
