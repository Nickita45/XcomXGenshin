using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
    private CharacterMovemovent _characterMovemovent;

    [SerializeField]
    private CharacterVisibility _characterVisibility;

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

    public CharacterMovemovent CharacterMovemovent => _characterMovemovent;
    public FreeCameraController CameraController => _freeCameraController;
    public FixedCameraController FixedCameraController => _fixedCameraController;
    public CharacterVisibility CharacterVisibility => _characterVisibility;
    public TurnController TurnController => _turnController;
    public StatusMain StatusMain => _statusMain;
    public EnemyPanel EnemyPanel => _enemyPanel;
    public GameObject MainParent => _mainParent;
    public GameObject GenereteTerritoryMove => _genereteTerritoryMove;

    public MatrixMap Map { get => _map; set => _map = value; }

    public Action OnClearMap;

    [Header("UI")]
    [SerializeField]
    private EnemyPanel _enemyPanel;

    public GunType Gun { get; set; } //in feature we need to move it to character

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
        Instance.CharacterVisibility.OnVisibilityUpdate += UpdateEnemyOutlines;
    }

    public GameObject CreatePlatformMovement(TerritroyReaded item)
    {
        var obj = Instantiate(_prefabPossibleTerritory, Instance.GenereteTerritoryMove.transform);
        obj.transform.localPosition = item.GetCordinats() - CharacterMovemovent.POSITIONFORSPAWN;
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
        UpdateEnemyOutlines(Instance.CharacterVisibility.VisibleEnemies);
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
        if (StatusMain.ActualPermissions.Contains(Permissions.ActionSelect))//(!Instance.CharacterMovemovent.IsMoving)
        {
            _fixedCameraController.ClearListHide();
            CharacterInfo selectedCharacter = Instance.CharacterMovemovent.SelectedCharacter;

            selectedCharacter.GunPrefab.transform.LookAt(icon.Enemy.transform); //gun look to enemy

            (Vector3 position, Quaternion rotation) = CameraUtils.CalculateEnemyView(selectedCharacter.gameObject, icon.Enemy);
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

    public void Attack(Action FinishAbility)
    {
        StatusMain.SetStatusShooting();
        FinishAbility += () =>
        {
            EnableFreeCameraMovement();
            Instance.CharacterMovemovent.SelectedCharacter.CountActions -= 2;
        };

        StartCoroutine(CharacterMovemovent.SelectedCharacter.GetComponent<ShootController>().Shoot(_enemyPanel.EnemyObject.transform,
            Instance.Gun, _enemyPanel.SelectedEnemyProcent, FinishAbility));

    }

    public IEnumerator WaitAndFinish(Action onFinish)
    {
        StatusMain.SetStatusWaiting();
        yield return new WaitForSeconds(2f);
        onFinish();
    }

    public void Overwatch(Action onFinish)
    {
        Debug.Log("Overwatch");
        onFinish += () => { Instance.CharacterMovemovent.SelectedCharacter.CountActions -= 2; };
        StartCoroutine(WaitAndFinish(onFinish));
    }

    public void HunkerDown(Action onFinish)
    {
        Debug.Log("Hunker Down");
        onFinish += () => { Instance.CharacterMovemovent.SelectedCharacter.CountActions -= 2; };
        StartCoroutine(WaitAndFinish(onFinish));
    }
}
