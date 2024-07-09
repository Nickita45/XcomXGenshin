using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using static UnityEngine.EventSystems.EventTrigger;

// Manages UI and game elements related to the movement of the selected character.
public class MovementManager : MonoBehaviour
{
    public static readonly Vector3 BIGVECTOR = new(int.MaxValue, int.MaxValue, int.MaxValue);// vector with max value of Integer
    public static readonly Vector3 POSITIONFORSPAWN = new(0, 0.5f, 0); //position for spawn air platforms

    [Header("Script Objects")]
    [SerializeField]
    private LineRenderer _lineRenderer;

    private (TerritroyReaded aktualTerritoryReaded, List<Vector3> path) _aktualTerritory;
    private Dictionary<TerritroyReaded, TerritroyReaded> _territoriesCalculated; //orig, previous
    private float _timerCanBeSeleced = 0.5f; //resolves problem with automove if character selected by mouse
    private bool _isMoving;

    public Action<(TerritroyReaded aktualTerritoryReaded, List<Vector3> path), Character> OnSelectNewTerritory;
    public Action OnStartMove;
    public Action<TerritroyReaded, Character> OnEndMove;

    public bool IsMoving => _isMoving;
    public TerritroyReaded GetSelectedTerritory => _aktualTerritory.aktualTerritoryReaded;
    public void LineRendererSet(bool result) => _lineRenderer.gameObject.SetActive(result);


    #region MONO

    private void Start()
    {
        _lineRenderer.gameObject.SetActive(false);
        Manager.Instance.OnClearMap += Clear;
        Manager.StatusMain.OnStatusChange += OnStatusChange;

        OnEndMove += DisableToBasic;
        OnSelectNewTerritory += SelectNewTerritory;

        OnSelectNewTerritory +=
            (territory, character) => ShelterDetectUtils.UpdateShelterObjects(territory.aktualTerritoryReaded, character);
        OnEndMove +=
            (_, character) => ShelterDetectUtils.DisableShelterObjects(character);
    }

    private void Update()
    {
        if (Manager.TurnManager.SelectedCharacter?.MoverActive() == true)
        {
            if (_timerCanBeSeleced > 0)
            {
                _timerCanBeSeleced -= Time.deltaTime;
                return;
            }

            SpawnMover();
        }
    }

    #endregion

    public void AirPlatformsSet(bool result)
    {
        foreach (var item in _territoriesCalculated.Keys)
            Manager.Map.GetAirPlatform(item)?.SetActive(result);
    }
    public void ResetCharacterMover()
    {
        Manager.TurnManager.SelectedCharacter.SetCoordinatsToMover(Manager.TurnManager.SelectedCharacter.ActualTerritory.GetCordinats()
              + Manager.MainParent.transform.position - POSITIONFORSPAWN); //set cordinats to mover
        _lineRenderer.positionCount = 0;
        _aktualTerritory = (null, null);
    }

    private void SpawnMover() //Detect Territory to move
    {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray); //RayCast from Mouse
        if (hits.Length > 0)
        {
            var neededHits = hits.Where(n => n.collider.gameObject.tag == "PanelMovement"); //Detect hits only from Territory than can be moved to
            if (!neededHits.Any() || hits.Count(n => n.collider.gameObject.GetComponent<CharacterStats>()) > 0)
                return;

            var hit = neededHits.FirstOrDefault(); //get First one

            TerritroyReaded detectTerritory = Manager.Map[hit.collider.gameObject.transform.localPosition + POSITIONFORSPAWN]; //get air territory

            if (detectTerritory != _aktualTerritory.aktualTerritoryReaded) //if another territory detected
            {
                _aktualTerritory.aktualTerritoryReaded = detectTerritory;

                OnSelectNewTerritory(_aktualTerritory, Manager.TurnManager.SelectedCharacter); //we choose new territory
            }

            if (Manager.HasPermission(Permissions.SelectPlaceToMovement))
            {
                if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0)
                    && !Manager.TurnManager.SelectedCharacter.IsActualTerritory(_aktualTerritory.aktualTerritoryReaded))
                {
                    //make movement to person
                    StartCoroutine(MoveSelectedCharacter(_aktualTerritory.aktualTerritoryReaded, _aktualTerritory.path)); 
                }
            }
        }
    }

    private void SelectNewTerritory((TerritroyReaded aktualTerritoryReaded, List<Vector3> path) territory, Character character)
    {
        Manager.TurnManager.SelectedCharacter.SetCoordinatsToMover(territory.aktualTerritoryReaded.GetCordinats()
            + Manager.MainParent.transform.position - POSITIONFORSPAWN); //set cordinats to mover of selectet character


        if (!Manager.HasPermission(Permissions.SummonObjectOnMap))
        {
            _aktualTerritory.path = CalculateAllPath(territory.aktualTerritoryReaded, character, _territoriesCalculated); //calculate actual path to selected territory

            DrawLine(_aktualTerritory.path); //draw the line
        }
    }

    private void DrawLine(List<Vector3> points)
    {
        _lineRenderer.positionCount = points.Count;

        for (int i = 0; i < points.Count; i++)
        {
            _lineRenderer.SetPosition(i, points[i] + Manager.MainParent.transform.position);
        }
    }
    

    #region Algoritmuses
    public List<Vector3> CalculateAllPath(TerritroyReaded starter, Unit unit, Dictionary<TerritroyReaded, TerritroyReaded> objectsCaclucated)
    {
        List<Vector3> path = new() //list of all point "breaks"
        {
            unit.transform.localPosition, //first point is unit location
            starter.GetCordinats() //selected block
        };

        Dictionary<Vector3, Vector3> airPaths = new(); //list of points of ascents and descents(air)

        while (true) //in this cyklus we will find all points "breaks"
        {
            List<Vector3> newPath = new(); //create new list of points
            for (int i = 0; i < path.Count - 1; i++) //we detect, if the between points exist shelters

            {
                var iList = CalculatePoints(Manager.Map[path[i + 1]], path[i], objectsCaclucated);
                newPath.AddRange(iList.paths);//if yes, we add them to list
                foreach (var item in iList.airPaths) // and add air points
                {
                    airPaths.TryAdd(item.Key, item.Value);
                }
            }

            newPath = newPath.Distinct().ToList();//delete all same pieces
            if (newPath.Count == path.Count)//if old path and new path has same count, it means that we cant find new brakes
                break;

            path = new List<Vector3>(newPath);
        }

        var basicPaths = FindPathBack(starter, objectsCaclucated); //detect all path from begin to end with their indexes
        int indexes = basicPaths.Count + 1; //to first one
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

    public (List<Vector3> paths, Dictionary<Vector3, Vector3> airPaths) CalculatePoints(TerritroyReaded starter, Vector3 firstVector, Dictionary<TerritroyReaded, TerritroyReaded> objectsCaclucated)
    {
        Dictionary<Vector3, int> paths = FindPathBack(starter, objectsCaclucated); //find all path from starter to aktual player (path is territories with their numeration)
        int indexes = paths.Count + 1;//spesial indexer for future sort path

        Vector3 targetPosition = starter.GetCordinats() - firstVector;
        RaycastHit[] hits = Physics.RaycastAll(firstVector + Manager.MainParent.transform.position, targetPosition, Vector3.Distance(firstVector, starter.GetCordinats()));

        Debug.DrawRay(firstVector + Manager.MainParent.transform.position, targetPosition, Color.red);

        Dictionary<Vector3, int> nextPaths = new(); //the points where we need to make break for line

        Dictionary<Vector3, Vector3> airPaths = new(); //the points with air paths

        foreach (var item in paths) //in this cycle we detect all air breaks (only for shelter ground)
        {
            var aktualItem = Manager.Map[item.Key];
            var beforeItem = objectsCaclucated[aktualItem]; //get previus block

            if (beforeItem == null || aktualItem == null) //if we in the beggining (cant find previous or next block)
                continue;

            if (beforeItem.YPosition != aktualItem.YPosition && beforeItem.HasGround() && aktualItem.HasGround())
            { //if it is true => we find air break
                TerritroyReaded newAir = null;//detected Air territory
                if (beforeItem.YPosition < aktualItem.YPosition) //up or down air break
                {
                    newAir = Manager.Map[beforeItem.IndexUp.First()];
                }
                else if (beforeItem.YPosition > aktualItem.YPosition)
                {
                    newAir = Manager.Map[aktualItem.IndexUp.First()];
                }

                if (newAir != null)
                { //if we find new air break, add it to our air breaks
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

            TerritroyReaded finded = Manager.Map[hitObject.transform.localPosition];//find hit territory
            Queue<TerritroyReaded> territoryes = new(); //Queue for new territories, which we must detect
            HashSet<TerritroyReaded> alreadyFinded = new(); //This hashSet we use to optimation our future calculations
            territoryes.Enqueue(finded); // first territory

            bool doesFindSomething = false; // stop boolean
            while (true) // in this cycle we search from hit for any territory which is in our path and
            {
                do
                {
                    TerritroyReaded newFinded = territoryes.Dequeue();
                    alreadyFinded.Add(newFinded);

                    foreach (var item in newFinded) //analyze all neighbors
                    {
                        TerritroyReaded detectItem = Manager.Map[item];
                        if (alreadyFinded.Contains(detectItem) || territoryes.Contains(detectItem))//IT CAN BE MAKED FASTER O(n)!!!! territoryes is a queue and contains is O(n) operation
                            continue;

                        if (paths.ContainsKey(detectItem.GetCordinats()))
                        {
                            nextPaths.TryAdd(detectItem.GetCordinats(), paths[detectItem.GetCordinats()]);

                            doesFindSomething = true; //stop algoritmus after all cycle

                            foreach (var nextItem in detectItem) // detect also neighbors' neigbors
                            {
                                TerritroyReaded nextDetectedItem = Manager.Map[nextItem];

                                if (paths.ContainsKey(nextDetectedItem.GetCordinats()))
                                {
                                    nextPaths.TryAdd(nextDetectedItem.GetCordinats(), paths[nextDetectedItem.GetCordinats()]);
                                }
                            }
                        }
                        else
                        {
                            territoryes.Enqueue(detectItem);
                        }
                    }
                }
                while (!doesFindSomething && territoryes.Count != 0);

                if (doesFindSomething)
                    break;
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

    public Dictionary<Vector3, int> FindPathBack(TerritroyReaded starter, Dictionary<TerritroyReaded, TerritroyReaded> objectsCaclucated, TerritroyReaded begin = null)
    {
        Dictionary<Vector3, int> paths = new(); //index from high to below
        TerritroyReaded aktual = starter;
        int indexes = 0;
        while (aktual != begin)
        {
            paths.Add(aktual.GetCordinats(), indexes++);

            aktual = objectsCaclucated[aktual];
        }
        return paths; //paths is a dictionary with a way from starter to begin with indexers
    }

    public Dictionary<TerritroyReaded, TerritroyReaded> CalculateAllPossible(int countMove, Unit unit)
    {
        Dictionary<TerritroyReaded, TerritroyReaded> objectsCalculated = new();//the final version of list of territories

        Queue<(TerritroyReaded orig, TerritroyReaded previus)> nextCalculated = new();//stack for bottom up calculations
        nextCalculated.Enqueue((unit.ActualTerritory, null));//first element
        HashSet<TerritroyReaded> already = new(); //save all blocks that we already detect

        int startValueMove = 0;

        // charge
        if (unit is Character && !Manager.StatusMain.HasPermisson(Permissions.SummonObjectOnMap))
        {
            if (unit.ActionsLeft > 1)
            {
                startValueMove = countMove;
                countMove *= 2;
            }
        }


        for (int i = 0; i <= countMove; i++)
        {
            int countNeedToBeAnalyzed = nextCalculated.Count(); //get count blocks we need to detect
            while (countNeedToBeAnalyzed > 0) //while exists block that we do not detect
            {
                --countNeedToBeAnalyzed;
                (TerritroyReaded orig, TerritroyReaded previus) = nextCalculated.Dequeue();

                if ((orig.TerritoryInfo != TerritoryType.Character || orig == unit.ActualTerritory) &&
                    orig.TerritoryInfo != TerritoryType.ShelterGround && orig.TerritoryInfo != TerritoryType.Enemy) //detect only block which we can move on
                {
                    if (objectsCalculated.ContainsKey(orig))
                    {
                        var oldItem = objectsCalculated[orig]; // this part of the code makes it more human
                        if (oldItem != null)
                        {
                            if ((oldItem.YPosition != orig.YPosition && previus.YPosition == orig.YPosition) ||
                                (oldItem.YPosition != orig.YPosition && previus.YPosition != orig.YPosition &&
                                   Vector3.Distance(unit.transform.localPosition, previus.GetCordinats()) < Vector3.Distance(unit.transform.localPosition, oldItem.GetCordinats())))
                            { //solves the problem with not nessary jumps
                                objectsCalculated.Remove(orig);
                                objectsCalculated.Add(orig, previus);

                                //change color if it is charge block or summon block
                                //mb move to method?
                                if (unit is Character)
                                {
                                    Manager.Map.GetAirPlatform(orig).GetComponent<PlateMoving>()
                                            .SetType(i > startValueMove ? PlateMovingType.Charge : PlateMovingType.Usual);
                                }
                            }
                        }
                    }
                    else
                    {
                        objectsCalculated.Add(orig, previus);

                        //change color if it is charge block or summon block
                        //mb move to method?
                        if (unit is Character)
                        {
                            Manager.Map.GetAirPlatform(orig).GetComponent<PlateMoving>()
                                .SetType(i > startValueMove ? PlateMovingType.Charge : PlateMovingType.Usual);
                        }
                    }
                }

                foreach (var item in orig)// detect all neighbors
                {
                    TerritroyReaded detectItem = Manager.Map[item];

                    if (detectItem.TerritoryInfo == TerritoryType.ShelterGround) //if we detect Shelter Ground, set detectItem as air above it 
                    {
                        var gettedString = detectItem.IndexUp.OrderBy(n => Vector3.Distance(TerritroyReaded.MakeVectorFromIndex(orig.Index), TerritroyReaded.MakeVectorFromIndex(n)))
                            .FirstOrDefault(n => Vector3.Distance(TerritroyReaded.MakeVectorFromIndex(orig.Index), TerritroyReaded.MakeVectorFromIndex(n)) < 2);

                        if (gettedString == null) continue;
                        detectItem = 
                            Manager.Map[gettedString];

                    }
                    if (detectItem.IndexDown.Count(n => Manager.Map[n].TerritoryInfo == TerritoryType.Air) == 1)//if there's air under the block, it's going down.
                    {
                        var newItem = detectItem.IndexDown.FirstOrDefault(n => Manager.Map[n].TerritoryInfo == TerritoryType.Air);
                        while (Manager.Map[newItem].IndexDown.Count(n => Manager.Map[n].TerritoryInfo == TerritoryType.Air) == 1)
                        { //select the bottommost block
                            newItem = Manager.Map[newItem].IndexDown.FirstOrDefault(n => Manager.Map[n].TerritoryInfo == TerritoryType.Air);
                        }
                        detectItem = Manager.Map[newItem];
                    }
                    if (detectItem.TerritoryInfo == TerritoryType.Shelter || detectItem.TerritoryInfo == TerritoryType.ShelterGround
                        || detectItem.TerritoryInfo == TerritoryType.Enemy || detectItem.TerritoryInfo == TerritoryType.Character ||
                      detectItem.IndexDown.Count(n => Manager.Map[n].TerritoryInfo == TerritoryType.Ground ||
                      Manager.Map[n].TerritoryInfo == TerritoryType.ShelterGround) == 0) // we dont select such blocks with such rules
                    {
                        continue;
                    }

                    if (objectsCalculated.ContainsKey(detectItem))
                    { //already detected block
                        continue;
                    }

                    if (already.Contains(detectItem) && (!detectItem.IsNearIsGround() || detectItem.InACenterOfGronds()))
                    { //optimization
                        continue;
                    }
                    nextCalculated.Enqueue((detectItem, orig));

                    if (!already.Contains(detectItem))
                        already.Add(detectItem);
                }
            }

        }
        return objectsCalculated;
    }

    public Dictionary<TerritroyReaded, TerritroyReaded> CalculateAllPossibleInSquare(int countMove, Unit unit)
    {
        Dictionary<TerritroyReaded, TerritroyReaded> objectsCalculated = new();//the final version of list of territories

        Queue<(TerritroyReaded orig, TerritroyReaded previus)> nextCalculated = new();//stack for bottom up calculations
        nextCalculated.Enqueue((unit.ActualTerritory, null));//first element
        HashSet<TerritroyReaded> already = new(); //save all blocks that we already detect

        for (int i = 0; i <= countMove; i++)
        {
            int countNeedToBeAnalyzed = nextCalculated.Count(); //get count blocks we need to detect
            while (countNeedToBeAnalyzed > 0) //while exists block that we do not detect
            {
                --countNeedToBeAnalyzed;
                (TerritroyReaded orig, TerritroyReaded previus) = nextCalculated.Dequeue();

                if (orig.TerritoryInfo == TerritoryType.Air)
                {
                    if (!objectsCalculated.ContainsKey(orig))
                    {
                        objectsCalculated.Add(orig, previus);
                        Manager.Map.GetAirPlatform(orig).GetComponent<PlateMoving>().SetType(PlateMovingType.Summon);
                    }

                }

                foreach (var item in orig)// detect all neighbors
                {
                    TerritroyReaded detectItem = Manager.Map[item];

                    if (detectItem.TerritoryInfo == TerritoryType.ShelterGround) //if we detect Shelter Ground, set detectItem as air above it 
                    {
                        var gettedString = detectItem.IndexUp.OrderBy(n => Vector3.Distance(TerritroyReaded.MakeVectorFromIndex(orig.Index), TerritroyReaded.MakeVectorFromIndex(n)))
                              .FirstOrDefault(n => Vector3.Distance(TerritroyReaded.MakeVectorFromIndex(orig.Index), TerritroyReaded.MakeVectorFromIndex(n)) < 2);

                        if (gettedString == null) continue;
                        detectItem =
                            Manager.Map[gettedString];
                    }
                    if (detectItem.IndexDown.Count(n => Manager.Map[n].TerritoryInfo == TerritoryType.Air) == 1)//if there's air under the block, it's going down.
                    {
                        var newItem = detectItem.IndexDown.FirstOrDefault(n => Manager.Map[n].TerritoryInfo == TerritoryType.Air);
                        while (Manager.Map[newItem].IndexDown.Count(n => Manager.Map[n].TerritoryInfo == TerritoryType.Air) == 1)
                        { //select the bottommost block
                            newItem = Manager.Map[newItem].IndexDown.FirstOrDefault(n => Manager.Map[n].TerritoryInfo == TerritoryType.Air);
                        }
                        detectItem = Manager.Map[newItem];
                    }
                    if (detectItem.IndexDown.Count(n => Manager.Map[n].TerritoryInfo == TerritoryType.Ground ||
                      Manager.Map[n].TerritoryInfo == TerritoryType.ShelterGround) == 0) // we dont select such blocks with such rules
                    {
                        continue;
                    }

                    if (objectsCalculated.ContainsKey(detectItem))
                    { //already detected block
                        continue;
                    }

                    if (already.Contains(detectItem) && (!detectItem.IsNearIsGround() || detectItem.InACenterOfGronds()))
                    { //optimization
                        continue;
                    }
                    nextCalculated.Enqueue((detectItem, orig));

                    if (!already.Contains(detectItem))
                        already.Add(detectItem);
                }
            }

            HashSet<TerritroyReaded> savingHash = new();

            foreach ((TerritroyReaded orig, TerritroyReaded previus) in nextCalculated.ToList())
            {
                foreach (var item in orig)// detect all neighbors
                {
                    TerritroyReaded detectItem = Manager.Map[item];
                    if ((orig.TerritoryInfo == TerritoryType.Shelter || orig.TerritoryInfo == TerritoryType.ShelterGround)
                        && detectItem.TerritoryInfo == TerritoryType.Air && detectItem.IndexDown.Any(n => Manager.Map[n].TerritoryInfo == TerritoryType.Air))
                    {
                        continue;
                    }


                    if (detectItem.TerritoryInfo == TerritoryType.ShelterGround) //if we detect Shelter Ground, set detectItem as air above it 
                    {
                        detectItem = Manager.Map[detectItem.IndexUp.OrderBy(n => Vector3.Distance(TerritroyReaded.MakeVectorFromIndex(orig.Index), TerritroyReaded.MakeVectorFromIndex(n))).FirstOrDefault()];
                    }
                    if (detectItem.IndexDown.Count(n => Manager.Map[n].TerritoryInfo == TerritoryType.Air) == 1)//if there's air under the block, it's going down.
                    {
                        var newItem = detectItem.IndexDown.FirstOrDefault(n => Manager.Map[n].TerritoryInfo == TerritoryType.Air);
                        while (Manager.Map[newItem].IndexDown.Count(n => Manager.Map[n].TerritoryInfo == TerritoryType.Air) == 1)
                        { //select the bottommost block
                            newItem = Manager.Map[newItem].IndexDown.FirstOrDefault(n => Manager.Map[n].TerritoryInfo == TerritoryType.Air);
                        }

                        detectItem = Manager.Map[newItem];
                    }
                    if (detectItem.IndexDown.Count(n => Manager.Map[n].TerritoryInfo == TerritoryType.Ground ||
                      Manager.Map[n].TerritoryInfo == TerritoryType.ShelterGround) == 0) // we dont select such blocks with such rules
                    {
                        continue;
                    }

                    if (objectsCalculated.ContainsKey(detectItem))
                    { //already detected block
                        continue;
                    }

                    if (already.Contains(detectItem) && (!detectItem.IsNearIsGround() || detectItem.InACenterOfGronds()))
                    { //optimization
                        continue;
                    }

                    if (savingHash.Contains(detectItem))
                        nextCalculated.Enqueue((detectItem, orig));
                    else
                        savingHash.Add(detectItem);
                }
            }
        }

        return objectsCalculated;
    }
    #endregion


    #region Coroutines
    private IEnumerator MoveSelectedCharacter(TerritroyReaded newTerritory, List<Vector3> points)
    {
        Character character = Manager.TurnManager.SelectedCharacter; //get selected character
        character.MoverActive(false); // disable mover
        character.ActualTerritory.TerritoryInfo = TerritoryType.Air; //set block where he was on air
        character.ActualTerritory = null;
        character.SelectItem.SetActive(false); //disable hi selecter

        _isMoving = true;
        OnStartMove();

        Manager.StatusMain.SetStatusWaiting();

        yield return StartCoroutine(character.Move(points)); //??????????

        _isMoving = false;

        if (!character.IsKilled)
        {
            OnEndMove(newTerritory, character);
            yield return Manager.TurnManager.AfterCharacterAction();
        }
        else
        {
            Manager.TurnManager.OutOfActions(character);
        }
    }

    public IEnumerator MoveEnemyToTerritoryFromSelected(Enemy enemy, Func<Dictionary<TerritroyReaded, TerritroyReaded>, TerritroyReaded> findTerritoryMoveTo)
    {
        var allPaths = Manager.MovementManager.CalculateAllPossible(enemy.Stats.MovementDistance(), enemy); // get enemie's objectsCalculated

        TerritroyReaded findTerritory = findTerritoryMoveTo(allPaths); //get target block
        // Only move if there's an available territory
        //if (findTerritory != null) //????? may be problem in future/can be in AI then cant find territory
        {
            List<Vector3> aktualPath = Manager.MovementManager.CalculateAllPath(findTerritory, enemy, allPaths); //get path with breaks for enemy

            // Only move if the path exists and contains at least 1 point
            if (aktualPath?.Count > 0)
            {
                yield return MoveUnitToTerritory(enemy, aktualPath, findTerritory);
            }
        }
    }

    public IEnumerator MoveUnitToTerritory(Unit unit, List<Vector3> aktualPath, TerritroyReaded findTerritory)
    {
        unit.ActualTerritory.TerritoryInfo = TerritoryType.Air; //actualization block type
        yield return Manager.MovementManager.StartCoroutine(unit.Move(aktualPath)); //?????? maybe better this method in this class


        if (!unit.IsKilled)
        {
            unit.ActualTerritory = findTerritory; //actualization enemy block
            unit.ActualTerritory.TerritoryInfo = TerritoryType.Character;
        }
    }
    #endregion


    #region Subscribers
    public void OnCharacterSelect(Character character)
    {
        Manager.TurnManager.SelectedCharacter.SetCoordinatsToMover(character.ActualTerritory.GetCordinats()
              + Manager.MainParent.transform.position - POSITIONFORSPAWN); //set cordinats to mover
    }

    public void OnCharacterDeselect()
    {
        _timerCanBeSeleced = 0.5f;
        _lineRenderer.positionCount = 0;

        AirPlatformsSet(false);

        _territoriesCalculated?.Clear();
    }

    private void DisableToBasic(TerritroyReaded newTerritory, Character character)
    {
        character.ActualTerritory = newTerritory; //set new block to character
        newTerritory.TerritoryInfo = TerritoryType.Character; //change block type
        _lineRenderer.positionCount = 0;

        if (Manager.Map.GetAirPlatform(newTerritory).GetComponent<PlateMoving>().GetPlateType == PlateMovingType.Charge) //minus actions
            character.ActionsLeft -= 2;
        else
            character.ActionsLeft -= 1;
    }

    private void OnStatusChange(HashSet<Permissions> permissions) //Clean by methods?
    {
        Character character = Manager.TurnManager.SelectedCharacter;
        if (permissions.Contains(Permissions.SelectPlaceToMovement))
        {
            if (character)
            {
                Manager.TurnManager.SelectedCharacter.SelectChanges();
                ShelterDetectUtils.DisableShelterObjects(character);
                _lineRenderer.gameObject.SetActive(true);
                _territoriesCalculated = CalculateAllPossible(character.Stats.MovementDistance(), character);
                Manager.TurnManager.SelectedCharacter.MoverActive(true);
                AirPlatformsSet(true);
            }
        } else if(permissions.Contains(Permissions.SummonObjectOnMap))
        {
            character.MoverActive(false);
            _lineRenderer.gameObject.SetActive(false);
            AirPlatformsSet(false);
            OnCharacterSelect(Manager.TurnManager.SelectedCharacter);
            _territoriesCalculated.Clear();

            if(Manager.AbilityPanel.Selected.Ability is IAbilitySummon abilitySummon) { 
                _territoriesCalculated = CalculateAllPossibleInSquare(abilitySummon.RangeSummon(), character);
            }

            AirPlatformsSet(true);
            Manager.TurnManager.SelectedCharacter.MoverActive(true);

        }
        else
        {
            if (character)
            {
                character.MoverActive(false);
                _lineRenderer.gameObject.SetActive(false);
                AirPlatformsSet(false);
                _territoriesCalculated.Clear();
            }
        }
    }

    private void Clear()
    {
        StopAllCoroutines();
        _lineRenderer.gameObject.SetActive(false); //clear the line render
        _lineRenderer.positionCount = 0;

        Manager.TurnManager.DeselectCharacter();
    }
    #endregion
}
