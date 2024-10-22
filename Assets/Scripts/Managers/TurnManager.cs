using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Manages selecting characters, game turns, enemy triggers etc.
public class TurnManager : MonoBehaviour
{
    [SerializeField]
    private MenuManager _menuManager;

    // List of all characters that have unspent actions left
    private List<Character> _characters = new();

    // We store the character as an object, not as an index.
    // This makes it easier to work with the characters list,
    // which is often being modified.
    private Character _selectedCharacter;
    public Character SelectedCharacter => _selectedCharacter;

    public List<Unit> UnitOverwatched { get; private set; }

    private float _secondsEndTurn = 3f;

    private void Start()
    {
        Manager.Instance.OnClearMap += ClearCharacters;
        _secondsEndTurn = ConfigurationManager.GlobalDataJson.secondsEndTurn;
        UnitOverwatched = new List<Unit>();
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

    private void ClearCharacters()
    {
        _characters.Clear();
        _selectedCharacter = null;
    }

    public void LoadCharacterData()
    {
        if (Manager.Map.Characters.GetList.Count == 0)
            return;

        List<Character> characters = Manager.Map.Characters.GetList; //actualization list of characters

        if (characters.First().Stats.Index == -1)  //set basic parameters to characters
        {
            // This requires to start the game using the Hub Scene
            HubData.Instance.charactersPoolID = HubData.Instance.charactersPoolID.OrderBy(x => x == -1).ToArray();
            for (int i = 0; i < characters.Count; i++)
            {
                characters[i].Stats.Index = (HubData.Instance.charactersPoolID[i] == -1) ? 0 : HubData.Instance.charactersPoolID[i];
                characters[i].OnIndexSet();
                if (HubData.Instance.charactersPoolID[i] == -1)
                {
                    characters[i].Kill(); //if such index character does not exist, removing him
                }
            }
        }
        Manager.StatisticsUtil.SoldierDeathCount = 0;
        Manager.StatisticsUtil.SoldierTotalCount = characters.Count;
        Manager.StatisticsUtil.EnemiesTotalCount = Manager.Map.Enemies.GetList.Count;
        /*foreach (Enemy enemy in Manager.Map.Enemies)
            Manager.Instance.StatisticsUtil.setEnemiesKilledList(enemy.Stats.Icon);*/
    }

    public IEnumerator BeginRound()
    {
        // Remove overwatch from all characters
        UnitOverwatched.RemoveAll(unit => unit is Character);

        // Restore actions
        foreach (Character character in Manager.Map.Characters.GetList) { 
            character.ActionsLeft = 2;
            character.DecreaseCooldownAbilities();
        }
        foreach (Enemy enemy in Manager.Map.Enemies.GetList) { enemy.ActionsLeft = enemy.Stats.BaseActions(); }
        foreach (Entity entity in Manager.Map.Entities.GetList) { entity.LifeTimerDecrease(); }

        // Trigger modifiers on begin round
        foreach (Unit unit in Manager.Map.GetAllUnits())
        {
            if (unit.Modifiers == null) continue;

            yield return StartCoroutine(unit.Modifiers.OnBeginRound());
            unit.Canvas.UpdateModifiersUI(unit.Modifiers);
        }
        BeginEntityTurn();

        BeginPlayerTurn();
    }

    public void BeginEntityTurn()
    {
        foreach (Entity entity in Manager.Map.Entities.GetList)
        {
            entity.Activate();
        }
    }

    public void BeginPlayerTurn()
    {
        Debug.Log("Begin turn");

        _characters.AddRange(Manager.Map.Characters.GetList);
        if (_characters.Count > 0) SelectCharacter(_characters[0]);
    }

    public IEnumerator AfterCharacterAction()
    {
        Manager.StatusMain.SetStatusWaiting();

        // Trigger all possible enemies
        foreach (var enemy in Manager.Map.Enemies.GetList)
        {
            if(!enemy.IsKilled)
                yield return StartCoroutine(enemy.ConditionalTrigger());
        } 

        // If the character still has some actions left, reselect them. 
        if (_selectedCharacter?.ActionsLeft > 0)
        {
            SelectCharacter(_selectedCharacter);
        }
        // Otherwise, remove them from the list.
        else
        {
            OutOfActions(_selectedCharacter);

            if (_characters.Count == 0)
            {
                _selectedCharacter.OnDeselected();
                //DeselectCharacter();
                yield return StartCoroutine(EndPlayerTurn());
            }
        }
    }

    public void OutOfActions(Character character)
    {
        _characters.Remove(character);

        // Select the next character, if possible.
        if (_characters.Count > 0)
        {
            SelectNextCharacter();
        }
    }

    private IEnumerator EndPlayerTurn()
    {
        Debug.Log("End turn");

        yield return new WaitForSeconds(_secondsEndTurn);
        yield return StartCoroutine(MakeEnemyTurn());
    }

    private IEnumerator MakeEnemyTurn()
    {
        _menuManager.SetPanelEnemy(true);
        UnitOverwatched.RemoveAll(unit => unit is Enemy);

        foreach (var enemy in Manager.Map.Enemies.GetList)//for (int i = Manager.Map.Enemies.Count - 1; i >= 0; i--) //make from bottom to up to savly removing killed enemies
        {
            if (enemy.IsKilled) continue;

            enemy.ActionsLeft = enemy.Stats.BaseActions();
            while (enemy.ActionsLeft > 0)
            {
                if (!enemy.Triggered)
                    break;

                yield return StartCoroutine(enemy.MakeTurn());
            }
        }
        _menuManager.SetPanelEnemy(false);
        yield return StartCoroutine(EndRound());
    }

    private IEnumerator EndRound()
    {
        // Trigger modifiers on end round
        foreach (Unit unit in Manager.Map.GetAllUnits())
        {
            if (unit.Modifiers == null) continue;

            yield return StartCoroutine(unit.Modifiers.OnEndRound());
            unit.Canvas.UpdateModifiersUI(unit.Modifiers);
        }

        // Start next round
        yield return StartCoroutine(BeginRound());
    }

    public IEnumerable<Unit> CheckOverwatchMake(Unit unit)
    {
        List<Unit> mustBeDeleted = new List<Unit>();
        foreach (var unitOvewatch in UnitOverwatched)
        {
            if (unitOvewatch is Character && unit is Character || unitOvewatch is Enemy && unit is Enemy)
                continue;

            if (TargetUtils.CanSee(unit, unitOvewatch))
            {
                mustBeDeleted.Add(unitOvewatch);
                yield return unitOvewatch;

                if (unit.IsKilled)
                    break;
            }
        }
        foreach (var delete in mustBeDeleted)
        {
            UnitOverwatched.Remove(delete);
        }
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
