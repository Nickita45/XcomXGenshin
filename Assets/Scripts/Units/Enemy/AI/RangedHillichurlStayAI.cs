using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Enemies.AI
{
    public class RangedHillichurlStayAI : EnemyAI
    {
        [SerializeField]
        private Element _element = Element.Physical;

        private AbilityShoot _shoot;
        private AbilityOverwatch _overwatch = new();

        public override void Init(Enemy enemy)
        {
            base.Init(enemy);
            _shoot = new AbilityShoot(_element);
        }

        public override IEnumerator Attack(Unit character)
        {
            yield return _shoot.Activate(_enemy, character);
            yield return new WaitForSeconds(2);
        }

        public override IEnumerator MakeTurn()
        {
            var characters = _enemy.GetVisibleCharacters();
            if (characters.Count > 0)
            {
                if (characters.Where(n => n is Character).Select(n => (Character)n).Select(ch =>
                            AimUtils.CalculateHitChance(_enemy.ActualTerritory, ch.ActualTerritory, ch.Stats.Weapon, _enemy.Stats.BaseAimPercent()).percent).Max() < 40)
                {
                    //make overwatch if has hit chance less then 40
                    _enemy.ActionsLeft -= 2;
                    StartCoroutine(_enemy.Canvas.PanelShow(_enemy.Canvas.PanelActionInfo(_overwatch.AbilityName, "Overwatch"), 2));
                    yield return StartCoroutine(_overwatch.Activate(_enemy, null));
                }
                else
                {
                    //make shoot if it is above 40 percent chance
                    _enemy.ActionsLeft -= 2;
                    yield return StartCoroutine(Attack(characters.Where(n => n is Character).Select(n => (Character)n).OrderByDescending(ch =>
                    AimUtils.CalculateHitChance(_enemy.ActualTerritory, ch.ActualTerritory, ch.Stats.Weapon, ch.Stats.BaseAimPercent()).percent).First())); //slow?
                    Manager.StatusMain.SetStatusWaiting();
                }
            }
            else
            {
                _enemy.ActionsLeft -= 2;
                StartCoroutine(_enemy.Canvas.PanelShow(_enemy.Canvas.PanelActionInfo(_overwatch.AbilityName, "Overwatch"), 2));
                yield return StartCoroutine(_overwatch.Activate(_enemy, null));
            }
        }

        public override TerritroyReaded TriggerEnemy(Dictionary<TerritroyReaded, TerritroyReaded> allPaths)
        {
            StartCoroutine(_enemy.Canvas.PanelShow(_enemy.Canvas.PanelActionInfo(_overwatch.AbilityName, "Overwatch"), 2));
            StartCoroutine(_overwatch.Activate(_enemy, null));
            return _enemy.ActualTerritory;
        }
    }
}
