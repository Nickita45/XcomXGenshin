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
    private Sprite _icon;
    public Sprite Icon => _icon;

    [SerializeField]
    private EnemyAI _enemyAI;
    public EnemyAI AI => _enemyAI;

    private bool _triggered;
    public bool Triggered => _triggered;

    public override Transform GetBulletSpawner(string name) => _bulletSpawner.transform;

    public int GetRandomDmg() => UnityEngine.Random.Range(Stats.MinDamage, Stats.MaxDamage + 1);

    public override void Start()
    {
        _countHp = _stats.MaxHP();//set maximum hp
        base.Start();

        ActualTerritory = Manager.Map[transform.localPosition]; //set actual block
        ActualTerritory.TerritoryInfo = TerritoryType.Character; //set actual block type on character tyoe

        Manager.StatusMain.OnStatusChange += OnStatusChange;

        _enemyAI.Init(this);
    }

    // Trigger the enemy if all conditions are met.
    public IEnumerator ConditionalTrigger()
    {
        if (!_triggered)
        {
            // Trigger if any of the characters can see the enemy
            if (Manager.Map.Characters.Any(character => TargetUtils.CanSee(character, this)))
            {
                _triggered = true;

                Character character = GetClosestCharacter();
                ObjectUtils.LookAtXZ(Animator.Model.transform, character.transform.position);

                StartCoroutine(Canvas.PanelShow(Canvas.PanelActionInfo("Triggered!"), 2));
                yield return StartCoroutine(MoveEnemy(_enemyAI.TriggerEnemy));
            }
        }
    }


    public IEnumerator MoveEnemy(Func<Dictionary<TerritroyReaded, TerritroyReaded>, TerritroyReaded> findTerritoryMoveTo)
    {
        yield return StartCoroutine(Manager.MovementManager.MoveEnemyToTerritory(this, findTerritoryMoveTo));
    }

    public IEnumerator MakeTurn()
    {
        // Only triggered enemies make turns
        if (!_triggered) yield break;
        yield return StartCoroutine(_enemyAI.MakeTurn());
    }

    public override void Kill()
    {
        Manager.Map.Enemies.Remove(this); //remove form list of enemies
        _canvas.DisableAll(); //disable all elements from canvas

        _animator.Model.SetActive(false); //disable animator

        GetComponent<BoxCollider>().enabled = false; //disable collider
        ActualTerritory.TerritoryInfo = TerritoryType.Air; //set his block type on air
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    // Gets the character closest to the enemy.
    public Character GetClosestCharacter()
    {
        return Manager.Map.Characters
                .OrderBy(ch => Vector3.Distance(ch.transform.localPosition, transform.localPosition))
                .FirstOrDefault();
    }

    // Gets a list of characters that the enemy can see.
    public List<Character> GetVisibleCharacters()
    {
        // we check whether the enemy can see the character by supplying
        // vision distance to the CanTarget function. This
        // implementation might change in future.
        return Manager.Map.Characters
            .Where(character => TargetUtils.CanSee(this, character))
            .ToList();
    }

    // Gets the character closest to the enemy,
    // chosen out of all characters that the enemy can see.
    public Character GetClosestVisibleCharacter()
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
