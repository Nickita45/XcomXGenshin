using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class EnemySlimeController : EnemyController, IEnemyController
{
    private Func<Dictionary<TerritroyReaded, TerritroyReaded>, TerritroyReaded> findTerritoryToCharacter;
    private Func<Dictionary<TerritroyReaded, TerritroyReaded>, TerritroyReaded> findTerritoryTheFarFromCharacter;

    protected override void Start()
    {
        base.Start();

        findTerritoryToCharacter = (allPaths) =>
        {
            var character = GetFirstPerson();

            if (character != null)
                return allPaths.Keys.OrderBy(n => Vector3.Distance(character.transform.localPosition, n.GetCordinats())).Where(n => n != _enemyInfo.ActualTerritory).First();
            else
            {
                List<TerritroyReaded> keysList = new List<TerritroyReaded>(allPaths.Keys.Where(n => n != _enemyInfo.ActualTerritory));
                Debug.Log("Random move");
                return keysList[UnityEngine.Random.Range(0, keysList.Count())];
            }
        };

        findTerritoryTheFarFromCharacter = (allPaths) =>
        {
            var character = GetFirstPerson();
            Debug.Log("RUUUNN");
            return allPaths.Keys.OrderBy(n => Vector3.Distance(character.transform.localPosition, n.GetCordinats())).Last(n => n != _enemyInfo.ActualTerritory);
        };
    }

    public void OnTriggerCharacter()
    {
        _enemyInfo.ObjectModel.transform.LookAt(GetFirstPerson().transform);
        GameManagerMap.Instance.TurnController.AddEnemyToTriggerList(_enemyInfo);
    }

    public void OnTriggerMakeAction()
    {
        Action finishAbility = () =>
        {
            GameManagerMap.Instance.TurnController.OnTriggerEndMakeAction(_enemyInfo);
        };
        StartCoroutine(MoveEnemy(finishAbility, findTerritoryToCharacter));
    }

    public void MakeAction(Action finalAction)
    {
        if (_enemyInfo.IsTriggered == false)
            return;

        if (finalAction == null)
        {
            finalAction = () =>
            {
                GameManagerMap.Instance.StatusMain.SetStatusWaiting();
                if (_enemyInfo.CountActions > 0)
                    MakeAction(finalAction);
                else
                    GameManagerMap.Instance.TurnController.OnEnemyEndMakeAction(_enemyInfo);
            };
        }
        var person = GetFirstPerson();
        if (person != null && Vector3.Distance(person.transform.localPosition, gameObject.transform.localPosition) < 2)
        {
            _enemyInfo.CountActions -= 2;
            StartCoroutine(MakeAttack(finalAction, person));
        }
        else
        {
            _enemyInfo.CountActions -= 1;


            StartCoroutine(MoveEnemy(finalAction, findTerritoryMoveTo: findTerritoryToCharacter));
        }
    }

    public IEnumerator MoveEnemy(Action finalAction, Func<Dictionary<TerritroyReaded, TerritroyReaded>, TerritroyReaded> findTerritoryMoveTo)
    {
        var character = GetFirstPerson();

        yield return StartCoroutine(GameManagerMap.Instance.CharacterMovement.MoveEnemyToTerritory(_enemyInfo, findTerritoryMoveTo));

        if (character != null)
            _enemyInfo.ObjectModel.transform.LookAt(character.transform);
        GameManagerMap.Instance.CharacterTargetFinder.OnEnemyUpdate();
        finalAction?.Invoke();
    }

    public IEnumerator MakeAttack(Action finalAction, CharacterInfo character)
    {
        if (UnityEngine.Random.Range(1, 100 + 1) > 50)
            StartCoroutine(character.CanvasController().PanelShow(character.CanvasController().PanelMiss));
        else
        {
            int dmg = _enemyInfo.GetRandomDmg();
            StartCoroutine(character.CanvasController().PanelShow(character.CanvasController().PanelHit(dmg), 4));
            character.MakeHit(dmg);
        }

        yield return new WaitForSeconds(2);

        if (UnityEngine.Random.Range(0, 1 + 1) > 0)
        {
            yield return StartCoroutine(MoveEnemy(null, findTerritoryMoveTo: findTerritoryTheFarFromCharacter));
        }

        finalAction();
    }
}
