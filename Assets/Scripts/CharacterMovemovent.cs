using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Image;

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

    //private HashSet<TerritroyReaded> _objectsCalculated;
    private Dictionary<TerritroyReaded, TerritroyReaded> _objectsCalculated; //orig, previous


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


        if(Input.GetKey(KeyCode.K))
        {
            var test = CalculateAllPossible();
            Debug.Log(test.Count);
           
        }
    }

    private void SpawnMover()
    {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 cordinats = hit.point - GameManagerMap.Instance.MainParent.transform.position;
            cordinats = new Vector3(Mathf.Floor(cordinats.x), Mathf.Floor(cordinats.y), Mathf.Floor(cordinats.z));


            TerritroyReaded detectTerritory;
            if (GameManagerMap.Instance.Map.ContainsVertexByPox(cordinats, out detectTerritory) ||
                GameManagerMap.Instance.Map.ContainsVertexByPox(cordinats + new Vector3(0.5f, 0.5f, 0.5f), out detectTerritory))
            {
                if (_objectsCalculated.Keys.Contains(detectTerritory) && detectTerritory.TerritoryInfo != TerritoryType.Shelter && detectTerritory.IndexDown.Where(n => GameManagerMap.Instance.Map[n].TerritoryInfo == TerritoryType.Ground).Count() == 1)
                {
                    _selectedCharacter.SetCordintasToMover(detectTerritory.GetCordinats() + GameManagerMap.Instance.MainParent.transform.position - new Vector3(0, 0.5f, 0));
                    var list = calculatePoints(detectTerritory);
                    if(!list.Contains(detectTerritory.GetCordinats()))
                        list.Add(detectTerritory.GetCordinats());
                    
                    DrawLine(list);

                    if (Input.GetMouseButton(0) && !_selectedCharacter.isAktualTerritory(detectTerritory))
                    {
                        StartCoroutine(CoroutineNewPositionCharacter(detectTerritory, list));
                        
                    }
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

        while(true)
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




    public List<Vector3> calculatePoints(TerritroyReaded starter)
    {
        Dictionary<Vector3, int> paths = new Dictionary<Vector3, int>(); //index from high to below
        TerritroyReaded aktual = starter;
        int indexes = 0;
        while (aktual != null)
        {
            paths.Add(aktual.GetCordinats(), indexes++);
            aktual = _objectsCalculated[aktual];
        }
        Vector3 targetPosition = starter.GetCordinats() + GameManagerMap.Instance.MainParent.transform.position - _selectedCharacter.transform.position;
        RaycastHit[] hits = Physics.RaycastAll(_selectedCharacter.transform.position, targetPosition, paths.Count - 2);
        Debug.DrawRay(_selectedCharacter.transform.position, targetPosition, Color.red);

        Dictionary<Vector3, int> nextPaths = new Dictionary<Vector3, int>();

        foreach (RaycastHit hit in hits)
        {
            GameObject hitObject = hit.collider.gameObject;
            if (!hitObject.GetComponent<TerritoryInfo>())
            {
                continue;
            }
            TerritroyReaded finded = GameManagerMap.Instance.Map[hitObject.transform.localPosition];
            foreach (var item in finded)
            {
                TerritroyReaded detectedItem = GameManagerMap.Instance.Map[item];

                if (!paths.Keys.Contains(detectedItem.GetCordinats()))
                {
                    continue;
                }

                if (paths.Keys.Contains(detectedItem.GetCordinats()) && !nextPaths.Keys.Contains(detectedItem.GetCordinats()) )
                {
                    nextPaths.Add(detectedItem.GetCordinats(), paths[detectedItem.GetCordinats()]);
                }
                

                foreach (var nextItem in detectedItem)
                {
                    TerritroyReaded nextDetectedItem = GameManagerMap.Instance.Map[nextItem];

                    if (nextDetectedItem == starter)
                    {
//                        continue;
                    }

                    if (paths.Keys.Contains(nextDetectedItem.GetCordinats()) && !nextPaths.Keys.Contains(nextDetectedItem.GetCordinats()))
                    {
                        nextPaths.Add(nextDetectedItem.GetCordinats(), paths[nextDetectedItem.GetCordinats()]);
                    }
                }

            }

        }

        Vector3[] finalCordinats = new Vector3[indexes];
        Array.Fill(finalCordinats, BIGVECTOR);

        finalCordinats[indexes-1] = _selectedCharacter.transform.localPosition;
        foreach (var item in nextPaths)
        {
            finalCordinats[item.Value] = item.Key;
        }

        var endList = finalCordinats.Where(n => n != BIGVECTOR).Distinct().Reverse().ToList();
        
        return endList;
            /*LinkedList<Vector3> points = new LinkedList<Vector3>();
            TerritroyReaded aktual = starter;
            TerritroyReaded previus = null;
            Vector3 sub = CharacterMovemovent.BIGVECTOR;
            while(true)
            {
                var next = _objectsCalculated[aktual];
                if (next == null)
                    break;

                var newSub = aktual.GetCordinats() - next.GetCordinats();
                if(sub != CharacterMovemovent.BIGVECTOR)
                {
                    var ifSub = sub - newSub;
                    if (!((ifSub.x == 0f && ifSub.y == 0f) || (ifSub.x == 0f && ifSub.z == 0f) || (ifSub.y == 0f && ifSub.z == 0f)))
                    {
                        if (TerritroyReaded.DetectSampleShelters(next, previus))
                        {
                            points.AddFirst(aktual.GetCordinats());
                        }
                    }
                }
                previus = aktual;
                sub = newSub;

                aktual = next;

            }

            points.AddFirst(_selectedCharacter.ActualTerritory.GetCordinats()); //first is player
            return points.ToList();*/
        }


    public Dictionary<TerritroyReaded, TerritroyReaded> CalculateAllPossible()
    {
        Dictionary<TerritroyReaded, TerritroyReaded> objectsCalculated = new Dictionary<TerritroyReaded, TerritroyReaded>();
        
        Stack<(TerritroyReaded orig, TerritroyReaded previus)> notCalculatedYet = new Stack<(TerritroyReaded orig, TerritroyReaded previus)>();
        Stack<(TerritroyReaded orig, TerritroyReaded previus)> nextCalculated = new Stack<(TerritroyReaded orig, TerritroyReaded previus)>();

        nextCalculated.Push((_selectedCharacter.ActualTerritory, null));

        for (int i = 0; i <= _countMove;i++)
        {
            while (nextCalculated.Count > 0)
            {
                (TerritroyReaded orig, TerritroyReaded previus) actual = nextCalculated.Pop();
                if (!objectsCalculated.Keys.Contains(actual.orig))
                {
                    objectsCalculated.Add(actual.orig, actual.previus);
                }

                foreach (var item in actual.orig)
                {
                    var detectItem = GameManagerMap.Instance.Map[item];
                    if (detectItem.TerritoryInfo == TerritoryType.Shelter)
                        continue;
                    
                    notCalculatedYet.Push((detectItem, actual.orig));
                }
            }

            while(notCalculatedYet.Count > 0)
            {
                nextCalculated.Push(notCalculatedYet.Pop());
            }
        }
        return objectsCalculated;
    }
}
