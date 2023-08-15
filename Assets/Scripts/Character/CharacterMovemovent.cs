using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public class CharacterMovemovent : MonoBehaviour
{
    public static readonly Vector3 BIGVECTOR = new Vector3(int.MaxValue, int.MaxValue, int.MaxValue);
    public static readonly Vector3 POSITIONFORSPAWN = new Vector3(0, 0.5f, 0);

    [Header("Consts Test")]
    [SerializeField]
    private int _countMove = 5;
    [SerializeField]
    private float _speed = 1f;

    [Header("Script Objects")]

    [SerializeField]
    private LineRenderer _lineRenderer;

    private CharacterInfo _selectedCharacter;

    private (TerritroyReaded aktualTerritoryReaded, List<Vector3> path) _aktualTerritory;

    private Dictionary<TerritroyReaded, TerritroyReaded> _objectsCalculated; //orig, previous

    public int CountMoveCharacter { get => _countMove; set => _countMove = value; }
    public float SpeedCharacter { get => _speed; set => _speed = value; }

    public CharacterInfo SelectedCharacter { get => _selectedCharacter; set => _selectedCharacter = value; }

    public Action<(TerritroyReaded aktualTerritoryReaded, List<Vector3> path), CharacterInfo> OnSelectNewTerritory;
    public Action<TerritroyReaded, CharacterInfo> OnEndMoveToNewTerritory;

    private bool _isMoving;
    public bool IsMoving => _isMoving;

    private void Start()
    {
        _lineRenderer.gameObject.SetActive(false);
        GameManagerMap.Instance.OnClearMap += Clear;

        OnEndMoveToNewTerritory += DisableToBasic;
        OnSelectNewTerritory += SelectNewTerritory;

        //Config
        _speed = ConfigurationManager.Instance.CharacterData.characterSpeed;
        _countMove = ConfigurationManager.Instance.CharacterData.characterMoveDistance;
    }
    private void Update()
    {
        if (_selectedCharacter != null && _selectedCharacter.MoverActive()
            && GameManagerMap.Instance.State == GameState.FreeMovement)
        {
            SpawnMover();
        }
    }

    private void SpawnMover() //Detect Territory to move
    {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray); //RayCast from Mouse

        if (hits.Count() > 0)
        {
            var neededHits = hits.Where(n => n.collider.gameObject.tag == "PanelMovement"); //Detect hits only from Territory than can be moved to
            if (neededHits.Count() == 0 || hits.Where(n => n.collider.gameObject.GetComponent<CharacterInfo>()).Count() > 0)
                return;

            var hit = neededHits.FirstOrDefault(); //get First one

            TerritroyReaded detectTerritory;
            detectTerritory = GameManagerMap.Instance.Map[hit.collider.gameObject.transform.localPosition + POSITIONFORSPAWN]; //get air territory

            if (detectTerritory != _aktualTerritory.aktualTerritoryReaded) //if another territory detected
            {
                _aktualTerritory.aktualTerritoryReaded = detectTerritory;

                OnSelectNewTerritory(_aktualTerritory, _selectedCharacter);
            }

            if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0) && !_selectedCharacter.isAktualTerritory(_aktualTerritory.aktualTerritoryReaded))
            {
                StartCoroutine(CoroutineNewPositionCharacter(_aktualTerritory.aktualTerritoryReaded, _aktualTerritory.path)); //make movement to person
            }

        }
    }

    private void SelectNewTerritory((TerritroyReaded aktualTerritoryReaded, List<Vector3> path) territory, CharacterInfo character)
    {

        _selectedCharacter.SetCordintasToMover(territory.aktualTerritoryReaded.GetCordinats()
            + GameManagerMap.Instance.MainParent.transform.position - POSITIONFORSPAWN); //set cordinats to mover

        _aktualTerritory.path = calculateAllPath(territory.aktualTerritoryReaded); //actual path to selected territory

        DrawLine(_aktualTerritory.path); //draw the line

    }


    public void CharacterSelect(CharacterInfo character)
    {
        if (GameManagerMap.Instance.State == GameState.FreeMovement)
        {
            _lineRenderer.gameObject.SetActive(true);

            if (_selectedCharacter != null && _selectedCharacter != character)
            {
                _selectedCharacter.OnDeselected();//deselect other chracters
            }
            _selectedCharacter = character;
            _objectsCalculated = CalculateAllPossible(_countMove); //algoritmus calculate all territories that player can move

            AirPlatformsSet(true);


            GameManagerMap.Instance.CharacterVisibility.UpdateVisibility(_selectedCharacter);
        }
    }

    public void CharacterDeselect()
    {
        if (GameManagerMap.Instance.State == GameState.FreeMovement)
        {
            _lineRenderer.gameObject.SetActive(false);

            _selectedCharacter = null;
            AirPlatformsSet(false);

            _objectsCalculated.Clear();

            GameManagerMap.Instance.CharacterVisibility.UpdateVisibility(_selectedCharacter);
        }
    }

    public void AirPlatformsSet(bool result)
    {
        foreach (var item in _objectsCalculated.Keys)
        {
            if (GameManagerMap.Instance.Map.GetAirPlatform(item) != null)
            {
                GameManagerMap.Instance.Map.GetAirPlatform(item).SetActive(result);
            }
        }
    }

    public void LineRendererSet(bool result)
    {
        _lineRenderer.gameObject.SetActive(result);
    }

    private IEnumerator CoroutineNewPositionCharacter(TerritroyReaded newTerritory, List<Vector3> points)
    {

        _selectedCharacter.MoverActive(false);//disable mover
        _selectedCharacter.ActualTerritory = null;

        _isMoving = true;

        yield return StartCoroutine(CoroutineMove(points)); //start movements

        _isMoving = false;

        OnEndMoveToNewTerritory(newTerritory, _selectedCharacter);
    }

    private void DisableToBasic(TerritroyReaded newTerritory, CharacterInfo character)
    {
        character.ActualTerritory = newTerritory;
        _lineRenderer.positionCount = 0;

        var save = character;
        character.OnDeselected();
        save.OnSelected(save);

        GameManagerMap.Instance.CharacterVisibility.UpdateVisibility(_selectedCharacter);

        _selectedCharacter.SetCordintasToMover(newTerritory.GetCordinats()
            + GameManagerMap.Instance.MainParent.transform.position - POSITIONFORSPAWN); //set cordinats to mover
    }

    private IEnumerator CoroutineMove(List<Vector3> targets)
    {
        float elapsedTime = 0f;
        int index = 0;
        Vector3 target = targets[++index]; //ignore first, becouse its for line

        while (true)
        {

            while (Vector3.Distance(_selectedCharacter.gameObject.transform.localPosition, target) > 0.1f)
            {
                _selectedCharacter.GunPrefab.transform.LookAt(target + GameManagerMap.Instance.MainParent.transform.position);
                elapsedTime = Time.deltaTime * _speed;
                _selectedCharacter.gameObject.transform.localPosition = Vector3.MoveTowards(_selectedCharacter.gameObject.transform.localPosition, target, elapsedTime);
                yield return null;
            }
            _selectedCharacter.gameObject.transform.localPosition = target;

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
        List<Vector3> path = new List<Vector3> //start with begin and end
        {
            _selectedCharacter.transform.localPosition,
            starter.GetCordinats()
        };

        Dictionary<Vector3, Vector3> airPaths = new Dictionary<Vector3, Vector3>();

        while (true) //in this cyklus we will find points in which ones must line gone
        {
            List<Vector3> newPath = new List<Vector3>();
            for (int i = 0; i < path.Count - 1; i++) //in this, we detect, if the between points exist shelters

            {
                var iList = calculatePoints(GameManagerMap.Instance.Map[path[i + 1]], path[i]);
                newPath.AddRange(iList.paths);//if yes, we add them to list
                foreach (var item in iList.airPaths)
                {
                    airPaths.TryAdd(item.Key, item.Value);
                }

            }

            newPath = newPath.Distinct().ToList();//delete all same pieces
            if (newPath.Count == path.Count)//if old path and new path has same count, it means that we cant find new territories to stop
                break;

            path = new List<Vector3>(newPath);
        }

        var basicPaths = FindPathBack(starter); //detect all path from begin to end
        int indexes = basicPaths.Count + 1;
        Vector3[] finalCordinats = new Vector3[indexes + airPaths.Count]; //create array to make consistent path
        Array.Fill(finalCordinats, BIGVECTOR);

        if (path.Count == 0)
        {
            return path;
        }


        finalCordinats[indexes + airPaths.Count - 1] = path.First();//set first element, because for reserve
        int airIndex = airPaths.Count; // index for seting path in raw

        foreach (var item in path)
        {
            if (airPaths.ContainsKey(item)) //if here is air
            {
                finalCordinats[basicPaths[item] + airIndex] = airPaths[item]; //add air
                airPaths.Remove(item);
                airIndex--; // after add next item
            }
            finalCordinats[basicPaths[item] + airIndex] = item;



        }
        var endList = finalCordinats.Where(n => n != BIGVECTOR).Distinct().Reverse().ToList();

        return endList;
    }

    public (List<Vector3> paths, Dictionary<Vector3, Vector3> airPaths) calculatePoints(TerritroyReaded starter, Vector3 firstVector)
    {
        Dictionary<Vector3, int> paths = FindPathBack(starter); //find all path from starter to aktual player (path is territories with their numeration)
        int indexes = paths.Count + 1;//spesial indexer for future sort path

        Vector3 targetPosition = starter.GetCordinats() - firstVector;
        RaycastHit[] hits = Physics.RaycastAll(firstVector + GameManagerMap.Instance.MainParent.transform.position, targetPosition, Vector3.Distance(firstVector, starter.GetCordinats()));

        Debug.DrawRay(firstVector + GameManagerMap.Instance.MainParent.transform.position, targetPosition, Color.red);

        Dictionary<Vector3, int> nextPaths = new Dictionary<Vector3, int>(); //the points where we need to make stops for line

        Dictionary<Vector3, Vector3> airPaths = new Dictionary<Vector3, Vector3>(); //the points with air paths

        foreach (var item in paths) //in this cycle we detect all air points (only for shelter ground)
        {
            var aktualItem = GameManagerMap.Instance.Map[item.Key];
            var beforeItem = _objectsCalculated[aktualItem];

            if (beforeItem == null || aktualItem == null)
                continue;

            if (beforeItem.YPosition != aktualItem.YPosition && beforeItem.HasGround() && aktualItem.HasGround())
            {
                TerritroyReaded newAir = null;//detected Air territory
                if (beforeItem.YPosition < aktualItem.YPosition) //get correct territory
                {
                    newAir = GameManagerMap.Instance.Map[beforeItem.IndexUp.First()];
                }
                else if (beforeItem.YPosition > aktualItem.YPosition)
                {
                    newAir = GameManagerMap.Instance.Map[aktualItem.IndexUp.First()];
                }

                if (newAir != null)
                {
                    nextPaths.TryAdd(item.Key, item.Value);
                    nextPaths.TryAdd(beforeItem.GetCordinats(), paths[beforeItem.GetCordinats()]);
                    airPaths.TryAdd(aktualItem.GetCordinats(), newAir.GetCordinats());
                }

            }
        }

        foreach (RaycastHit hit in hits) // check all hits
        {
            GameObject hitObject = hit.collider.gameObject;
            if (!hitObject.GetComponent<TerritoryInfo>())
                continue;

            if (hitObject.name == "NoParent")
                hitObject = hitObject.transform.parent.gameObject;

            TerritroyReaded finded = GameManagerMap.Instance.Map[hitObject.transform.localPosition];//find hit territory
            Stack<TerritroyReaded> territoryes = new Stack<TerritroyReaded>(); //Stack for new territories, which we must detect
            HashSet<TerritroyReaded> alreadyFinded = new HashSet<TerritroyReaded>(); //This hashSet we use to optimation our future calculations
            territoryes.Push(finded); // first territory

            bool doesFindSomething = false; // stop boolean
            while (true) // in this cycle we search from hit for any territory which is in our path and
            {
                Stack<TerritroyReaded> newTerritoryes = new Stack<TerritroyReaded>(); //future territories which we must detect after one cycle
                while (true)
                {
                    TerritroyReaded newFinded = territoryes.Pop();
                    alreadyFinded.Add(newFinded);

                    foreach (var item in newFinded) //analyze all neighbors
                    {
                        TerritroyReaded detectItem = GameManagerMap.Instance.Map[item];
                        if (alreadyFinded.Contains(detectItem) || newTerritoryes.Contains(detectItem))//IT CAN BE MAKED FASTER O(n)!!!! newTerritoryes is a stack and contains is O(n) operation
                            continue;

                        if (paths.ContainsKey(detectItem.GetCordinats()))
                        {
                            nextPaths.TryAdd(detectItem.GetCordinats(), paths[detectItem.GetCordinats()]);

                            doesFindSomething = true; //stop algoritmus after all cycle

                            foreach (var nextItem in detectItem) // detect also neighbors' neigbors
                            {
                                TerritroyReaded nextDetectedItem = GameManagerMap.Instance.Map[nextItem];

                                if (paths.ContainsKey(nextDetectedItem.GetCordinats()))
                                {
                                    nextPaths.TryAdd(nextDetectedItem.GetCordinats(), paths[nextDetectedItem.GetCordinats()]);
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

                territoryes = new Stack<TerritroyReaded>(newTerritoryes); //add all newTerritoryes to territoryes for detect other terriotories
            }
        }

        //the final phase of the algoritmus
        Vector3[] finalCordinats = new Vector3[indexes];//create array to make new numeration
        Array.Fill(finalCordinats, BIGVECTOR);

        finalCordinats[indexes - 1] = firstVector;//set last as first, because reserve in the finish of algoritmus
        foreach (var item in nextPaths) //set all points in which we need to make stops
        {
            finalCordinats[item.Value] = item.Key;
        }

        var endList = finalCordinats.Where(n => n != BIGVECTOR).Distinct().Reverse().ToList();//clear from BIGVECTOR and reserve the array
        endList.Add(starter.GetCordinats());

        return (endList, airPaths);

    }

    public Dictionary<Vector3, int> FindPathBack(TerritroyReaded starter, TerritroyReaded begin = null)
    {
        Dictionary<Vector3, int> paths = new Dictionary<Vector3, int>(); //index from high to below
        TerritroyReaded aktual = starter;
        int indexes = 0;
        while (aktual != begin)
        {
            paths.Add(aktual.GetCordinats(), indexes++);

            aktual = _objectsCalculated[aktual];
        }
        return paths;
    }

    public Dictionary<TerritroyReaded, TerritroyReaded> CalculateAllPossible(int countMove)
    {
        Dictionary<TerritroyReaded, TerritroyReaded> objectsCalculated = new Dictionary<TerritroyReaded, TerritroyReaded>();//the final version of list of territories

        Stack<(TerritroyReaded orig, TerritroyReaded previus)> nextCalculated = new Stack<(TerritroyReaded orig, TerritroyReaded previus)>();//need to calculate territories
        nextCalculated.Push((_selectedCharacter.ActualTerritory, null));//first element
        HashSet<TerritroyReaded> already = new HashSet<TerritroyReaded>(); //save all territries that we dont need to detect

        HashSet<TerritroyReaded> territoriesBan = new HashSet<TerritroyReaded>();
        for (int i = 0; i <= countMove; i++)
        {
            int calcs = 0;

            Stack<(TerritroyReaded orig, TerritroyReaded previus)> notCalculatedYet = new Stack<(TerritroyReaded orig, TerritroyReaded previus)>(); //the elements which we need to detect in next cycle
            calcs = nextCalculated.Count();
            while (nextCalculated.Count > 0)
            {

                (TerritroyReaded orig, TerritroyReaded previus) actual = nextCalculated.Pop();
                if (actual.orig.TerritoryInfo != TerritoryType.ShelterGround && actual.orig.TerritoryInfo != TerritoryType.Enemy) // we cant move on shelterground element
                {
                    if (objectsCalculated.ContainsKey(actual.orig))
                    {
                        var oldItem = objectsCalculated[actual.orig];
                        if (oldItem != null)
                        {
                            if ((oldItem.YPosition != actual.orig.YPosition && actual.previus.YPosition == actual.orig.YPosition) ||
                                (oldItem.YPosition != actual.orig.YPosition && actual.previus.YPosition != actual.orig.YPosition &&
                                   Vector3.Distance(_selectedCharacter.transform.localPosition, actual.previus.GetCordinats()) < Vector3.Distance(_selectedCharacter.transform.localPosition, oldItem.GetCordinats()))) //||
                            {
                                objectsCalculated.Remove(actual.orig);
                                objectsCalculated.Add(actual.orig, actual.previus);
                            }
                        }
                    }
                    else
                    {
                        objectsCalculated.Add(actual.orig, actual.previus);
                    }
                }

                foreach (var item in actual.orig)// detect all neighbors
                {
                    var detectItem = GameManagerMap.Instance.Map[item];

                    if (detectItem.TerritoryInfo == TerritoryType.ShelterGround) //if we detect Shelter Ground, set detectItem as air above it 
                    {
                        detectItem = GameManagerMap.Instance.Map[detectItem.IndexUp.OrderBy(n => Vector3.Distance(TerritroyReaded.MakeVectorFromIndex(actual.orig.Index), TerritroyReaded.MakeVectorFromIndex(n))).FirstOrDefault()];
                    }
                    if (detectItem.IndexDown.Where(n => GameManagerMap.Instance.Map[n].TerritoryInfo == TerritoryType.Air).Count() == 1)//for down air
                    {
                        var newItem = detectItem.IndexDown.Where(n => GameManagerMap.Instance.Map[n].TerritoryInfo == TerritoryType.Air).FirstOrDefault();
                        while (GameManagerMap.Instance.Map[newItem].IndexDown.Where(n => GameManagerMap.Instance.Map[n].TerritoryInfo == TerritoryType.Air).Count() == 1)
                        {
                            newItem = GameManagerMap.Instance.Map[newItem].IndexDown.Where(n => GameManagerMap.Instance.Map[n].TerritoryInfo == TerritoryType.Air).FirstOrDefault();
                        }
                        detectItem = GameManagerMap.Instance.Map[newItem];

                    }
                    if (detectItem.TerritoryInfo == TerritoryType.Shelter || detectItem.TerritoryInfo == TerritoryType.Enemy ||
                      detectItem.IndexDown.Where(n => GameManagerMap.Instance.Map[n].TerritoryInfo == TerritoryType.Ground ||
                      GameManagerMap.Instance.Map[n].TerritoryInfo == TerritoryType.ShelterGround).Count() == 0) // we dont select such territories
                        continue;

                    if (objectsCalculated.ContainsKey(detectItem))
                    {
                        continue;
                    }

                    if (already.Contains(detectItem) && (detectItem.IsNearIsGround() == false || detectItem.InACenterOfGronds() == true))
                        continue;

                    notCalculatedYet.Push((detectItem, actual.orig));

                    if (!already.Contains(detectItem))
                        already.Add(detectItem);

                }
            }

            nextCalculated = new Stack<(TerritroyReaded orig, TerritroyReaded previus)>(notCalculatedYet);
            //  Debug.Log(calcs);
        }
        return objectsCalculated;
    }

    private void Clear()
    {
        StopAllCoroutines();
        _lineRenderer.gameObject.SetActive(false);
        _lineRenderer.positionCount = 0;

        if (_selectedCharacter != null)
        {
            _selectedCharacter.OnDeselected();
        }
    }
}
