using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTargetPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject _iconPrefab;

    private List<EnemyIcon> _icons = new();

    private int? _selectedIndex;
    public EnemyIcon Selected => _selectedIndex.HasValue ? _icons[_selectedIndex.Value] : null;
    public Enemy Enemy => Selected?.Enemy;

    private Action<object> _onSelect;

    void Start()
    {
        Manager.Instance.OnClearMap += ClearVisibleEnemies;
        Manager.MovementManager.OnStartMove += ClearVisibleEnemies;
        Manager.StatusMain.OnStatusChange += OnStatusChange;
    }

    void Update()
    {
        if (Manager.HasPermission(Permissions.SelectEnemy) && _selectedIndex.HasValue)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                int newIndex = _selectedIndex.Value - 1;
                if (newIndex == -1) newIndex = transform.childCount - 1;
                SelectEnemy(_icons[newIndex]);
                Manager.StatusMain.SetStatusSelectEnemy();
            }
            else if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                int newIndex = _selectedIndex.Value + 1;
                if (newIndex == transform.childCount) newIndex = 0;
                SelectEnemy(_icons[newIndex]);
                Manager.StatusMain.SetStatusSelectEnemy();
            }
        }
    }

    public void UpdateVisibleEnemies(HashSet<Enemy> enemies)
    {
        ClearVisibleEnemies();

        foreach (Enemy enemy in enemies)
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

        ObjectUtils.DestroyAllChildren(gameObject);
    }

    public void SelectEnemy(EnemyIcon icon)
    {
        ClearSelection();
        _selectedIndex = _icons.FindIndex(i => i == icon);
        icon.Image.color = Color.red;

        _onSelect?.Invoke(icon.Enemy);

        Manager.CameraManager.FixCameraOnEnemy(icon);
        Manager.OutlineManager.ClearCharacterTargets();
        Manager.OutlineManager.TargetEnemy(icon.Enemy.Animator.Outline);
    }

    public void ClearSelection()
    {
        if (_selectedIndex.HasValue)
        {
            _icons[_selectedIndex.Value].Image.color = Color.white;
            _selectedIndex = null;
        }
    }

    // Gets the EnemyIcon pointing at the given enemy.
    public EnemyIcon GetIconForEnemy(Enemy enemy)
    {
        return _icons.Find(i => i.Enemy == enemy);
    }

    // Selects the last icon and returns it.
    public EnemyIcon SelectLast()
    {
        EnemyIcon icon = _icons[^1];
        SelectEnemy(icon);
        return icon;
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

    // Accepts a callback that is called every time an enemy icon is selected.
    // This is used for setting up the enemy target in TargetSelectManager.
    public void OnSelect(Action<object> onSelect)
    {
        _onSelect = onSelect;
    }

    public void OnCharacterSelect(Character character)
    {
        gameObject.SetActive(true);
        UpdateVisibleEnemies(TargetUtils.GetAvailableTargets(Manager.TurnManager.SelectedCharacter));
    }

    public void OnCharacterDeselect()
    {
        gameObject.SetActive(false);
    }
}