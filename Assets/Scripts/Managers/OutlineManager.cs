using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Manages outlines for the units. The outlines help to distinguish one unit from another
// in situations where this might be hard. It also helps with debugging.
public class OutlineManager : MonoBehaviour
{
    private bool _outlinesEnabled = false;

    // Sets containing the targeted units, which will be
    // highlighted in a slightly brighter colors.
    private HashSet<UnitOutline> _targetCharacters = new();
    private HashSet<UnitOutline> _targetEnemies = new();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        _outlinesEnabled = !_outlinesEnabled;
        UpdateOutlines();
    }

    private void UpdateOutlines()
    {
        foreach (UnitOutline outline in Manager.Map.Enemies.GetList.Select(u => u.Animator.Outline)
            .Concat(Manager.Map.Characters.GetList.Select(u => u.Animator.Outline)))
        {
            if (_targetEnemies.Contains(outline) || _targetCharacters.Contains(outline))
            {
                outline.SetOutline(true);
                outline.SetTargetColor();
            }
            else
            {
                outline.SetOutline(_outlinesEnabled);
                outline.SetNormalColor();
            }
        }
    }

    public void TargetEnemy(UnitOutline outline) => TargetEnemies(new() { outline });

    // Targets selected enemies. This means the enemy will have
    // a brighter outline that is always visible.
    //
    // Doing this would replace the previous enemy targets.
    public void TargetEnemies(HashSet<UnitOutline> targets)
    {
        _targetEnemies = targets;
        UpdateOutlines();
    }
    public void ClearEnemyTargets() => TargetEnemies(new());

    public void TargetCharacter(UnitOutline outline) => TargetCharacters(new() { outline });

    // Targets selected characters. This means the character will have
    // a brighter outline that is always visible.
    //
    // Doing this would replace the previous character targets.
    public void TargetCharacters(HashSet<UnitOutline> targets)
    {
        _targetCharacters = targets;
        UpdateOutlines();
    }
    public void ClearCharacterTargets() => TargetCharacters(new());

    // Removes all targets from enemies and characters.
    public void ClearTargets()
    {
        ClearCharacterTargets();
        ClearEnemyTargets();
    }
}
