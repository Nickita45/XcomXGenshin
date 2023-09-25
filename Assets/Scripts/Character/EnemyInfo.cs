using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyInfo : PersonInfo
{
    [SerializeField]
    private GameObject _objectModelParent;
    
    [SerializeField]
    private GameObject _bulletSpawner;

    [SerializeField]
    private float _speedCharacter, _visiblityCharacter;
    [SerializeField]
    private int _distanceToMove, _minDmg, _maxDmg;

    private EnemyCanvasController _canvasController;
    public override EnemyCanvasController CanvasController() => _canvasController;


    public bool IsTriggered { get; private set; }
    public override TerritroyReaded ActualTerritory { get; set; }

    private int _countAction = 2;
    public override int CountActions { get => _countAction; set => _countAction = value; }

    private List<CharacterInfo> _characterInfos = new List<CharacterInfo>();
    public override float SpeedCharacter() => _speedCharacter;
    public override int MoveDistanceCharacter() => _distanceToMove;
    public override float VisibilityCharacter() => _visiblityCharacter;

    public int GetRandomDmg() => UnityEngine.Random.Range(_minDmg, _maxDmg + 1);

    public GameObject ObjectModel => _objectModelParent;

    [SerializeField]
    private IEnemyController _enemyController;

    public IEnemyController EnemyController => _enemyController;

    public List<CharacterInfo> VisibleCharacters => _characterInfos;

    public override Transform GetBulletSpawner(string name)
    {
        return _bulletSpawner.transform;
    }

    private void Start()
    {
        _canvasController = GetComponent<EnemyCanvasController>();
        _canvasController.SetStartHealth(_countHp);
        _enemyController = GetComponent<IEnemyController>();

        ActualTerritory = GameManagerMap.Instance.Map[transform.localPosition];
        ActualTerritory.TerritoryInfo = TerritoryType.Character;

        GameManagerMap.Instance.CharacterTargetFinder.OnEnemyUpdate += UpdateVisibility;
        GameManagerMap.Instance.StatusMain.OnStatusChange += OnStatusChange;

    }

    private void UpdateVisibility()
    {
        _characterInfos.Clear();
        _characterInfos = GameManagerMap.Instance.CharacterTargetFinder.UpdateVisibilityForEnemy(this);

        if (_characterInfos.Count > 0 && !IsTriggered)
        {
            if (_enemyController == null) //for test :)
                return;

                _enemyController.OnTriggerCharacter();

                IsTriggered = true;
        }
    }

    private void OnStatusChange(HashSet<Permissions> permissions)
    {
        if (permissions.Count == 0)
        {
            GameManagerMap.Instance.StatusMain.OnStatusChange -= OnStatusChange;
            GameManagerMap.Instance.CharacterTargetFinder.OnEnemyUpdate -= UpdateVisibility;

            return;
        }
    }



    protected override void KillPerson()
    {
        GameManagerMap.Instance.Map.Enemy.Remove(gameObject);
        GameManagerMap.Instance.TurnController.OnEnemyEndMakeAction(this);
        _canvasController.DisableAll();
        if (_objectModelParent == null)
            GetComponent<MeshRenderer>().enabled = false;
        else
            _objectModelParent.SetActive(false);
        GetComponent<BoxCollider>().enabled = false;
        ActualTerritory.TerritoryInfo = TerritoryType.Air;
    }


}
