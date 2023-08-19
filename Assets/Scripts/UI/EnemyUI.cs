using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyUI : MonoBehaviour
{
    [SerializeField]
    private GameObject _iconPrefab;

    private List<EnemyIcon> _icons = new();
    public List<EnemyIcon> Icons => _icons;

    private int? _selectedIndex;

    public GameObject EnemyObject => _icons[_selectedIndex.Value].Enemy;
    public GameObject CanvasEnemyObject => EnemyObject.GetComponent<EnemyCanvasController>().CanvasToMove;
    public EnemyCanvasController EnemyCanvasController => EnemyObject.GetComponent<EnemyCanvasController>();
    public int SelectedEnemyProcent => Icons[_selectedIndex.Value].Procent;

    void Start()
    {
        GameManagerMap.Instance.OnClearMap += ClearVisibleEnemies;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (_selectedIndex.HasValue)
            {
                int newIndex = _selectedIndex.Value - 1;
                if (newIndex == -1) newIndex = transform.childCount - 1;

                GameManagerMap.Instance.ViewEnemy(_icons[newIndex]);

                _selectedIndex = newIndex;
            }
        }
    }

    public void AddVisibleEnemy(GameObject enemy)
    {
        GameObject image = Instantiate(_iconPrefab, transform);

        EnemyIcon icon = image.GetComponent<EnemyIcon>();
        icon.SetEnemy(enemy);

        _icons.Add(icon);
    }

    public void ClearVisibleEnemies()
    {
        _icons.Clear();
        _selectedIndex = null;

        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    public void SelectEnemy(EnemyIcon icon)
    {
        Exit();
        //ClearSelection();
        _selectedIndex = _icons.FindIndex(i => i == icon);
        icon.Image.color = Color.red;
    }

    public void ClearSelection()
    {
        if (_selectedIndex.HasValue)
        {
            _icons[_selectedIndex.Value].Image.color = Color.white;
            _selectedIndex = null;
        }
    }

    public EnemyIcon GetIconForEnemy(GameObject enemy)
    {
        return _icons.Find(i => i.Enemy == enemy);
    }
}