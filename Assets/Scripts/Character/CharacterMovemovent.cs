using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CharacterMovemovent : MonoBehaviour
{
    public static readonly Vector3 BIGVECTOR = new Vector3(int.MaxValue, int.MaxValue, int.MaxValue);

    [Header("Consts Test")]
    [SerializeField]
    private int _countMove = 5;
    [SerializeField]
    private float _speed = 1f;

    [Header("Script Objects")]
    [SerializeField]
    private GameObject _prefabPossibleTerritory;
    [SerializeField]
    private LineRenderer _lineRenderer;

    private CharacterInfo _selectedCharacter;

    private (TerritroyReaded aktualTerritoryReaded, List<Vector3> path) _aktualTerritory;

    private Dictionary<TerritroyReaded, TerritroyReaded> _objectsCalculated; //orig, previous

    public int CountMoveCharacter { get => _countMove; set => _countMove = value; }
    public float SpeedCharacter { get => _speed; set => _speed = value; }

    private void Start()
    {
        _lineRenderer.gameObject.SetActive(false);
    }
    private void Update()
    {
        if (_selectedCharacter != null && _selectedCharacter.MoverActive())
        {
            SpawnMover();
        }


        if (Input.GetKey(KeyCode.K))
        {
            //var test = CalculateAllPossible();
            //Debug.Log(test.Count);
            Debug.Log(_objectsCalculated[GameManagerMap.Instance.Map["-5,5_1,5_-9,5"]]);
            Debug.Log(_objectsCalculated[GameManagerMap.Instance.Map["-6_-0,07_-9,59"]]);
        }
    }

    private void SpawnMover()
    {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        if (hits.Count() > 0)
        {
            var neededHits = hits.Where(n => n.collider.gameObject.tag == "PanelMovement");
            if (neededHits.Count() == 0)
            {
                return;
            }
            var hit = neededHits.First();

            TerritroyReaded detectTerritory;

            detectTerritory = GameManagerMap.Instance.Map[hit.collider.gameObject.transform.localPosition + new Vector3(0, 0.5f, 0)];
            if (_objectsCalculated.ContainsKey(detectTerritory) && TerritroyReaded.IsNotShelter(detectTerritory)
                && detectTerritory.IndexDown.Where(n => GameManagerMap.Instance.Map[n].TerritoryInfo == TerritoryType.Ground ||
                GameManagerMap.Instance.Map[n].TerritoryInfo == TerritoryType.ShelterGround).Count() > 0)
            {
                if (detectTerritory != _aktualTerritory.aktualTerritoryReaded)
                {
                    _aktualTerritory.aktualTerritoryReaded = detectTerritory;

                    _selectedCharacter.SetCordintasToMover(detectTerritory.GetCordinats() + GameManagerMap.Instance.MainParent.transform.position - new Vector3(0, 0.5f, 0));
                    _aktualTerritory.path = calculateAllPath(detectTerritory);//calculatePoints(detectTerritory, _selectedCharacter.transform.localPosition);//
                                                                              //var list = calculatePoints(detectTerritory, _selectedCharacter.transform.localPosition);//calculatePoints(detectTerritory, _selectedCharacter.transform.localPosition);//
                    DrawLine(_aktualTerritory.path);
                    Debug.Log(string.Join(",", _aktualTerritory.path));

                }

                if (Input.GetMouseButtonDown(0) && !_selectedCharacter.isAktualTerritory(_aktualTerritory.aktualTerritoryReaded))
                {
                    StartCoroutine(CoroutineNewPositionCharacter(_aktualTerritory.aktualTerritoryReaded, _aktualTerritory.path));

                }
            }

        }
    }

    public void CharacterSelect(CharacterInfo character)
    {
        _lineRenderer.gameObject.SetActive(true);

        if (_selectedCharacter != null && _selectedCharacter != character)
        {
            _selectedCharacter.OnDeselected();
        }
        _selectedCharacter = character;
        _objectsCalculated = CalculateAllPossible();

        foreach (var item in _objectsCalculated.Keys)
        {
            var obj = Instantiate(_prefabPossibleTerritory, GameManagerMap.Instance.GenereteTerritoryMove.transform);
            obj.transform.localPosition = item.GetCordinats() - new Vector3(0, 0.5f, 0);
        }
    }

    public void CharacterDeselect()
    {
        _lineRenderer.gameObject.SetActive(false);

        _selectedCharacter = null;

        _objectsCalculated.Clear();
        foreach (Transform item in GameManagerMap.Instance.GenereteTerritoryMove.transform)
        {
            Destroy(item.gameObject);
        }
    }


    private IEnumerator CoroutineNewPositionCharacter(TerritroyReaded newTerritory, List<Vector3> points)
    {
        _selectedCharacter.MoverActive(false);

        yield return StartCoroutine(CoroutineMove(points));
        _selectedCharacter.ActualTerritory = newTerritory;

        var save = _selectedCharacter;
        _selectedCharacter.OnDeselected();
        save.OnSelected(save);
    }

    private IEnumerator CoroutineMove(Vector3 target)
    {
        Vector3 startPosition = _selectedCharacter.gameObject.transform.localPosition;

        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            _selectedCharacter.gameObject.transform.localPosition = Vector3.Lerp(startPosition, target, elapsedTime);

            elapsedTime += Time.deltaTime * _speed;

            yield return null;
        }

        _selectedCharacter.gameObject.transform.localPosition = target;
    }

    private IEnumerator CoroutineMove(List<Vector3> targets)
    {
        Vector3 startPosition = _selectedCharacter.gameObject.transform.localPosition;

        float elapsedTime = 0f;
        int index = 0;
        Vector3 target = targets[++index]; //ignore first, becouse its for line

        while (true)
        {
            while (elapsedTime < 1f)
            {
                _selectedCharacter.gameObject.transform.localPosition = Vector3.Lerp(startPosition, target, elapsedTime);

                elapsedTime += Time.deltaTime * _speed;

                yield return null;
            }
            _selectedCharacter.gameObject.transform.localPosition = target;
            startPosition = _selectedCharacter.gameObject.transform.localPosition;
            elapsedTime = 0f;

            if (target == targets[^1]) //last
            {
                break;
            }
            target = targets[++index];
            yield return null;
        }
    }

    private void DrawLine(List<Vector3> points)
    {

        _lineRenderer.positionCount = points.Count;

        for (int i = 0; i < points.Count; i++)
        {
            _lineRenderer.SetPosition(i, points[i] + GameManagerMap.Instance.MainParent.transform.position);
        }
    }


    public List<Vector3> calculateAllPath(TerritroyReaded starter)
    {
        List<Vector3> path = new List<Vector3>
        {
            _selectedCharacter.transform.localPosition,
            starter.GetCordinats()

        };

        while (true)
        {
            List<Vector3> newPath = new List<Vector3>();
            for (int i = 0; i < path.Count - 1; i++)
            {
                var iList = calculatePoints(GameManagerMap.Instance.Map[path[i + 1]], path[i]);
                newPath.AddRange(iList);
            }

            newPath = newPath.Distinct().ToList();
            if (newPath.Count == path.Count)
                break;

            path.Clear();
            foreach (var i in newPath)
            {
                path.Add(i);
            }


        }

        var basicPaths = FindPathBack(starter);
        int indexes = basicPaths.Count + 1;
        Vector3[] finalCordinats = new Vector3[indexes];
        Array.Fill(finalCordinats, BIGVECTOR);

        if (path.Count == 0)
        {
            return path;
        }


        finalCordinats[indexes - 1] = path.First();

        foreach (var item in path)
        {
            finalCordinats[basicPaths[item]] = item;
        }
        var endList = finalCordinats.Where(n => n != BIGVECTOR).Distinct().Reverse().ToList();

        return endList;
    }

    public List<Vector3> calculatePoints(TerritroyReaded starter, Vector3 firstVector)
    {
        var paths = FindPathBack(starter);
        int indexes = paths.Count + 1;

        


        Vector3 targetPosition = starter.GetCordinats() - firstVector;
        RaycastHit[] hits = Physics.RaycastAll(firstVector + GameManagerMap.Instance.MainParent.transform.position, targetPosition, Vector3.Distance(firstVector, starter.GetCordinats()));
        Debug.DrawRay(firstVector + GameManagerMap.Instance.MainParent.transform.position, targetPosition, Color.red);

        Dictionary<Vector3, int> nextPaths = new Dictionary<Vector3, int>();
        Debug.Log(string.Join(",", paths));
        Dictionary<int, Vector3> aboveBlocks = new Dictionary<int, Vector3>();
        foreach (var item in paths)
        {
            var aktualItem = GameManagerMap.Instance.Map[item.Key];
            var beforeItem = _objectsCalculated[aktualItem];

            if (beforeItem == null || aktualItem == null)
                continue;

            if (beforeItem.YPosition != aktualItem.YPosition)
            {
                Debug.Log(beforeItem + " " + aktualItem);
                if(!nextPaths.ContainsKey(item.Key))
                    nextPaths.Add(item.Key, item.Value);

                if (!nextPaths.ContainsKey(beforeItem.GetCordinats()))
                    nextPaths.Add(beforeItem.GetCordinats(), paths[beforeItem.GetCordinats()]);
                
                if (beforeItem.YPosition < aktualItem.YPosition)
                {
                   // aboveBlocks.Add(item.Value, GameManagerMap.Instance.Map[beforeItem.IndexUp.First()].GetCordinats());
                    
                }
                else if (beforeItem.YPosition > aktualItem.YPosition)
                {
                   // aboveBlocks.Add(item.Value, GameManagerMap.Instance.Map[aktualItem.IndexUp.First()].GetCordinats());
                }
            }
        }
        Debug.Log(string.Join(",", aboveBlocks));

        foreach (RaycastHit hit in hits)
        {
            GameObject hitObject = hit.collider.gameObject;
            if (!hitObject.GetComponent<TerritoryInfo>())
            {
                continue;
            }
            TerritroyReaded finded = GameManagerMap.Instance.Map[hitObject.transform.localPosition];
            Stack<TerritroyReaded> territoryes = new Stack<TerritroyReaded>();
            HashSet<TerritroyReaded> alreadyFinded = new HashSet<TerritroyReaded>();
            territoryes.Push(finded);

            bool doesFindSomething = false;
            while (true)
            {
                Stack<TerritroyReaded> newTerritoryes = new Stack<TerritroyReaded>();
                while (true)
                {
                    TerritroyReaded newFinded = territoryes.Pop();
                    alreadyFinded.Add(newFinded);

                    foreach (var item in newFinded)
                    {
                        TerritroyReaded detectItem = GameManagerMap.Instance.Map[item];
                        if (alreadyFinded.Contains(detectItem) || newTerritoryes.Contains(detectItem))//???
                            continue;

                        if (paths.ContainsKey(detectItem.GetCordinats()))
                        {
                            if (!nextPaths.ContainsKey(detectItem.GetCordinats()))
                            {
                                nextPaths.Add(detectItem.GetCordinats(), paths[detectItem.GetCordinats()]);
                            }

                            doesFindSomething = true;

                            foreach (var nextItem in detectItem)
                            {
                                TerritroyReaded nextDetectedItem = GameManagerMap.Instance.Map[nextItem];

                                if (paths.ContainsKey(nextDetectedItem.GetCordinats()) && !nextPaths.ContainsKey(nextDetectedItem.GetCordinats()))
                                {
                                    nextPaths.Add(nextDetectedItem.GetCordinats(), paths[nextDetectedItem.GetCordinats()]);
                                }
                            }
                        }
                        else
                        {
                            newTerritoryes.Push(detectItem);
                        }
                    }


                    if (doesFindSomething || territoryes.Count == 0)
                        break;

                }

                if (doesFindSomething)
                    break;

                while (newTerritoryes.Count > 0)
                {
                    territoryes.Push(newTerritoryes.Pop());
                }

            }

        }

        Vector3[] finalCordinats = new Vector3[indexes + aboveBlocks.Count];
        Array.Fill(finalCordinats, BIGVECTOR);
        int aboveIndex = 0;
        finalCordinats[indexes - 1] = firstVector;
        foreach (var item in nextPaths)
        {
            finalCordinats[item.Value + aboveIndex] = item.Key;
            if(aboveBlocks.ContainsKey(item.Value))
            {
                aboveIndex++;
                finalCordinats[item.Value + aboveIndex] = aboveBlocks[item.Value];
            }
        }

        var endList = finalCordinats.Where(n => n != BIGVECTOR).Distinct().Reverse().ToList();
        if (!endList.Contains(starter.GetCordinats()))
        {
            endList.Add(starter.GetCordinats());
        }
        return endList;

    }

    public Dictionary<Vector3, int> FindPathBack(TerritroyReaded starter)
    {
        Dictionary<Vector3, int> paths = new Dictionary<Vector3, int>(); //index from high to below
        TerritroyReaded aktual = starter;
        int indexes = 0;
        while (aktual != null)
        {
            paths.Add(aktual.GetCordinats(), indexes++);

            if (aktual.TerritoryInfo == TerritoryType.ShelterGround)
                Debug.Log(aktual + " " + _objectsCalculated[aktual]);

            aktual = _objectsCalculated[aktual];
        }
        return paths;
    }

    public Dictionary<TerritroyReaded, TerritroyReaded> CalculateAllPossible()
    {
        Dictionary<TerritroyReaded, TerritroyReaded> objectsCalculated = new Dictionary<TerritroyReaded, TerritroyReaded>();
        Dictionary<TerritroyReaded, TerritroyReaded> shelterGround = new Dictionary<TerritroyReaded, TerritroyReaded>();

        Stack<(TerritroyReaded orig, TerritroyReaded previus)> notCalculatedYet = new Stack<(TerritroyReaded orig, TerritroyReaded previus)>();
        Stack<(TerritroyReaded orig, TerritroyReaded previus)> nextCalculated = new Stack<(TerritroyReaded orig, TerritroyReaded previus)>();
        nextCalculated.Push((_selectedCharacter.ActualTerritory, null));

        for (int i = 0; i <= _countMove; i++)
        {
            HashSet<TerritroyReaded> already = new HashSet<TerritroyReaded>();
            while (nextCalculated.Count > 0)
            {
                (TerritroyReaded orig, TerritroyReaded previus) actual = nextCalculated.Pop();
                if (!objectsCalculated.ContainsKey(actual.orig) && actual.orig.TerritoryInfo != TerritoryType.ShelterGround)
                {
                    TerritroyReaded previus = actual.previus;
                    while(previus != null && previus.TerritoryInfo == TerritoryType.ShelterGround)
                    {
                        previus = shelterGround[previus];
                    }
                    

                    objectsCalculated.Add(actual.orig, previus);
                }

                foreach (var item in actual.orig.GetEnumeratorByOne(_selectedCharacter.ActualTerritory.Index))
                {

                    var detectItem = GameManagerMap.Instance.Map[item];


                    if (detectItem.TerritoryInfo == TerritoryType.Shelter ||
                        detectItem.IndexDown.Where(n => GameManagerMap.Instance.Map[n].TerritoryInfo == TerritoryType.Ground ||
                        GameManagerMap.Instance.Map[n].TerritoryInfo == TerritoryType.ShelterGround).Count() == 0)
                        continue;

                    if (objectsCalculated.ContainsKey(detectItem) || already.Contains(detectItem))
                        continue;

                    if(detectItem.TerritoryInfo == TerritoryType.ShelterGround && !shelterGround.ContainsKey(detectItem))
                    {
                        shelterGround.Add(detectItem, actual.orig);
                    }

                    notCalculatedYet.Push((detectItem, actual.orig));
                    already.Add(detectItem);

                }
            }

            while (notCalculatedYet.Count > 0)
            {
                nextCalculated.Push(notCalculatedYet.Pop());
            }
        }
        return objectsCalculated;
    }
}