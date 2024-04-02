using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The main class of the game.
//
// Contains some of the global data, as well as references to other
// managers.
public class Manager : MonoBehaviour
{
    [Header("Serialize elements")]
    [SerializeField]
    private DeReadingMap _deReadingMap;
    public static DeReadingMap DeReadingMap => Instance._deReadingMap;

    [Header("Game elements")]
    [SerializeField]
    private MatrixMap _map;
    public static MatrixMap Map { get => Instance._map; set => Instance._map = value; }

    [SerializeField]
    private MovementManager _movementManager;
    public static MovementManager MovementManager => Instance._movementManager;


    [SerializeField]
    private TargetSelectManager _targetSelectManager;
    public static TargetSelectManager TargetSelectManager => Instance._targetSelectManager;

    [SerializeField]
    private ShootManager _shootManager;
    public static ShootManager ShootManager => Instance._shootManager;

    [SerializeField]
    private CameraManager _cameraManager;
    public static CameraManager CameraManager => Instance._cameraManager;

    [SerializeField]
    private CameraObjectTransparency _cameraObjectTransparency;
    public static CameraObjectTransparency CameraObjectTransparency => Instance._cameraObjectTransparency;

    [SerializeField]
    private TurnManager _turnManager;
    public static TurnManager TurnManager => Instance._turnManager;

    [SerializeField]
    private OutlineManager _outlineManager;
    public static OutlineManager OutlineManager => Instance._outlineManager;

    [Header("MainObjects")]
    [SerializeField]
    private GameObject _mainParent;
    public static GameObject MainParent => Instance._mainParent;

    [SerializeField]
        private GameObject _generateTerritoryMove;
    public static GameObject GenerateTerritoryMove => Instance._generateTerritoryMove;

    [SerializeField]
    private StatusMain _statusMain;
    public static StatusMain StatusMain => Instance._statusMain;

    [Header("Plane to movement")]
    [SerializeField]
    private GameObject _prefabPossibleTerritory;

    [Header("UI")]
    [SerializeField]
    private EnemyTargetPanel _enemyPanel;
    public static EnemyTargetPanel EnemyPanel => Instance._enemyPanel;

    [SerializeField]
    private AbilityPanel _abilityPanel;
    public static AbilityPanel AbilityPanel => Instance._abilityPanel;

    [SerializeField]
    private UnitInfoDialog _unitInfoDialog;
    public static UnitInfoDialog UnitInfoDialog => Instance._unitInfoDialog;

    [SerializeField]
    private UIIcons _uiIcons;
    public static UIIcons UIIcons => Instance._uiIcons;

    public Action OnClearMap;

    private static Manager _instance;
    public static Manager Instance => _instance;

    public StatisticsUtil StatisticsUtil {get;private set;}
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    private void Start()
    {
        OnClearMap += ClearMap;
    }

    private void ClearMap()
    {
        Map = null;
        ObjectUtils.DestroyAllChildrenImmediate(MainParent);
        ObjectUtils.DestroyAllChildrenImmediate(GenerateTerritoryMove);
        StatisticsUtil = new StatisticsUtil();
    }

    public GameObject CreatePlatformMovement(TerritroyReaded item)
    {
        var obj = Instantiate(_prefabPossibleTerritory, _generateTerritoryMove.transform);
        obj.transform.localPosition = item.GetCordinats() - MovementManager.POSITIONFORSPAWN;
        obj.SetActive(false);
        return obj;
    }

    // Activates the given ability for the selected character.
    public void CharacterActivateAbility(AbilityIcon icon)
    {
        IEnumerator ActivateCoroutine(AbilityIcon icon)
        {
            // Wait until the interaction is finished
            StatusMain.SetStatusWaiting();

            OutlineManager.ClearTargets();

            Character character = _turnManager.SelectedCharacter;

            // Show info panel for self-targeting abilities, e.g. Overwatch
            CharacterCanvas canvas = character.Canvas;
            if (icon.Ability.TargetType == TargetType.Self)
                StartCoroutine(canvas.PanelShow(canvas.PanelActionInfo(icon.Ability.AbilityName, icon.Ability.Icon), 4));

            // Start the interaction coroutine
            yield return StartCoroutine(icon.Ability.Activate(character, icon.Target));
            character.ActionsLeft -= icon.Ability.ActionCost;

            icon.ExitTargetMode();

            // Run the AfterCharacterAction Coroutine, which
            // might end the turn if there are no actions left
            yield return TurnManager.AfterCharacterAction();
        }

        StartCoroutine(ActivateCoroutine(icon));
    }

    // A short utility function to check whether a certain permission is contained in the current status.
    public static bool HasPermission(Permissions permission) => Instance._statusMain.ActualPermissions.Contains(permission);
}
