using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterTargetFinder : MonoBehaviour
{
    [SerializeField]
    private EnemyPanel _enemyPanel;

    private readonly HashSet<GameObject> _availableTargets = new();
    public HashSet<GameObject> AvailableTargets => _availableTargets;

    public Action<HashSet<GameObject>> OnUpdate;
    public Action OnVisibilityEnemyUpdate;

    // Updates the set of enemies that can be targeted by the selected character's attacks,
    // adjusts UI accordingly.
    public void UpdateAvailableTargets(CharacterInfo character)
    {
        // Clear the current set of enemies
        _availableTargets.Clear();

        // If a character is selected, look for available targets
        if (character)
        {
            foreach (GameObject enemy in GameManagerMap.Instance.Map.Enemy
                .Where(e => TargetingHelpers.CanTarget(character.transform, e.transform, character.VisibilityCharacter())))
            {
                // Add a suitable enemy to the set
                _availableTargets.Add(enemy);
            }
        }

        // We have to update the enemy panel before any other update,
        // because some systems rely on using the enemy icons.
        //
        // This might change in future.
        _enemyPanel.UpdateVisibleEnemies(_availableTargets);

        OnUpdate(_availableTargets);
        OnVisibilityEnemyUpdate();
    }

    public List<CharacterInfo> UpdateVisibilityForEnemy(EnemyInfo enemyInfo)
    {
        return GameManagerMap.Instance.Map.Characters.Select(obj => obj.GetComponent<CharacterInfo>())
            .Where(e => TargetingHelpers.CanTarget(enemyInfo.transform, e.transform, enemyInfo.VisibilityCharacter()))
            .ToList();
    }
}
