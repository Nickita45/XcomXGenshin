using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigSlimeAI : EnemyAI
{
    [SerializeField]
    private Element _element;

    private AbilityMeleeAttack _attack;
    private AbilityBigSlimeJump _jump;

    void Start()
    {
        _attack = new AbilityMeleeAttack(_element);
        _jump = new AbilityBigSlimeJump(_element);
    }

    // The slime's element is applied to it every turn
    private void ApplyElementToSelf()
    {
        _enemy.Health.MakeHit(0, _element, _enemy);
    }

    public override void OnSpawn()
    {
        ApplyElementToSelf();
    }

    public override int OnResistance(int hit, Element element)
    {
        if (element == _element)
            return 0;
        else return hit;
    }

    public override IEnumerator MakeTurn()
    {
        // Only apply element at the start of the turn (when actions are full)
        _jump.ActualCooldown--;
        if (_enemy.ActionsLeft == _enemy.Stats.BaseActions() && !_enemy.Modifiers.GetElements().Contains(_element)) ApplyElementToSelf();
        var character = _enemy.GetClosestVisibleCharacter();
        if(_jump.InAir)
        {
            _enemy.ActionsLeft -= _jump.ActionCost;
            yield return StartCoroutine(_jump.Activate(_enemy, character));
            yield break;
        }

        if (character != null && Vector3.Distance(character.transform.localPosition, _enemy.transform.localPosition) < 2) //if enemy is on neighbourhood block 
        {
            _enemy.ActionsLeft -= _attack.ActionCost;
            yield return StartCoroutine(Attack(character));
        }
        else
        {
            if (character != null)
            {
                if (_jump.IsAvailable &&
                     RandomExtensions.GetChance(50)) //Vector3.Distance(character.transform.localPosition, _enemy.transform.localPosition) > _enemy.Stats.MovementDistance() &&
                {
                    _enemy.ActionsLeft -= _jump.ActionCost;
                    yield return StartCoroutine(_jump.Activate(_enemy, character));
                } else
                {
                    _enemy.ActionsLeft -= 1;
                    yield return StartCoroutine(_enemy.MoveEnemy(FindTerritoryToCharacter));
                }
                
            }
            else 
            {
                _enemy.ActionsLeft -= 1;
                yield return StartCoroutine(_enemy.MoveEnemy(FindTerritoryToCharacter));
            }
        }
    }

    public override IEnumerator Attack(Unit character)
    {
        yield return _attack.Activate(_enemy, character);
        yield return new WaitForSeconds(2);
    }
    public override TerritroyReaded TriggerEnemy(Dictionary<TerritroyReaded, TerritroyReaded> allPaths)
    {
        return FindTerritoryToCharacter(allPaths);
    }
}
