using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Manages selecting characters, game turns, enemy triggers etc.
public class TurnManager : MonoBehaviour
{
    [SerializeField]
    private MenuManager _menuManager;

    private List<Character> _characters = new();

    // We store the character as an object, not as an index.
    // This makes it easier to work with the characters list,
    // which is often being modified.
    private Character _selectedCharacter;
    public Character SelectedCharacter => _selectedCharacter;

    public Action onBeginTurn;
    private float _secondsEndTurn = 3f;

    private void Start()
    {
        Manager.Instance.OnClearMap += Clear;
        _secondsEndTurn = ConfigurationManager.GlobalDataJson.secondsEndTurn;
    }

    public void SelectCharacter(Character character)
    {
        if (character != null && _characters.Contains(character))
        {
            // deselect current character
            _selectedCharacter?.OnDeselected();

            // select new character
            _selectedCharacter = character;
            _selectedCharacter.OnSelected(_selectedCharacter);

            Manager.CameraManager.FreeCamera.MoveToSelectedCharacter();
            Manager.StatusMain.SetStatusSelectAction();
        }
    }

    public void DeselectCharacter()
    {
        if (_selectedCharacter)
        {
            _selectedCharacter.OnDeselected();
            _selectedCharacter = null;

            Manager.StatusMain.SetStatusSelectCharacter();
        }
    }

    private void SelectNextCharacter()
    {
        if (_selectedCharacter)
        {
            int index = _characters.FindIndex(c => c == _selectedCharacter);
            index = index != _characters.Count - 1 ? index + 1 : 0;

            SelectCharacter(_characters[index]);
        }
    }

    private void SelectPreviousCharacter()
    {
        if (_selectedCharacter)
        {
            int index = _characters.FindIndex(c => c == _selectedCharacter);
            index = index != 0 ? index - 1 : _characters.Count - 1;

            SelectCharacter(_characters[index]);
        }
    }

    private void Clear()
    {
        _characters.Clear();
        _selectedCharacter = null;
    }

    public void BeginOfTheTurn()
    {
        Debug.Log("New turn");

        onBeginTurn?.Invoke();
        _menuManager.SetPanelEnemy(false);

        if (Manager.Map.Characters.Count == 0)
            return;

        _characters.AddRange(Manager.Map.Characters); //actualization list of characters

        if (_characters.First().Stats.Index == -1) //set basic parameters to characters
        {
            // This requires to start the game using the Hub Scene
            HubData.Instance.charactersPoolID = HubData.Instance.charactersPoolID.OrderBy(x => x == -1).ToArray();
            for (int i = 0; i < _characters.Count; i++)
            {
                _characters[i].Stats.Index = (HubData.Instance.charactersPoolID[i] == -1) ? 0 : HubData.Instance.charactersPoolID[i]; //set index
                _characters[i].OnIndexSet();
                if (HubData.Instance.charactersPoolID[i] == -1)
                {
                    _characters[i].Kill(); //if such index character does not exist, removing him
                }
            }
        }

        SelectCharacter(_characters[0]);
    }

    public void EndCharacterTurn(Character character)
    {
        _characters.Remove(character);
        if (_characters.Count > 0) SelectNextCharacter();
    }

    private IEnumerator EndTurn()
    {
        Debug.Log("End turn");
        yield return new WaitForSeconds(_secondsEndTurn);
        yield return StartCoroutine(EnemyTurn());
    }

    private IEnumerator EnemyTurn()
    {
        _menuManager.SetPanelEnemy(true);

        foreach (Enemy enemy in Manager.Map.Enemies)
        {
            enemy.ActionsLeft = enemy.Stats.BaseActions();
            while(enemy.ActionsLeft > 0)
            {
                Debug.Log("huh");
                yield return StartCoroutine(enemy.MakeTurn());
            }
        }

        BeginOfTheTurn();
        yield return null;
    }

    // Actions that happen right after the character made an action (or moved)
    public IEnumerator AfterCharacterAction()
    {
        Manager.StatusMain.SetStatusWaiting();

        // Trigger all possible enemies
        foreach (Enemy enemy in Manager.Map.Enemies)
        {
            yield return StartCoroutine(enemy.ConditionalTrigger());
        }

        // If the character still has some actions left,
        // reselect it. Otherwise, end their turn. 
        if (_selectedCharacter?.ActionsLeft <= 0)
        {
            EndCharacterTurn(_selectedCharacter);
        }
        else
        {
            SelectCharacter(_selectedCharacter);
        }

        if (_characters.Count == 0)
            yield return StartCoroutine(EndTurn());
    }

    private void Update()
    {
        if (Manager.HasPermission(Permissions.SelectCharacter) && _characters.Count > 1)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                SelectNextCharacter();
            }
            else if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                SelectPreviousCharacter();
            }
        }
    }
}
