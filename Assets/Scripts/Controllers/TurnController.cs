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
        _characters.AddRange(FindObjectsOfType<CharacterInfo>());

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

    public void CharacterEndHisTurn(CharacterInfo character)
    {
        if (character == GameManagerMap.Instance.CharacterMovemovent.SelectedCharacter)
            character.OnDeselected();

        if (_characters.Count > 1)
        {
             IteratorPlusOne();
        }

        _characters.Remove(character);

        if (_characters.Count == 0)
            StartCoroutine(EndTurn());
    }

    private IEnumerator EndTurn()
    {
        Debug.Log("End turn");
        GameManagerMap.Instance.StatusMain.SetStatusWaiting();
        menuController.SetPanelEnemy(true);
        yield return new WaitForSeconds(_secondsEndTurn);
        GameManagerMap.Instance.StatusMain.SetStatusSelectCharacter();
        Debug.Log("New turn");
        BeginOfTheTurn();
    }

    public void SetCharacter(CharacterInfo characterInfo)
    {
        _iterator = _characters.IndexOf(characterInfo);
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab) && GameManagerMap.Instance.StatusMain.ActualPermissions.Contains(Permissions.SelectCharacter) &&
            GameManagerMap.Instance.CharacterMovemovent.SelectedCharacter != null && _characters.Count > 1) 
        {
            IteratorPlusOne();
        }   
    }

    private void IteratorPlusOne()
    {
        _iterator++;
        if (_iterator >= _characters.Count)
            _iterator = 0;

        _characters[_iterator].OnSelected(_characters[_iterator]);
        GameManagerMap.Instance.CameraController.MoveToSelectedCharacter();
        GameManagerMap.Instance.StatusMain.SetStatusSelectAction();

    }

    private void Clear()
    {
        _characters.Clear();
        _iterator = 0;
    }
}
