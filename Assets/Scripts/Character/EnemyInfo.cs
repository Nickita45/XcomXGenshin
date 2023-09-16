using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyInfo : PersonInfo
{
    [SerializeField]
    private GameObject _objectModelParent;

    private EnemyCanvasController _canvasController;
    public override EnemyCanvasController CanvasController() => _canvasController;


    public bool IsTriggered { get; private set; }
    public override TerritroyReaded ActualTerritory { get; set; }

    private int _countAction = 2;
    public override int CountActions { get => _countAction; set => _countAction = value; }

    private List<CharacterInfo> _characterInfos = new List<CharacterInfo>();
    public override float SpeedCharacter() => 3;
    public override int MoveDistanceCharacter() => 5;
    public override float VisibilityCharacter() => 10;

    public int GetRandomDmg() => UnityEngine.Random.Range(1, 3 + 1);

    public GameObject ObjectModel => _objectModelParent;

    [SerializeField]
    private EnemyController _enemyController;

    public EnemyController EnemyController => _enemyController;
    public CharacterInfo GetFirstPerson()
    {
        if (_characterInfos.Count > 0)
            return _characterInfos.OrderBy(n => Vector3.Distance(n.gameObject.transform.localPosition, gameObject.transform.localPosition)).First();
        else
            return null;
    }

    private void Start()
    {
        _canvasController = GetComponent<EnemyCanvasController>();
        _canvasController.SetStartHealth(_countHp);

        ActualTerritory = GameManagerMap.Instance.Map[transform.localPosition];
        ActualTerritory.TerritoryInfo = TerritoryType.Character;

        GameManagerMap.Instance.CharacterTargetFinder.OnVisibilityEnemyUpdate += UpdateVisibility;
        GameManagerMap.Instance.StatusMain.OnStatusChange += OnStatusChange;

    }

    private void UpdateVisibility()
    {
        _characterInfos.Clear();
        _characterInfos = GameManagerMap.Instance.CharacterTargetFinder.UpdateVisibilityForEnemy(this);

        if (_characterInfos.Count > 0 && !IsTriggered)
        {
            _enemyController.OnTriggerCharacter();

            if (GetComponent<TerritoryInfo>().Path.Contains("Slime")) //for test :)
                IsTriggered = true;
        }
    }

    private void OnStatusChange(HashSet<Permissions> permissions)
    {
        if (permissions.Count == 0)
        {
            GameManagerMap.Instance.StatusMain.OnStatusChange -= OnStatusChange;
            GameManagerMap.Instance.CharacterTargetFinder.OnVisibilityEnemyUpdate -= UpdateVisibility;

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
