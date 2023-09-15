using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterVisibility : MonoBehaviour
{
    [SerializeField]
    private EnemyPanel _enemyPanel;

    private readonly HashSet<GameObject> _visibleEnemies = new();
    public HashSet<GameObject> VisibleEnemies => _visibleEnemies;

    public Action<HashSet<GameObject>> OnVisibilityUpdate;
    public Action OnVisibilityEnemyUpdate;

    // Updates the set of enemies visible by the selected character
    // and adjusts UI accordingly.
    public void UpdateVisibility(CharacterInfo character)
    {
        // Clear the current set of visible enemies
        _visibleEnemies.Clear();

        // If a character is selected, look for visible enemies
        if (character)
        {

            foreach (TerritoryInfo enemy in GameManagerMap.Instance.Map.Enemy
                .Select(obj => obj.GetComponent<TerritoryInfo>())
                .Where(e => Vector3.Distance(e.transform.position, (character).transform.position) < character.VisibilityCharacter())
                .Where(e => IsEnemyVisible(character, e)))
            {
                // Add visible enemies to set
                _visibleEnemies.Add(enemy.gameObject);
            }
        }
        // We have to update the enemies before any other visibility update,
        // as some systems rely on using the enemy icons.
        _enemyPanel.UpdateVisibleEnemies(_visibleEnemies);

        OnVisibilityUpdate(_visibleEnemies);
        OnVisibilityEnemyUpdate();
    }

    public List<CharacterInfo> UpdateVisibilityForEnemy(EnemyInfo enemyInfo)
    {
        return GameManagerMap.Instance.Map.Characters.Select(obj => obj.GetComponent<CharacterInfo>())
            .Where(e => Vector3.Distance(e.transform.position, (enemyInfo).transform.position) < enemyInfo.VisibilityCharacter())
            .Where(e => IsEnemyVisible(enemyInfo, e.GetComponent<TerritoryInfo>())).ToList();
    }

    private bool IsEnemyVisible(PersonInfo character, TerritoryInfo enemy)
    {
        // Try out all tiles around the character,
        // to account for being able to see through corners
        foreach (Vector3 direction in DIRECTIONS)
        {
            Vector3 origin = enemy.transform.position;
            Vector3 target = character.transform.position + direction;

            // Find difference vector between target and the origin
            Vector3 delta = target - origin;

            // Create a raycast from the enemy to the target tile
            RaycastHit[] hits = Physics.RaycastAll(origin, delta.normalized, delta.magnitude);
            bool anyShelter = false;

            foreach (RaycastHit hit in hits)
            {
                // Check if any hit is a full shelter
                TerritoryInfo info = hit.transform.GetComponent<TerritoryInfo>();
                if (info && info.Type == TerritoryType.Shelter && info.ShelterType.Left == ShelterType.Full &&
                    info.ShelterType.Right == ShelterType.Full && info.ShelterType.Bottom == ShelterType.Full
                        && info.ShelterType.Front == ShelterType.Full)
                {
                    //Debug.DrawRay(info.gameObject.transform.position, Vector3.up * 100f, Color.blue, 60f);
                    anyShelter = true;
                    break;
                }
            }

            // If there weren't any full shelters, the enemy is considered visible
            if (!anyShelter) return true;
        }

        return false;
    }

    private static readonly Vector3[] DIRECTIONS = {
        Vector3.zero,
        Vector3.forward,
        Vector3.back,
        Vector3.right,
        Vector3.left,
        Vector3.forward + Vector3.right,
        Vector3.forward + Vector3.left,
        Vector3.back + Vector3.right,
        Vector3.back + Vector3.left
    };
}
