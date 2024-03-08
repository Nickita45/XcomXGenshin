using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class ReadingMap : MonoBehaviour
{
    public static readonly string SPLITTER = "_"; //The symbol that will divide the values in the file

    private const float TIMEWAITREADING = 0.02f;//Optimal timer


    [Header("DETECTER")]
    [SerializeField]
    private GameObject _objectDetecterPrefab;//Prefab
    [SerializeField]
    private Transform _startTransform;//Start position for block detecter
    [SerializeField]
    private string _fileName;//Future file name

    private GameObject _objectDetect; //Actual block detecter
    private GameObject _aktualGameObject; //An actual shelter that is reading
    private Vector3 _startPosition; //Position of the block before moving

    private TerritroyReaded _lastReadedTerritory; //The previous block, before the new reading

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
        _aktualGameObject = null; //initialisation  
        _lastReadedTerritory = null;

        _objectDetect = Instantiate(_objectDetecterPrefab, _startTransform.position, Quaternion.identity); // Create detecter block
        _objectDetect.GetComponent<DetectBlock>().OnDetectItem += OnBlockDetect;
        _startPosition = _objectDetect.transform.position;

        int row = 0; // row counter
        int column = 0; //column counter
        bool isDetectSomething = false;
        yield return new WaitForSeconds(TIMEWAITREADING);
        AddNewTerritory(vector); //added first territory

        while (true) //infinity cyklus
        {
            int countHeightOrWeight = 0; //count columns
            if (vector != DetecterVector.UpDown) //algoritmus 
            {
                while (_aktualGameObject == null ||
                    (_aktualGameObject.GetComponent<TerritoryInfo>() && _aktualGameObject.GetComponent<TerritoryInfo>().Type != TerritoryType.Boarder))
                {
                    countHeightOrWeight++;
                    AlgoritmusRowMove(wayRow, ref isDetectSomething); // move as in row

                    yield return new WaitForFixedUpdate();
                    AddNewTerritory(vector); //create new territory
                }

                if (countHeightOrWeight > _matrixMap.width)
                    _matrixMap.width = countHeightOrWeight;
            }
            else
            {
                GameObject beforeObj = null;
                int countUp = 0; //memorise the number of times we climb to the top
                do
                {
                    beforeObj = _aktualGameObject;
                    AlgoritmusRowMove(wayRow, ref isDetectSomething); // move as in row for up/down movement

                    yield return new WaitForFixedUpdate();

                    AddNewTerritory(vector);
                    countUp++;
                } while (countUp != _matrixMap.height); //until the detector block rises to maximum.
            }

            row++;

            yield return MoveDetecterByVector(vectorMove: new Vector3(_startPosition.x + koefRow.x * row + koefColumn.x * column,
                 _startPosition.y + koefRow.y * row + koefColumn.y * column,
                 _startPosition.z + koefRow.z * row + koefColumn.z * column), vector: vector); //moving in new row, creating a new block and/or adding connections

            if (_aktualGameObject?.GetComponent<TerritoryInfo>() &&
                _aktualGameObject.GetComponent<TerritoryInfo>().Type == TerritoryType.Boarder) //detecting on colliding with boarders
            {
                if (!isDetectSomething)
                {
                    if (vector == DetecterVector.Vertical)
                        _matrixMap.height = column;

                    break;
                }
                isDetectSomething = false; //is used to end the algorithm
                column++;
                row = 0;

                yield return MoveDetecterByVector(vectorMove: new Vector3(_startPosition.x + koefRow.x * row + koefColumn.x * column,
                  _startPosition.y + koefRow.y * row + koefColumn.y * column,
                  _startPosition.z + koefRow.z * row + koefColumn.z * column), vector: vector); //moving in new column, creating a new block and/or adding connections

                if (_aktualGameObject != null && _aktualGameObject.GetComponent<TerritoryInfo>().Type == TerritoryType.Boarder)// if true, algoritmus is end
                {
                    if (vector == DetecterVector.Vertical)
                        _matrixMap.height = column;

                    break;
                }
            }
        }
        _objectDetect.GetComponent<BoxCollider>().enabled = false; //disabling for avoidances errors
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

        yield return new WaitForFixedUpdate();
        AddNewTerritory(vector);
    }

    private void AddNewTerritory(DetecterVector vector)
    {
        TerritroyReaded newItem; //future terrioty block
        if (_aktualGameObject == null) // if _aktualGameObject == null it seams that we on the block of AIR
        {
            if (!_matrixMap.ContainsVertexByPos(_objectDetect.transform.position, out newItem)) //creating if not exists
            {
                newItem = _matrixMap.AddVertex(new TerritroyReaded(_objectDetect.transform) // creating AIR
                {
                    TerritoryInfo = TerritoryType.Air,
                    ShelterType = ShelterInfo.EMPTY,
                }, _matrixMap.Vertex);
            }
        }
        else
        {
            if (_aktualGameObject.GetComponent<TerritoryInfo>().Type == TerritoryType.Boarder) //if it is boarder
            {
                return;
            }
            Transform transforObject = _aktualGameObject.transform;
            if (_aktualGameObject.name == "NoParent")
            {
                transforObject = _aktualGameObject.transform.parent;
            }
            else if (_aktualGameObject.GetComponent<TerritoryInfo>().Type == TerritoryType.Decor) //if the block is a decoration, we create it on the basis of our block detecter
            {
                transforObject = _objectDetect.transform;
            }

            if (!_matrixMap.ContainsVertexByPos(transforObject.position, out newItem)) //if such block of territory will not already added
            {
                if (_aktualGameObject.GetComponent<TerritoryInfo>().Type == TerritoryType.Decor) //creating for decor type
                {
                    newItem = _matrixMap.AddVertex(new TerritroyReaded(transforObject) //creating block of air on this decor territory
                    {
                        TerritoryInfo = TerritoryType.Air,
                        ShelterType = ShelterInfo.EMPTY,
                    }, _matrixMap.Vertex);

                    if (!_matrixMap.ContainsVertexByPos(_aktualGameObject.transform.position, out _, true))
                    {
                        var decorItem = _matrixMap.AddVertex(new TerritroyReaded(_aktualGameObject.transform) //create block decor in other dictionary
                        {
                            TerritoryInfo = TerritoryType.Decor,
                            PathPrefabBase = _aktualGameObject.GetComponent<TerritoryInfo>().PathBase
                        }, _matrixMap.Decors);
                        //decorItem.SetNewPosition(_aktualGameObject.transform);
                    }
                }
                else
                {
                    newItem = _matrixMap.AddVertex(new TerritroyReaded(transforObject) //creating for other type
                    {
                        TerritoryInfo = _aktualGameObject.GetComponent<TerritoryInfo>().Type,
                        ShelterType = _aktualGameObject.GetComponent<TerritoryInfo>().ShelterType,
                        PathPrefabBase = _aktualGameObject.GetComponent<TerritoryInfo>().PathBase,
                        PathPrefabAdditional = _aktualGameObject.GetComponent<TerritoryInfo>().PathAdditional
                    }, _matrixMap.Vertex);
                }
            }
        }

        if (_lastReadedTerritory != null && newItem != _lastReadedTerritory) //if this condition is true it means that we can create connection between _lastReadedTerritory and actual territory 
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

    public void SavetToJson() //save in json
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

