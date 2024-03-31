using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class SlimeAI : EnemyAI
{
    [SerializeField]
    private Element _element;

    private AbilityMeleeAttack _attack;

    void Start()
    {
        _attack = new AbilityMeleeAttack(_element);
    }

    // The slime's element is applied to it every turn
    private void ApplyElementToSelf()
    {
        _enemy.MakeHit(0, _element, _enemy);
    }

    public override void OnSpawn()
    {
        ApplyElementToSelf();
    }

    public override IEnumerator MakeTurn()
    {
        // Only apply element at the start of the turn (when actions are full)
        if (_enemy.ActionsLeft == _enemy.Stats.BaseActions()) ApplyElementToSelf();
        var character = _enemy.GetClosestVisibleCharacter();
        if (character != null && Vector3.Distance(character.transform.localPosition, _enemy.transform.localPosition) < 2) //if enemy is on neighbourhood block 
        {
            _enemy.ActionsLeft -= 2;
            yield return StartCoroutine(Attack(character));

            // 50 % chance to run after attack
            if (UnityEngine.Random.Range(0, 2) == 0)
            {
                yield return StartCoroutine(
                    _enemy.MoveEnemy(FindTerritoryFromCharacter)
                );
            }
        }
        else
        {
            _enemy.ActionsLeft -= 1;
            yield return StartCoroutine(_enemy.MoveEnemy(FindTerritoryToCharacter));
        }

       // if (_enemy.ActionsLeft > 0) yield return StartCoroutine(MakeTurn());
    }

    public override IEnumerator Attack(Character character)
    {
        yield return _attack.Activate(_enemy, character);
        yield return new WaitForSeconds(2);
    }
    public override TerritroyReaded TriggerEnemy(Dictionary<TerritroyReaded, TerritroyReaded> allPaths)
    {
        return FindTerritoryToCharacter(allPaths);
    }
}