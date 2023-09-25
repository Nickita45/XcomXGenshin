using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

public class EnemyHilichurlRangerController : EnemyController, IEnemyController
{
    private Func<Dictionary<TerritroyReaded, TerritroyReaded>, TerritroyReaded> findSaveTerritory;
    private Func<Dictionary<TerritroyReaded, TerritroyReaded>, TerritroyReaded> findNewRandomPosition;

    protected override void Start()
    {
        base.Start();

        findSaveTerritory = (allPaths) =>
        {
            (TerritroyReaded territory, float procent) minimum = (null, Int32.MinValue);
            foreach(var item in allPaths)
            {
                int procGetHit = _enemyInfo.VisibleCharacters.Sum(ch => AimCalculater.CalculateShelterPercent(item.Key, ch.ActualTerritory, ch.WeaponCharacter, ch.BaseAimCharacter).percent);
                int procMakeHit = _enemyInfo.VisibleCharacters.Sum(ch => AimCalculater.CalculateShelterPercent(ch.ActualTerritory, item.Key, GunType.Automatic, 50).percent); //???

                float proc = (2f * (100 - procGetHit) + 0.4f * procMakeHit) / (2f + 0.4f);

                if (proc > minimum.procent)
                    minimum = (item.Key, proc);

                //Debug.Log($"get hit procnet: {procGetHit}; make hit proc:{procMakeHit}; proc:{proc}; ter {item.Key}; count vis {string.Join(",", _enemyInfo.VisibleCharacters.Select(n => n.NameCharacter()))}");
            }
            Debug.Log(minimum.procent + " " + allPaths.Count());
            return minimum.territory;
        };

        findNewRandomPosition = (allPaths) =>
        {
            List<TerritroyReaded> keysList = new List<TerritroyReaded>(allPaths.Keys.Where(n => n != _enemyInfo.ActualTerritory && n.IsNearShelter()));
            Debug.Log("Random move " + nameof(EnemyHilichurlRangerController));
            return keysList[UnityEngine.Random.Range(0, keysList.Count())];
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
        StartCoroutine(MoveEnemy(finishAbility, findSaveTerritory));
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
        if (person != null)
        {
            if (_enemyInfo.CountActions == 2 && _enemyInfo.VisibleCharacters.Select(ch =>
                        AimCalculater.CalculateShelterPercent(_enemyInfo.ActualTerritory, ch.ActualTerritory, ch.WeaponCharacter, ch.BaseAimCharacter).percent).Max() > 50)
            {
                _enemyInfo.CountActions -= 1;
                StartCoroutine(MoveEnemy(finalAction, findSaveTerritory));
            } else {
                _enemyInfo.CountActions -= 2;
                StartCoroutine(MakeAttack(finalAction, person));
            }
        }
        else
        {
            _enemyInfo.CountActions -= 1;

            if (_enemyInfo.CountActions == 0 && UnityEngine.Random.Range(1, 100 + 1) > 75)
                StartCoroutine(MakeOverwatch(finalAction));
            else
                StartCoroutine(MoveEnemy(finalAction, findTerritoryMoveTo: findNewRandomPosition));
        }
    }

    public IEnumerator MakeOverwatch(Action onFinsh)
    {
        Debug.Log("Enemy overwatch");
        _enemyInfo.CountActions -= 2;
        StartCoroutine(_enemyInfo.CanvasController().PanelShow(_enemyInfo.CanvasController().PanelActionInfo("Overwatch"), 4));
        yield return new WaitForSeconds(2);
        onFinsh();
    }

    public IEnumerator MoveEnemy(Action finalAction, Func<Dictionary<TerritroyReaded, TerritroyReaded>, TerritroyReaded> findTerritoryMoveTo)
    {
        var character = GetFirstPerson();

        yield return StartCoroutine(GameManagerMap.Instance.CharacterMovement.MoveEnemyToTerritory(_enemyInfo, findTerritoryMoveTo));

        if (character != null)
            _enemyInfo.ObjectModel.transform.LookAt(character.transform);

        finalAction?.Invoke();
    }

    public IEnumerator MakeAttack(Action finalAction, CharacterInfo character)
    {
        var percent = AimCalculater.CalculateShelterPercent(character.ActualTerritory, _enemyInfo.ActualTerritory, GunType.Automatic, 50).percent;

        yield return StartCoroutine(GameManagerMap.Instance.ShootController.Shoot(GetFirstPerson(),
            GunType.Automatic, percent, null, _enemyInfo));

        finalAction();
    }

    

}
