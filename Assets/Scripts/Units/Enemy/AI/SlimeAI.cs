using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class SlimeAI : EnemyAI
{
    private AbilityMeleeAttack _attack = new();

    public override IEnumerator MakeTurn()
    {
        var character = _enemy.GetClosestVisibleCharacter();
        if (character != null && Vector3.Distance(character.transform.localPosition, _enemy.transform.localPosition) < 2)
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

        if (_enemy.ActionsLeft > 0) yield return StartCoroutine(MakeTurn());
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