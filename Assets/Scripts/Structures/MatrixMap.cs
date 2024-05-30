using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class MatrixMap
{
    private Dictionary<string, TerritroyReaded> _vertex = new();

    private Dictionary<string, TerritroyReaded> _decors = new();

    //dictionary of platforms for movement cells(actually they are air blocks)
    private Dictionary<string, GameObject> _planeToMovement = new();
    private ListUnitsMap<Enemy> _enemy = new();
    private ListUnitsMap<Character> _characters = new();
    private ListUnitsMap<Entity> _entity = new();
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

    public Dictionary<string, TerritroyReaded> Decors { get { return _decors; } }
    public Dictionary<string, TerritroyReaded> Vertex { get { return _vertex; } }

    public ListUnitsMap<Enemy> Enemies { get => _enemy; private set => _enemy = value; }
    public ListUnitsMap<Character> Characters { get => _characters; private set => _characters = value; }
    public ListUnitsMap<Entity> Entities { get => _entity; private set => _entity = value; }

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
        }
        else //find in decor dictionary
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

    // Get a list of all allies of the unit (including the unit themselves)
    public IEnumerable<Unit> GetAllies(Unit _target)
    {
        if (_target is Character)
            return Characters.GetList.Select(c => (Unit)c);
        else if (_target is Enemy)
            return Enemies.GetList.Select(e => (Unit)e);
        else
            return new List<Unit>();
    }

    //Get a list of all units on the map
    public IEnumerable<Unit> GetAllUnits()
    {
        return Characters.GetList.Select(c => (Unit)c).Concat(Enemies.GetList.Select(e => (Unit)e)).Concat(Entities.GetList.Select(e => (Unit)e));
    }

    // Get a list of allies of the unit within n squares from them (including the unit themselves)
    public IEnumerable<Unit> GetAdjancentAllies(int n, Unit _target)
    {
        Vector3 coordinats = _target.ActualTerritory.GetCordinats();

        return Manager.Map.GetAllies(_target).Where(UnitsAdjancent(n, coordinats));
    }

    private static Func<Unit, bool> UnitsAdjancent(int n, Vector3 coordinats, bool noY = false)
    {
        return unit =>
        {
            Vector3 otherCoordinats = unit.ActualTerritory.GetCordinats();
            if (noY)
                 return Mathf.Abs(coordinats.y - otherCoordinats.y) <= 2 && Math.Round(Vector3.Distance(coordinats, otherCoordinats)) <= n;

            // Find if any are within 1 square from the unit
            return Math.Round(Vector3.Distance(coordinats, otherCoordinats)) <= n;   
              //  Mathf.Abs(coordinats.x - otherCoordinats.x) <= n &&
              //  Mathf.Abs(coordinats.y - otherCoordinats.y) <= n &&
              //  Mathf.Abs(coordinats.z - otherCoordinats.z) <= n;
        };
    }

    // Get a list of units of the unit within n squares from them (including the unit themselves)
    public IEnumerable<Unit> GetAdjancentUnits(int n, Unit _target)
    {
        Vector3 coordinats = _target.ActualTerritory.GetCordinats();

        return Manager.Map.GetAllUnits().Where(UnitsAdjancent(n, coordinats));
    }

    public IEnumerable<Unit> GetAdjancentUnits(int n, TerritroyReaded _target)
    {
        Vector3 coordinats = _target.GetCordinats();

        return Manager.Map.GetAllUnits().Where(UnitsAdjancent(n, coordinats));
    }
}

[System.Serializable]
public class ListUnitsMap<T>
{
    private List<T> _units = new();

    public List<T> GetList => _units.ToList();
    public void Remove(T unit) => _units.Remove(unit);
    public void Add(T unit) => _units.Add(unit);

}