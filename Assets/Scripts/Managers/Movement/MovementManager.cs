using MovementAlgorimus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using static UnityEditor.Progress;
using static UnityEngine.EventSystems.EventTrigger;

// Manages UI and game elements related to the movement of the selected character.
public class MovementManager : MonoBehaviour
{
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
            _aktualTerritory.path = MovementCalculationPaths.CalculateAllPath(territory.aktualTerritoryReaded, character, _territoriesCalculated); //calculate actual path to selected territory

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
        var allPaths = MovementCalculationBlocks.CalculateAllPossible(enemy.Stats.MovementDistance(), enemy); // get enemie's objectsCalculated

        TerritroyReaded findTerritory = findTerritoryMoveTo(allPaths); //get target block
        // Only move if there's an available territory
        //if (findTerritory != null) //????? may be problem in future/can be in AI then cant find territory
        {
            List<Vector3> aktualPath = MovementCalculationPaths.CalculateAllPath(findTerritory, enemy, allPaths); //get path with breaks for enemy

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
                _territoriesCalculated = MovementCalculationBlocks.CalculateAllPossible(character.Stats.MovementDistance(), character);
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
                _territoriesCalculated = MovementCalculationBlocks.CalculateAllPossibleInSquare(abilitySummon.RangeSummon(), character);
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
