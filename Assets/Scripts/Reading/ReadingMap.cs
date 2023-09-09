using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class ReadingMap : MonoBehaviour
{
    public static readonly string SPLITTER = "_";

    private const float TIMEWAITREADING = 0.02f;//optimal is 0.05f


    [Header("DETECTER")]
    [SerializeField]
    private GameObject _objectDetecterPrefab;
    [SerializeField]
    private Transform _startTransform;
    [SerializeField]
    private string _fileName;

    private GameObject _objectDetect;
    private GameObject _aktualGameObject;
    private Vector3 _startPosition;

    private TerritroyReaded _lastReadedTerritory;

    [Header("Map")]
    [SerializeField]
    private MatrixMap _matrixMap = new();

    private void Start()
    {
        StartCoroutine(AlgoritmusReadingMap());
    }

    private IEnumerator AlgoritmusReadingMap()
    {
        yield return AlgoritmusReadingMapOneWay(wayRow: new Vector3(0, 0, 1), koefRow: (-1, 0, 0), koefColumn: (0, 1, 0), vector: DetecterVector.Horizontal);
        yield return AlgoritmusReadingMapOneWay(wayRow: new Vector3(-1, 0, 0), koefRow: (0, 0, 1), koefColumn: (0, 1, 0), vector: DetecterVector.Vertical);
        yield return AlgoritmusReadingMapOneWay(wayRow: new Vector3(0, 1, 0), koefRow: (0, 0, 1), koefColumn: (-1, 0, 0), vector: DetecterVector.UpDown);

        SavetToJson();
    }

    private IEnumerator AlgoritmusReadingMapOneWay(Vector3 wayRow, (int x, int y, int z) koefRow, (int x, int y, int z) koefColumn, DetecterVector vector)
    {
        _aktualGameObject = null;
        _lastReadedTerritory = null;

        _objectDetect = Instantiate(_objectDetecterPrefab, _startTransform.position, Quaternion.identity);
        _objectDetect.GetComponent<DetectBlock>().OnDetectItem += OnBlockDetect;
        _startPosition = _objectDetect.transform.position;

        int row = 0;
        int column = 0;
        bool isDetectSomething = false;
        yield return new WaitForSeconds(TIMEWAITREADING);
        AddNewTerritory(vector);

        while (true)
        {
            int countHeightOrWeight = 0;
            if (vector != DetecterVector.UpDown)
            {
                while (_aktualGameObject == null ||
                    (_aktualGameObject.GetComponent<TerritoryInfo>() && _aktualGameObject.GetComponent<TerritoryInfo>().Type != TerritoryType.Boarder))
                {
                    countHeightOrWeight++;
                    AlgoritmusRowMove(wayRow, ref isDetectSomething);

                    yield return new WaitForSeconds(TIMEWAITREADING);
                    AddNewTerritory(vector);
                }

                if (countHeightOrWeight > _matrixMap.width)
                    _matrixMap.width = countHeightOrWeight;
            }
            else
            {
                GameObject beforeObj = null;
                int countUp = 0;
                do
                {
                    beforeObj = _aktualGameObject;
                    AlgoritmusRowMove(wayRow, ref isDetectSomething);

                    yield return new WaitForSeconds(TIMEWAITREADING);

                    AddNewTerritory(vector);
                    countUp++;
                } while (countUp != _matrixMap.height);//(beforeObj != null || _aktualGameObject != null);
            }

            row++;

            yield return MoveDetecterByVector(vectorMove: new Vector3(_startPosition.x + koefRow.x * row + koefColumn.x * column,
                 _startPosition.y + koefRow.y * row + koefColumn.y * column,
                 _startPosition.z + koefRow.z * row + koefColumn.z * column), vector: vector);

            if (_aktualGameObject?.GetComponent<TerritoryInfo>() &&
                _aktualGameObject.GetComponent<TerritoryInfo>().Type == TerritoryType.Boarder)
            {
                if (!isDetectSomething)
                {
                    if (vector == DetecterVector.Vertical)
                        _matrixMap.height = column;

                    break;
                }
                isDetectSomething = false;
                column++;
                row = 0;

                yield return MoveDetecterByVector(vectorMove: new Vector3(_startPosition.x + koefRow.x * row + koefColumn.x * column,
                  _startPosition.y + koefRow.y * row + koefColumn.y * column,
                  _startPosition.z + koefRow.z * row + koefColumn.z * column), vector: vector);

                if (_aktualGameObject != null && _aktualGameObject.GetComponent<TerritoryInfo>().Type == TerritoryType.Boarder)
                {
                    if (vector == DetecterVector.Vertical)
                        _matrixMap.height = column;

                    break;
                }
            }
        }
        _objectDetect.GetComponent<BoxCollider>().enabled = false;
    }

    private void AlgoritmusRowMove(Vector3 wayRow, ref bool isDetect)
    {
        _objectDetect.transform.position += wayRow;
        if (_aktualGameObject != null &&
            _aktualGameObject.gameObject.GetComponent<TerritoryInfo>().Type != TerritoryType.Air && _aktualGameObject.gameObject.GetComponent<TerritoryInfo>().Type != TerritoryType.Boarder)
        {
            isDetect = true;
        }
    }

    private IEnumerator MoveDetecterByVector(Vector3 vectorMove, DetecterVector vector)
    {
        _objectDetect.transform.position = vectorMove;
        _lastReadedTerritory = null;

        yield return new WaitForSeconds(TIMEWAITREADING);
        AddNewTerritory(vector);
    }

    private void AddNewTerritory(DetecterVector vector)
    {
        TerritroyReaded newItem;
        if (_aktualGameObject == null)
        {
            if (!_matrixMap.ContainsVertexByPox(_objectDetect.transform.position, out newItem))
            {
                newItem = _matrixMap.AddVertex(new TerritroyReaded(_objectDetect.transform)
                {
                    TerritoryInfo = TerritoryType.Air,
                    ShelterType = ShelterInfo.EMPTY,
                }, _matrixMap.Vertex);
            }
        }
        else
        {
            if (_aktualGameObject.GetComponent<TerritoryInfo>().Type == TerritoryType.Boarder)
            {
                return;
            }
            Transform transforObject = _aktualGameObject.transform;
            if (_aktualGameObject.name == "NoParent")
            {
                transforObject = _aktualGameObject.transform.parent;
            }
            else if (_aktualGameObject.GetComponent<TerritoryInfo>().Type == TerritoryType.Decor)
            {
                transforObject = _objectDetect.transform;
            }

            if (!_matrixMap.ContainsVertexByPox(transforObject.position, out newItem))
            {
                if (_aktualGameObject.GetComponent<TerritoryInfo>().Type == TerritoryType.Decor)
                {
                    newItem = _matrixMap.AddVertex(new TerritroyReaded(transforObject)
                    {
                        TerritoryInfo = TerritoryType.Air,
                        ShelterType = ShelterInfo.EMPTY,
                    }, _matrixMap.Vertex);
                    var decorItem = _matrixMap.AddVertex(new TerritroyReaded(transforObject)
                    {
                        TerritoryInfo = TerritoryType.Decor,
                        PathPrefab = _aktualGameObject.GetComponent<TerritoryInfo>().Path
                    }, _matrixMap.Decors);
                    decorItem.SetNewPosition(_aktualGameObject.transform);
                }
                else
                {
                    newItem = _matrixMap.AddVertex(new TerritroyReaded(transforObject)
                    {
                        TerritoryInfo = _aktualGameObject.GetComponent<TerritoryInfo>().Type,
                        ShelterType = _aktualGameObject.GetComponent<TerritoryInfo>().ShelterType,
                        PathPrefab = _aktualGameObject.GetComponent<TerritoryInfo>().Path
                    }, _matrixMap.Vertex);
                }
            }
        }

        if (_lastReadedTerritory != null && newItem != _lastReadedTerritory)
        {
            switch (vector)
            {
                case DetecterVector.Horizontal:
                    _lastReadedTerritory.IndexRight.Add(newItem.Index);
                    newItem.IndexLeft.Add(_lastReadedTerritory.Index);
                    break;
                case DetecterVector.Vertical:
                    _lastReadedTerritory.IndexBottom.Add(newItem.Index);
                    newItem.IndexFront.Add(_lastReadedTerritory.Index);
                    break;
                case DetecterVector.UpDown:
                    _lastReadedTerritory.IndexUp.Add(newItem.Index);
                    newItem.IndexDown.Add(_lastReadedTerritory.Index);
                    break;
            }
        }

        _lastReadedTerritory = newItem;
    }

    private void OnBlockDetect(GameObject obj)
    {
        _aktualGameObject = obj;
    }

    public void SavetToJson()
    {
        _matrixMap.DebugToConsole();
        Debug.Log(CultureInfo.CurrentCulture.Name);
        string jsonText = Newtonsoft.Json.JsonConvert.SerializeObject(_matrixMap, Newtonsoft.Json.Formatting.Indented);
        Debug.Log(jsonText);

        string filePath = Application.dataPath + "/Resources" + _fileName;
        System.IO.File.WriteAllText(filePath, jsonText);
    }
}

enum DetecterVector
{
    Horizontal,
    Vertical,
    UpDown
}

