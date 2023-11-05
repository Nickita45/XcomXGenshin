using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

// Manages UI and game elements related to the movement of the selected character.
public class MovementManager : MonoBehaviour
{
    public static readonly Vector3 BIGVECTOR = new(int.MaxValue, int.MaxValue, int.MaxValue);
    public static readonly Vector3 POSITIONFORSPAWN = new(0, 0.5f, 0);

    [Header("Script Objects")]

    [SerializeField]
    private LineRenderer _lineRenderer;

    private (TerritroyReaded aktualTerritoryReaded, List<Vector3> path) _aktualTerritory;

    private Dictionary<TerritroyReaded, TerritroyReaded> _objectsCalculated; //orig, previous

    public Action<(TerritroyReaded aktualTerritoryReaded, List<Vector3> path), Character> OnSelectNewTerritory;
    public Action OnStartMove;
    public Action<TerritroyReaded, Character> OnEndMove;

    private bool _isMoving;
    public bool IsMoving => _isMoving;

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
            SpawnMover();
        }
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

                OnSelectNewTerritory(_aktualTerritory, Manager.TurnManager.SelectedCharacter);
            }

            if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0) && !Manager.TurnManager.SelectedCharacter.IsActualTerritory(_aktualTerritory.aktualTerritoryReaded))
            {
                StartCoroutine(MoveSelectedCharacter(_aktualTerritory.aktualTerritoryReaded, _aktualTerritory.path)); //make movement to person
            }
        }
    }

    private void SelectNewTerritory((TerritroyReaded aktualTerritoryReaded, List<Vector3> path) territory, Character character)
    {
        Manager.TurnManager.SelectedCharacter.SetCoordinatsToMover(territory.aktualTerritoryReaded.GetCordinats()
            + Manager.MainParent.transform.position - POSITIONFORSPAWN); //set cordinats to mover

        _aktualTerritory.path = CalculateAllPath(territory.aktualTerritoryReaded, character, _objectsCalculated); //actual path to selected territory

        DrawLine(_aktualTerritory.path); //draw the line
    }

    public void OnCharacterSelect(Character character)
    {
        Manager.TurnManager.SelectedCharacter.SetCoordinatsToMover(character.ActualTerritory.GetCordinats()
              + Manager.MainParent.transform.position - POSITIONFORSPAWN); //set cordinats to mover
    }

    public void OnCharacterDeselect()
    {
        _lineRenderer.positionCount = 0;

        AirPlatformsSet(false);

        _objectsCalculated?.Clear();
    }

    public void AirPlatformsSet(bool result)
    {
        foreach (var item in _objectsCalculated.Keys)
        {
            Manager.Map.GetAirPlatform(item)?.SetActive(result);

            //if(!result)
            //   Manager.Map.GetAirPlatform(item).GetComponent<PlateMoving>().SetCharge(result);
        }
    }

    public void LineRendererSet(bool result) => _lineRenderer.gameObject.SetActive(result);

    private IEnumerator MoveSelectedCharacter(TerritroyReaded newTerritory, List<Vector3> points)
    {
        Character character = Manager.TurnManager.SelectedCharacter;
        character.MoverActive(false); // disable mover
        character.ActualTerritory.TerritoryInfo = TerritoryType.Air;
        character.ActualTerritory = null;
        character.SelectItem.SetActive(false);

        _isMoving = true;
        OnStartMove();

        Manager.StatusMain.SetStatusWaiting();

        yield return StartCoroutine(character.Move(points));

        _isMoving = false;
        OnEndMove(newTerritory, character);

        yield return Manager.TurnManager.AfterCharacterAction();
    }

    private void DisableToBasic(TerritroyReaded newTerritory, Character character)
    {
        character.ActualTerritory = newTerritory;
        newTerritory.TerritoryInfo = TerritoryType.Character;
        _lineRenderer.positionCount = 0;

        if (Manager.Map.GetAirPlatform(newTerritory).GetComponent<PlateMoving>().IsCharge)
            character.ActionsLeft -= 2;
        else
            character.ActionsLeft -= 1;
    }

    private void DrawLine(List<Vector3> points)
    {
        _lineRenderer.positionCount = points.Count;

        for (int i = 0; i < points.Count; i++)
        {
            _lineRenderer.SetPosition(i, points[i] + Manager.MainParent.transform.position);
        }
    }

    public List<Vector3> CalculateAllPath(TerritroyReaded starter, Unit unit, Dictionary<TerritroyReaded, TerritroyReaded> objectsCaclucated)
    {
        List<Vector3> path = new()
        {
            unit.transform.localPosition,
            starter.GetCordinats()
        };

        Dictionary<Vector3, Vector3> airPaths = new();

        while (true) //in this cyklus we will find points in which ones must line gone
        {
            List<Vector3> newPath = new();
            for (int i = 0; i < path.Count - 1; i++) //in this, we detect, if the between points exist shelters

            {
                var iList = CalculatePoints(Manager.Map[path[i + 1]], path[i], objectsCaclucated);
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

        var basicPaths = FindPathBack(starter, objectsCaclucated); //detect all path from begin to end
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

    public (List<Vector3> paths, Dictionary<Vector3, Vector3> airPaths) CalculatePoints(TerritroyReaded starter, Vector3 firstVector, Dictionary<TerritroyReaded, TerritroyReaded> objectsCaclucated)
    {
        Dictionary<Vector3, int> paths = FindPathBack(starter, objectsCaclucated); //find all path from starter to aktual player (path is territories with their numeration)
        int indexes = paths.Count + 1;//spesial indexer for future sort path

        Vector3 targetPosition = starter.GetCordinats() - firstVector;
        RaycastHit[] hits = Physics.RaycastAll(firstVector + Manager.MainParent.transform.position, targetPosition, Vector3.Distance(firstVector, starter.GetCordinats()));

        Debug.DrawRay(firstVector + Manager.MainParent.transform.position, targetPosition, Color.red);

        Dictionary<Vector3, int> nextPaths = new(); //the points where we need to make stops for line

        Dictionary<Vector3, Vector3> airPaths = new(); //the points with air paths

        foreach (var item in paths) //in this cycle we detect all air points (only for shelter ground)
        {
            var aktualItem = Manager.Map[item.Key];
            var beforeItem = objectsCaclucated[aktualItem];

            if (beforeItem == null || aktualItem == null)
                continue;

            if (beforeItem.YPosition != aktualItem.YPosition && beforeItem.HasGround() && aktualItem.HasGround())
            {
                TerritroyReaded newAir = null;//detected Air territory
                if (beforeItem.YPosition < aktualItem.YPosition) //get correct territory
                {
                    newAir = Manager.Map[beforeItem.IndexUp.First()];
                }
                else if (beforeItem.YPosition > aktualItem.YPosition)
                {
                    newAir = Manager.Map[aktualItem.IndexUp.First()];
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

            TerritroyReaded finded = Manager.Map[hitObject.transform.localPosition];//find hit territory
            Stack<TerritroyReaded> territoryes = new(); //Stack for new territories, which we must detect
            HashSet<TerritroyReaded> alreadyFinded = new(); //This hashSet we use to optimation our future calculations
            territoryes.Push(finded); // first territory

            bool doesFindSomething = false; // stop boolean
            while (true) // in this cycle we search from hit for any territory which is in our path and
            {
                Stack<TerritroyReaded> newTerritoryes = new(); //future territories which we must detect after one cycle
                do
                {
                    TerritroyReaded newFinded = territoryes.Pop();
                    alreadyFinded.Add(newFinded);

                    foreach (var item in newFinded) //analyze all neighbors
                    {
                        TerritroyReaded detectItem = Manager.Map[item];
                        if (alreadyFinded.Contains(detectItem) || newTerritoryes.Contains(detectItem))//IT CAN BE MAKED FASTER O(n)!!!! newTerritoryes is a stack and contains is O(n) operation
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
                            newTerritoryes.Push(detectItem);
                        }
                    }
                }
                while (!doesFindSomething && territoryes.Count != 0);

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
        return paths;
    }

    public Dictionary<TerritroyReaded, TerritroyReaded> CalculateAllPossible(int countMove, Unit unit)
    {
        Dictionary<TerritroyReaded, TerritroyReaded> objectsCalculated = new();//the final version of list of territories

        Stack<(TerritroyReaded orig, TerritroyReaded previus)> nextCalculated = new();//need to calculate territories
        nextCalculated.Push((unit.ActualTerritory, null));//first element
        HashSet<TerritroyReaded> already = new(); //save all territries that we dont need to detect

        int startValueMove = 0;

        // charge
        if (unit is Character)
        {
            if (unit.ActionsLeft > 1)
            {
                startValueMove = countMove;
                countMove *= 2;
            }
        }

        for (int i = 0; i <= countMove; i++)
        {
            int calcs = 0;

            Stack<(TerritroyReaded orig, TerritroyReaded previus)> notCalculatedYet = new(); //the elements which we need to detect in next cycle
            calcs = nextCalculated.Count;
            while (nextCalculated.Count > 0)
            {
                (TerritroyReaded orig, TerritroyReaded previus) = nextCalculated.Pop();

                if ((orig.TerritoryInfo != TerritoryType.Character || orig == unit.ActualTerritory) &&
                    orig.TerritoryInfo != TerritoryType.ShelterGround && orig.TerritoryInfo != TerritoryType.Enemy) // we cant move on shelterground element
                {
                    if (objectsCalculated.ContainsKey(orig))
                    {
                        var oldItem = objectsCalculated[orig];
                        if (oldItem != null)
                        {
                            if ((oldItem.YPosition != orig.YPosition && previus.YPosition == orig.YPosition) ||
                                (oldItem.YPosition != orig.YPosition && previus.YPosition != orig.YPosition &&
                                   Vector3.Distance(unit.transform.localPosition, previus.GetCordinats()) < Vector3.Distance(unit.transform.localPosition, oldItem.GetCordinats()))) //||
                            {
                                objectsCalculated.Remove(orig);
                                objectsCalculated.Add(orig, previus);

                                if (unit is Character)
                                    Manager.Map.GetAirPlatform(orig).GetComponent<PlateMoving>().SetCharge(i > startValueMove);
                            }
                        }
                    }
                    else
                    {
                        objectsCalculated.Add(orig, previus);

                        if (unit is Character)
                            Manager.Map.GetAirPlatform(orig).GetComponent<PlateMoving>().SetCharge(i > startValueMove);
                    }
                }

                foreach (var item in orig)// detect all neighbors
                {
                    TerritroyReaded detectItem = Manager.Map[item];

                    if (detectItem.TerritoryInfo == TerritoryType.ShelterGround) //if we detect Shelter Ground, set detectItem as air above it 
                    {
                        detectItem = Manager.Map[detectItem.IndexUp.OrderBy(n => Vector3.Distance(TerritroyReaded.MakeVectorFromIndex(orig.Index), TerritroyReaded.MakeVectorFromIndex(n))).FirstOrDefault()];
                    }
                    if (detectItem.IndexDown.Count(n => Manager.Map[n].TerritoryInfo == TerritoryType.Air) == 1)//for down air
                    {
                        var newItem = detectItem.IndexDown.FirstOrDefault(n => Manager.Map[n].TerritoryInfo == TerritoryType.Air);
                        while (Manager.Map[newItem].IndexDown.Count(n => Manager.Map[n].TerritoryInfo == TerritoryType.Air) == 1)
                        {
                            newItem = Manager.Map[newItem].IndexDown.FirstOrDefault(n => Manager.Map[n].TerritoryInfo == TerritoryType.Air);
                        }
                        detectItem = Manager.Map[newItem];
                    }
                    if (detectItem.TerritoryInfo == TerritoryType.Shelter || detectItem.TerritoryInfo == TerritoryType.ShelterGround
                        || detectItem.TerritoryInfo == TerritoryType.Enemy || detectItem.TerritoryInfo == TerritoryType.Character ||
                      detectItem.IndexDown.Count(n => Manager.Map[n].TerritoryInfo == TerritoryType.Ground ||
                      Manager.Map[n].TerritoryInfo == TerritoryType.ShelterGround) == 0) // we dont select such territories
                    {
                        continue;
                    }

                    if (objectsCalculated.ContainsKey(detectItem))
                    {
                        continue;
                    }

                    if (already.Contains(detectItem) && (!detectItem.IsNearIsGround() || detectItem.InACenterOfGronds()))
                    {
                        continue;
                    }
                    notCalculatedYet.Push((detectItem, orig));

                    if (!already.Contains(detectItem))
                        already.Add(detectItem);
                }
            }

            nextCalculated = new Stack<(TerritroyReaded orig, TerritroyReaded previus)>(notCalculatedYet);
        }
        return objectsCalculated;
    }


    public IEnumerator MoveEnemyToTerritory(Enemy enemy, Func<Dictionary<TerritroyReaded, TerritroyReaded>, TerritroyReaded> findTerritoryMoveTo)
    {
        var allPaths = Manager.MovementManager.CalculateAllPossible(enemy.Stats.MovementDistance(), enemy);

        TerritroyReaded findTerritory = findTerritoryMoveTo(allPaths);

        // Only move if there's an available territory
        if (findTerritory != null)
        {
            List<Vector3> aktualPath = Manager.MovementManager.CalculateAllPath(findTerritory, enemy, allPaths);

            // Only move if the path exists and contains at least 1 point
            if (aktualPath?.Count > 0)
            {
                enemy.ActualTerritory.TerritoryInfo = TerritoryType.Air;
                yield return Manager.MovementManager.StartCoroutine(enemy.Move(aktualPath));
                enemy.ActualTerritory = findTerritory;
                enemy.ActualTerritory.TerritoryInfo = TerritoryType.Character;
            }
        }
    }

    private void OnStatusChange(HashSet<Permissions> permissions)
    {
        Character character = Manager.TurnManager.SelectedCharacter;
        if (permissions.Contains(Permissions.SelectPlaceToMovement))
        {
            if (character)
            {
                Manager.TurnManager.SelectedCharacter.SelectChanges();
                _lineRenderer.gameObject.SetActive(true);
                _objectsCalculated = CalculateAllPossible(character.Stats.MovementDistance(), character);
                Manager.TurnManager.SelectedCharacter.MoverActive(true);
                AirPlatformsSet(true);
            }
        }
        else
        {
            if (character)
            {
                character.MoverActive(false);
                _lineRenderer.gameObject.SetActive(false);
                AirPlatformsSet(false);
                _objectsCalculated.Clear();
            }
        }
    }

    private void Clear()
    {
        StopAllCoroutines();
        _lineRenderer.gameObject.SetActive(false);
        _lineRenderer.positionCount = 0;

        Manager.TurnManager.DeselectCharacter();
    }
}
