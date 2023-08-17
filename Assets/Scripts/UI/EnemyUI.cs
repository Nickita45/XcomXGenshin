using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyUI : MonoBehaviour
{
    [SerializeField]
    private GameObject _iconPrefab;

    private EnemyIcon _selected;

    void Start()
    {
        GameManagerMap.Instance.OnClearMap += ClearVisibleEnemies;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (transform.childCount > 1 && _selected)
            {
                int index = _selected.transform.GetSiblingIndex() - 1;
                if (index < 0) 
                    index = transform.childCount - 1;

                Transform iconObject = transform.GetChild(index);

                EnemyIcon icon = iconObject.GetComponent<EnemyIcon>();
                GameManagerMap.Instance.ViewEnemy(icon);
            }
        }
    }

    public void AddVisibleEnemy(GameObject enemy)
    {
        GameObject image = Instantiate(_iconPrefab, transform);

        EnemyIcon icon = image.GetComponent<EnemyIcon>();
        icon.SetEnemy(enemy);
    }

    public void ClearVisibleEnemies()
    {
        _selected = null;

        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    public void SelectEnemy(EnemyIcon icon)
    {
        Exit();
        icon.Image.color = Color.red;
        _selected = icon;
    }

    public void Exit()
    {
        if (_selected) 
            _selected.Image.color = Color.white;
    }

    public EnemyIcon GetIconForEnemy(GameObject enemy)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            EnemyIcon icon = transform.GetChild(i).GetComponent<EnemyIcon>();
            if (icon.Enemy == enemy) return icon;
        }
        return null;
    }
}