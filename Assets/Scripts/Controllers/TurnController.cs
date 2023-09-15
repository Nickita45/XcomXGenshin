using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class TurnController : MonoBehaviour
{
    [SerializeField]
    private MenuController menuController;

    private List<CharacterInfo> _characters = new List<CharacterInfo>();
    private List<EnemyInfo> _enemiesTriggered = new List<EnemyInfo>();
    private List<EnemyInfo> _enemiesMakesTurn = new List<EnemyInfo>();
    private int _iterator = 0;

    public Action CharacterBegining;
    private float _secondsEndTurn = 3f;

    private void Start()
    {
        GameManagerMap.Instance.OnClearMap += Clear;

        _secondsEndTurn = ConfigurationManager.Instance.GlobalDataJson.secondsEndTurn;
    }

    public void BeginOfTheTurn()
    {

        CharacterBegining?.Invoke();
        menuController.SetPanelEnemy(false);
        if (GameManagerMap.Instance.Map.Characters.Count == 0)
            return;

        _characters.AddRange(GameManagerMap.Instance.Map.Characters.Select(n => n.GetComponent<CharacterInfo>()));


        if (_characters.First().Index == -1)
        {
            for (int i = 0; i < _characters.Count; i++)
            {
                _characters[i].Index = i;
                _characters[i].OnIndexSet();
            }
        }

        IteratorPlusOne();
    }

    public void AddEnemyToTriggerList(EnemyInfo enemy) => _enemiesTriggered.Add(enemy);
    public void CharacterEndHisTurn(CharacterInfo character)
    {
        if (character == GameManagerMap.Instance.CharacterMovement.SelectedCharacter)
            character.OnDeselected();

        _characters.Remove(character);

        if (_enemiesMakesTurn.Count != 0)
            return;

        if (_characters.Count > 0)
        {
            IteratorPlusOne();
        }


        if (_characters.Count == 0)
        {
            if (_enemiesTriggered.Count > 0)
            {
                OnTriggerEndMakeAction(null);
            }
            else
                StartCoroutine(EndTurn());
        }
    }

    private IEnumerator EndTurn()
    {
        Debug.Log("End turn");
        GameManagerMap.Instance.StatusMain.SetStatusWaiting();
        menuController.SetPanelEnemy(true);
        yield return new WaitForSeconds(_secondsEndTurn);
        BeginOfTheEnemyTurn();
    }

    public void SetCharacter(CharacterInfo characterInfo)
    {
        _iterator = _characters.IndexOf(characterInfo);
    }

    private void BeginOfTheEnemyTurn()
    {
        _enemiesMakesTurn = GameManagerMap.Instance.Map.Enemy.Select(n => n.GetComponent<EnemyInfo>()).Where(n => n.IsTriggered).ToList();
        _enemiesMakesTurn.ForEach(n => n.CountActions = 2);
        OnEnemyEndMakeAction(null);
    }

    public void OnEnemyEndMakeAction(EnemyInfo enemy)
    {
        if (_enemiesMakesTurn != null)
            _enemiesMakesTurn.Remove(enemy);

        if (_characters.Count != 0)
            return;

        if (_enemiesMakesTurn.Count > 0)
            _enemiesMakesTurn.First().EnemyController.MakeAction(null);
        else
        {
            GameManagerMap.Instance.StatusMain.SetStatusSelectCharacter();
            Debug.Log("New turn");
            BeginOfTheTurn();
        }
    }
    public void OnTriggerEndMakeAction(EnemyInfo enemy)
    {
        if (_enemiesTriggered != null)
            _enemiesTriggered.Remove(enemy);

        if (_enemiesTriggered.Count > 0)
            _enemiesTriggered.First().EnemyController.OnTriggerMakeAction();
        else
        {
            if (_characters.Count > 0)
                IteratorPlusOne();
            else
                StartCoroutine(EndTurn());
        }
    }

    private void SetNexCharacter()
    {
        GameManagerMap.Instance.StatusMain.SetStatusSelectAction();
        _characters[_iterator].OnSelected(_characters[_iterator]);
        GameManagerMap.Instance.CameraController.MoveToSelectedCharacter();
    }
    private void IteratorPlusOne()
    {
        _iterator++;
        if (_iterator >= _characters.Count)
            _iterator = 0;

        SetActualCharacter();
    }

    public void SetActualCharacter()
    {
        if (_characters.Count == 0)
            return;

        if (_enemiesTriggered.Count == 0)
            SetNexCharacter();
        else
            OnTriggerEndMakeAction(null);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && GameManagerMap.Instance.StatusMain.ActualPermissions.Contains(Permissions.SelectCharacter) &&
            GameManagerMap.Instance.CharacterMovemovent.SelectedCharacter != null && _characters.Count > 1)
        {
            IteratorPlusOne();
        }
    }


    private void Clear()
    {
        _characters.Clear();
        _iterator = 0;
    }
}
