using System;
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
    private FreeCameraController _cameraController;

    [SerializeField]
    private FixedCameraController _fixedCameraController;

    [SerializeField]
    private CharacterVisibility _characterVisibility;

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
    public FreeCameraController CameraController => _cameraController;
    public FixedCameraController FixedCameraController => _fixedCameraController;
    public CharacterVisibility CharacterVisibility => _characterVisibility;
    public StatusMain StatusMain => _statusMain;
    public EnemyUI EnemyUI => _enemyUI;
    public GameObject MainParent => _mainParent;
    public GameObject GenereteTerritoryMove => _genereteTerritoryMove;

    public MatrixMap Map { get => _map; set => _map = value; }

    public Action OnClearMap;

    private EnemyUI _enemyUI;
    //[SerializeField]
    //private GameObject _disableInteraction;

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
        _enemyUI = FindObjectOfType<EnemyUI>();
    }

    public GameObject CreatePlatformMovement(TerritroyReaded item)
    {
        var obj = Instantiate(_prefabPossibleTerritory, GameManagerMap.Instance.GenereteTerritoryMove.transform);
        obj.transform.localPosition = item.GetCordinats() - CharacterMovemovent.POSITIONFORSPAWN;
        obj.SetActive(false);
        return obj;
    }

    private void ClearMap()
    {
        Map = null;
        DeleteAllChildren(MainParent);
        DeleteAllChildren(GenereteTerritoryMove);
    }

    private static void DeleteAllChildren(GameObject parent)
    {
        int childCount = parent.transform.childCount;

        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = parent.transform.GetChild(i);
            DestroyImmediate(child.gameObject);
        }
    }

    /*private void SetState(GameState state)
    {
        // Exit current state if any
        switch (_state)
        {
            case GameState.FreeMovement:
                _characterMovemovent.AirPlatformsSet(false);
                _characterMovemovent.LineRendererSet(false);
                break;
            case GameState.ViewEnemy:
                _enemyUI.Exit();
                _disableInteraction.SetActive(false);
                break;
        }

        _state = state;
    }*/

    public void FreeMovement()
    {
        StatusMain.SetStatusSelectAction();

        _cameraController.TeleportToSelectedCharacter();
        _cameraController.Init();
        _fixedCameraController.ClearListHide();

        _characterVisibility.UpdateVisibility(_characterMovemovent.SelectedCharacter);
    }

    public void ViewEnemy(EnemyIcon icon)
    {
        if (StatusMain.ActualPermissions.Contains(Permissions.ActionSelect))//(!Instance.CharacterMovemovent.IsMoving)
        {
            //SetState(GameState.ViewEnemy);
            _fixedCameraController.ClearListHide();
            _enemyUI.SelectEnemy(icon);
            StatusMain.SetStatusSelectEnemy();


            CharacterInfo selectedCharacter = Instance.CharacterMovemovent.SelectedCharacter;

            selectedCharacter.GunPrefab.transform.LookAt(icon.Enemy.transform); //gun look to enemy

            (Vector3 position, Quaternion rotation) = CameraUtils.CalculateEnemyView(selectedCharacter.gameObject, icon.Enemy);
            _fixedCameraController.Init(position, rotation, 0.5f, _fixedCameraController.FinishingDetect);

            //_disableInteraction.SetActive(true);

            foreach (GameObject e in Instance.Map.Enemy)
            {
                e.GetComponent<Outline>().enabled = e == icon.Enemy;
            }
        }
    }

    public void ButtonFire()
    {
        StatusMain.SetStatusShooting();
        StartCoroutine(CharacterMovemovent.SelectedCharacter.GetComponent<ShootController>().Shoot(_enemyUI.EnemyObject.transform,
            GameManagerMap.Instance.Gun, _enemyUI.SelectedEnemyProcent, EndFire));
    }

    private void EndFire()
    {
        CharacterMovemovent.SelectedCharacter = null;
        FreeMovement();
        StatusMain.SetStatusSelectCharacter();
    }

    private void Update()
    {
        if(StatusMain.ActualPermissions.Contains(Permissions.SelectEnemy) && Input.GetKeyDown(KeyCode.Escape)) //Down works once
        {
            FreeMovement();
        }

        
        /*switch (_state)
        {
            case GameState.FreeMovement:
                break;
            case GameState.ViewEnemy:
                // Exit the viewing mode
                if (Input.GetKeyDown(KeyCode.Escape)) FreeMovement();
                break;
        }*/
    }

    public void Attack()
    {
        Debug.Log("Attack");
    }

    public void Overwatch()
    {
        Debug.Log("Overwatch");
    }

    public void HunkerDown()
    {
        Debug.Log("Hunker Down");
    }
}
