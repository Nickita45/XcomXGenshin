using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject _iconPrefab;

    private List<EnemyIcon> _icons = new();

    private int? _selectedIndex;

    public GameObject EnemyObject => _icons[_selectedIndex.Value].Enemy;
    public GameObject CanvasEnemyObject => EnemyObject.GetComponent<EnemyCanvasController>().CanvasToMove;
    public EnemyCanvasController EnemyCanvasController => EnemyObject.GetComponent<EnemyCanvasController>();
    public EnemyInfo EnemyInfo => EnemyObject.GetComponent<EnemyInfo>();
    public int SelectedEnemyPercent => _icons[_selectedIndex.Value].Percent;

    [SerializeField]
    private AbilityPanel _abilityPanel;

    [SerializeField]
    private AbilityIcon _attackAbility;

    void Start()
    {
        GameManagerMap.Instance.OnClearMap += ClearVisibleEnemies;
        GameManagerMap.Instance.CharacterMovement.OnStartMove += ClearVisibleEnemies;
        GameManagerMap.Instance.StatusMain.OnStatusChange += OnStatusChange;

    }

    void Update()
    {
        if (GameManagerMap.Instance.StatusMain.ActualPermissions.Contains(Permissions.SelectEnemy)
            && Input.GetKeyDown(KeyCode.Tab))
        {
            if (_selectedIndex.HasValue)
            {
                int newIndex = _selectedIndex.Value - 1;
                if (newIndex == -1) newIndex = transform.childCount - 1;

                SelectEnemy(_icons[newIndex]);
            }
        }
    }

    public void UpdateVisibleEnemies(HashSet<GameObject> enemies)
    {
        ClearVisibleEnemies();

        foreach (GameObject enemy in enemies)
        {
            GameObject image = Instantiate(_iconPrefab, transform);

            EnemyIcon icon = image.GetComponent<EnemyIcon>();
            icon.SetEnemy(enemy);

            _icons.Add(icon);
        }
    }

    public void ClearVisibleEnemies()
    {
        _icons.Clear();
        _selectedIndex = null;

        ObjectHelpers.DestroyAllChildren(gameObject);
    }

    public void SelectEnemy(EnemyIcon icon)
    {
        ClearSelection();
        _selectedIndex = _icons.FindIndex(i => i == icon);
        icon.Image.color = Color.red;

        _abilityPanel.SelectAbility(_attackAbility);
    }

    public void ClearSelection()
    {
        if (_selectedIndex.HasValue)
        {
            _icons[_selectedIndex.Value].Image.color = Color.white;
            _selectedIndex = null;
        }
    }

    // Gets the EnemyIcon pointing at the given enemy object.
    public EnemyIcon GetIconForEnemy(GameObject enemy)
    {
        return _icons.Find(i => i.Enemy == enemy);
    }

    // Gets the selected icon.
    // If the selection is none, selects the last icon and returns it.
    public EnemyIcon GetSelectedIconOrLast()
    {
        if (!_selectedIndex.HasValue) SelectEnemy(_icons[^1]);
        return _icons[_selectedIndex.Value];
    }

    private void OnStatusChange(HashSet<Permissions> permissions)
    {
        if (permissions.Contains(Permissions.AnimationShooting) || permissions.Contains(Permissions.Waiting))
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
}