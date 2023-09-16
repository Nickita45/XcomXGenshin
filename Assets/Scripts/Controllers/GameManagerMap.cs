using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerMap : MonoBehaviour
{
    [Header("Serialize elemnets")]
    [SerializeField]
    private DeReadingMap _deReadingMap;

    public DeReadingMap DeReadingMap => _deReadingMap;

    [Header("Game elements")]
    [SerializeField]
    private MatrixMap _map;

    [SerializeField]
    private CharacterMovement _characterMovement;

    [SerializeField]
    private CharacterTargetFinder _characterTargetFinder;

    [SerializeField]
    private FreeCameraController _freeCameraController;

    [SerializeField]
    private FixedCameraController _fixedCameraController;

    [SerializeField]
    private TurnController _turnController;

    [Header("MainObjects")]
    [SerializeField]
    private GameObject _mainParent;
    [SerializeField]
    private GameObject _genereteTerritoryMove;
    [SerializeField]
    private StatusMain _statusMain;

    [Header("Plane to movement")]
    [SerializeField]
    private GameObject _prefabPossibleTerritory;

    private static GameManagerMap _instance;
    public static GameManagerMap Instance => _instance;

    public CharacterMovement CharacterMovement => _characterMovement;
    public FreeCameraController FreeCameraController => _freeCameraController;
    public FixedCameraController FixedCameraController => _fixedCameraController;
    public CharacterTargetFinder CharacterTargetFinder => _characterTargetFinder;
    public TurnController TurnController => _turnController;
    public StatusMain StatusMain => _statusMain;
    public EnemyPanel EnemyPanel => _enemyPanel;
    public GameObject MainParent => _mainParent;
    public GameObject GenereteTerritoryMove => _genereteTerritoryMove;
    public UIIcons UIIcons => _uiIcons;

    public MatrixMap Map { get => _map; set => _map = value; }

    public Action OnClearMap;

    [Header("UI")]
    [SerializeField]
    private EnemyPanel _enemyPanel;

    public GunType Gun { get; set; } //in future we need to move it to character

    [Header("UI icons")]
    [SerializeField]
    private UIIcons _uiIcons;

    private float _secondsTimerTurnCharacter = 2f;

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
        Instance.CharacterTargetFinder.OnUpdate += UpdateEnemyOutlines;

        _secondsTimerTurnCharacter = ConfigurationManager.Instance.GlobalDataJson.secondsTimerTurnCharacter;
    }

    public GameObject CreatePlatformMovement(TerritroyReaded item)
    {
        var obj = Instantiate(_prefabPossibleTerritory, Instance.GenereteTerritoryMove.transform);
        obj.transform.localPosition = item.GetCordinats() - CharacterMovement.POSITIONFORSPAWN;
        obj.SetActive(false);
        return obj;
    }

    private void ClearMap()
    {
        Map = null;
        ObjectHelpers.DestroyAllChildrenImmediate(MainParent);
        ObjectHelpers.DestroyAllChildrenImmediate(GenereteTerritoryMove);
    }

    // Enables the free camera, which allows the player to navigate around the map.
    public void EnableFreeCameraMovement()
    {
        UpdateEnemyOutlines(Instance.CharacterTargetFinder.AvailableTargets);
        _freeCameraController.InitAsMainCamera();

        // If coming from enemy selection or shooting, perform additional setup
        if (StatusMain.ActualPermissions.Contains(Permissions.SelectEnemy)
            || StatusMain.ActualPermissions.Contains(Permissions.AnimationShooting))
        {
            _enemyPanel.ClearSelection();
            _freeCameraController.TeleportToSelectedCharacter();
        }

        StatusMain.SetStatusSelectAction();
    }

    // Transitions the camera to look at the selected enemy.
    // If none is selected, this also selects the last enemy.
    //
    // Used while selecting the enemy target for an ability.
    public void FixCameraOnSelectedEnemy()
    {
        Instance.FixCameraOnEnemy(_enemyPanel.GetSelectedIconOrLast());
    }

    // Transitions the camera to look at the given enemy.
    //
    // Used while selecting the enemy target for an ability.
    public void FixCameraOnEnemy(EnemyIcon icon)
    {
        if (StatusMain.ActualPermissions.Contains(Permissions.ActionSelect))//(!Instance.CharacterMovement.IsMoving)
        {
            _fixedCameraController.ClearListHide();
            CharacterInfo selectedCharacter = Instance.CharacterMovement.SelectedCharacter;

            (Vector3 position, Quaternion rotation) = CameraHelpers.CalculateEnemyView(selectedCharacter.gameObject, icon.Enemy);
            _fixedCameraController.InitAsMainCamera(position, rotation, 0.5f);

            foreach (GameObject e in Instance.Map.Enemy)
            {
                e.GetComponent<Outline>().enabled = e == icon.Enemy;
            }

            StatusMain.SetStatusSelectEnemy();
        }
    }

    // Enables an outline for each visible enemy, disables for others.
    private void UpdateEnemyOutlines(HashSet<GameObject> visibleEnemies)
    {
        foreach (GameObject enemy in Instance.Map.Enemy)
        {
            enemy.GetComponent<Outline>().enabled = visibleEnemies.Contains(enemy.gameObject);
        }
    }

    public void Attack(Action onFinish)
    {
        StatusMain.SetStatusShooting();
        onFinish += () =>
        {
            EnableFreeCameraMovement();
            Instance.CharacterMovement.SelectedCharacter.CountActions -= 2;
        };

        StartCoroutine(AttackCoroutine(onFinish));
    }

    public IEnumerator AttackCoroutine(Action onFinish)
    {
        Vector3 directionToTarget = _enemyPanel.EnemyObject.transform.position - CharacterMovement.SelectedCharacter.transform.position;
        directionToTarget.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        CharacterAnimation animation = CharacterMovement.SelectedCharacter.Animation;
        ShootController shootController = CharacterMovement.SelectedCharacter.GetComponent<ShootController>();

        // Setup shooting animation
        yield return StartCoroutine(animation.CrouchRotate(targetRotation));
        yield return StartCoroutine(animation.StopCrouching());
        yield return StartCoroutine(animation.Shoot());

        // Shoot
        yield return StartCoroutine(shootController.Shoot(_enemyPanel.EnemyObject.transform,
            Instance.CharacterMovement.SelectedCharacter.WeaponCharacter, _enemyPanel.SelectedEnemyPercent));

        // Setup idle crouching animation
        yield return StartCoroutine(animation.StopShooting());
        yield return StartCoroutine(animation.Crouch());
        yield return StartCoroutine(CharacterMovement.CrouchRotateCharacterNearShelter(Instance.CharacterMovement.SelectedCharacter));

        onFinish();
    }

    public IEnumerator WaitAndFinish(Action onFinish)
    {
        StatusMain.SetStatusWaiting();
        yield return new WaitForSeconds(_secondsTimerTurnCharacter);
        onFinish();
    }

    public void Overwatch(Action onFinish)
    {
        Debug.Log("Overwatch");
        onFinish += () => { Instance.CharacterMovement.SelectedCharacter.CountActions -= 2; };
        StartCoroutine(Instance.CharacterMovement.SelectedCharacter.CanvasController().PanelShow(Instance.CharacterMovement.SelectedCharacter.CanvasController().PanelActionInfo("Overwatch"), 4));
        StartCoroutine(WaitAndFinish(onFinish));
    }

    public void HunkerDown(Action onFinish)
    {
        Debug.Log("Hunker Down");
        onFinish += () => { Instance.CharacterMovement.SelectedCharacter.CountActions -= 2; };
        StartCoroutine(Instance.CharacterMovement.SelectedCharacter.CanvasController().PanelShow(Instance.CharacterMovement.SelectedCharacter.CanvasController().PanelActionInfo("Hunker Down"), 4));
        StartCoroutine(WaitAndFinish(onFinish));
    }
}
