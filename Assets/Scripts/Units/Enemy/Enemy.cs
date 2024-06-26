using FischlWorks_FogWar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : Unit
{
    public new EnemyStats Stats => (EnemyStats)_stats;
    public new EnemyCanvas Canvas => (EnemyCanvas)_canvas;
    public new EnemyAnimator Animator => (EnemyAnimator)_animator;

    public override TerritroyReaded ActualTerritory { get; set; }

    private int _actionsLeft = 2;
    public override int ActionsLeft { get => _actionsLeft; set => _actionsLeft = value; }

    [SerializeField]
    private GameObject _bulletSpawner;
    [SerializeField]
    private csFogVisibilityAgent _fogVisibilityAgent;
    public csFogVisibilityAgent csFogVisibilityAgent => _fogVisibilityAgent;

    [SerializeField]
    private EnemyAI _enemyAI;
    public EnemyAI AI => _enemyAI;

    private bool _triggered;
    public bool Triggered => _triggered;

    public void SetStats(EnemyStats stats)
    {
        _stats = stats;
    }

    public void SetAI(EnemyAI ai)
    {
        _enemyAI = ai;
    }

    public void SetBulletSpawner(GameObject spawner)
    {
        _bulletSpawner = spawner;
    }

    public void SetAnimator(Animator animator)
    {
        _animator.Init(animator);
    }

    public override Transform GetBulletSpawner(string name) => _bulletSpawner.transform;

    public int GetRandomDmg() => UnityEngine.Random.Range(Stats.MinDamage, Stats.MaxDamage + 1);

    public override void Start()
    {
        Resurrect();//set maximum hp
        base.Start();

        ActualTerritory = Manager.Map[transform.localPosition]; //set actual block
        ActualTerritory.TerritoryInfo = TerritoryType.Character; //set actual block type on character tyoe

        Manager.StatusMain.OnStatusChange += OnStatusChange;

        _enemyAI.Init(this);
        _enemyAI.OnSpawn();
    }

    // Trigger the enemy if all conditions are met.
    public IEnumerator ConditionalTrigger()
    {
        if (!_triggered)
        {
            // Trigger if any of the characters can see the enemy
            if (Manager.Map.Characters.GetList.Any(character => TargetUtils.CanSee(this, character)))
            {
                _triggered = true;

                Unit character = GetClosestCharacter();
                ObjectUtils.LookAtXZ(Animator.Model.transform, character.transform.position);

                StartCoroutine(Canvas.PanelShow(Canvas.PanelActionInfo("Triggered!","TriggerEnemy"), 2));
                yield return StartCoroutine(MoveEnemy(_enemyAI.TriggerEnemy));
            }
        }
    }


    public IEnumerator MoveEnemy(Func<Dictionary<TerritroyReaded, TerritroyReaded>, TerritroyReaded> findTerritoryMoveTo)
    {
        yield return StartCoroutine(Manager.MovementManager.MoveEnemyToTerritoryFromSelected(this, findTerritoryMoveTo));
    }

    public IEnumerator MakeTurn()
    {
        // Only triggered enemies make turns
        if (!_triggered) yield break;

        if (_actionsLeft > 0) yield return StartCoroutine(_enemyAI.MakeTurn());
    }

    public override void Kill()
    {
        Manager.StatisticsUtil.SetEnemiesKilledList(Stats.Icon);
            Manager.Map.Enemies.Remove(this); //remove form list of enemies
        foreach (Modifier m in _modifiers.Modifiers) m.DestroyModel(this);
        _canvas.DisableAll(); //disable all elements from canvas
        _actionsLeft = 0;

        _animator.Model.SetActive(false); //disable animator

        GetComponent<BoxCollider>().enabled = false; //disable collider
        ActualTerritory.TerritoryInfo = TerritoryType.Air; //set his block type on air

        Manager.StatisticsUtil.EnemiesDeathCount++;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    // Gets the character closest to the enemy.
    public Unit GetClosestCharacter()
    {
        return Manager.Map.Characters.GetList.Concat(Manager.Map.Entities.GetList.Select(e => (Unit)e))
                .OrderBy(ch => Vector3.Distance(ch.transform.localPosition, transform.localPosition))
                .FirstOrDefault();
    }

    // Gets a list of characters that the enemy can see.
    public List<Unit> GetVisibleCharacters()
    {
        // we check whether the enemy can see the character by supplying
        // vision distance to the CanTarget function. This
        // implementation might change in future.
        return Manager.Map.Characters.GetList.Concat(Manager.Map.Entities.GetList.Select(e => (Unit)e))
            .Where(character => TargetUtils.CanSee(this, character))
            .ToList();
    }

    // Gets the character closest to the enemy,
    // chosen out of all characters that the enemy can see.
    public Unit GetClosestVisibleCharacter()
    {
        return GetVisibleCharacters()
            .OrderBy(n => Vector3.Distance(n.transform.localPosition, transform.localPosition))
            .FirstOrDefault();
    }

    private void OnStatusChange(HashSet<Permissions> permissions)
    {
        if (permissions.Count == 0)
        {
            Manager.StatusMain.OnStatusChange -= OnStatusChange;
            return;
        }
    }
}
