using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterVisibility : MonoBehaviour
{
    private EnemyUI _enemyUI;

    private readonly HashSet<GameObject> _visibleEnemies = new();

    [SerializeField]
    private float _maxVisionDistance = 10.0f;

    public float MaxVisionDistance { get => _maxVisionDistance; set => _maxVisionDistance = value; }

    // Start is called before the first frame update
    void Start()
    {
        _enemyUI = GameObject.FindAnyObjectByType<EnemyUI>();
    }

    // Updates the set of enemies visible by the selected character
    // and adjusts UI accordingly.
    public void UpdateVisibility(CharacterInfo character)
    {
        // Clear the current set of visible enemies
        _visibleEnemies.Clear();

        // If a character is selected, look for visible enemies
        if (character)
        {

            foreach (TerritoryInfo enemy in GameManagerMap.Instance.Map.Enemy.Select(n => n.GetComponent<TerritoryInfo>())
                .Where(info => Vector3.Distance(info.transform.position, character.transform.position) < _maxVisionDistance)
                .Where(info => IsEnemyVisible(character, info)))
            {
                // Add visible enemies to set
                _visibleEnemies.Add(enemy.gameObject);
            }

        }

        // Clear icons
        _enemyUI.ClearVisibleEnemies();

        // Add icons on UI for each visible enemy 
        foreach (GameObject enemy in _visibleEnemies)
        {
            _enemyUI.AddVisibleEnemy(enemy);
        }

        // Mark visible enemies with colors
        foreach (TerritoryInfo enemy in GameManagerMap.Instance.Map.Enemy.Select(n => n.GetComponent<TerritoryInfo>()))
        {
            Color color = _visibleEnemies.Contains(enemy.gameObject) ? Color.blue : Color.red;
            enemy.gameObject.GetComponent<MeshRenderer>().material.color = color;
        }
    }

    bool IsEnemyVisible(CharacterInfo character, TerritoryInfo enemy)
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
