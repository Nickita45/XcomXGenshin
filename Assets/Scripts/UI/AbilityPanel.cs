using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityPanel : MonoBehaviour
{
    private AbilityDialog _abilityDialog;

    private AbilityIcon _selected;

    void Start()
    {
        //GameManagerMap.Instance.OnClearMap += ClearVisibleEnemies;
        _abilityDialog = FindObjectOfType<AbilityDialog>(true);
    }

    void Update()
    {
        for (int i = 0; i < 10; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                Debug.Log(i);
            }
        }
    }

    public void SelectAbility(AbilityIcon icon)
    {
        Exit();

        if (_selected && _selected == icon)
        {
            _selected = null;
            _abilityDialog.gameObject.SetActive(false);
        }
        else
        {
            _selected = icon;
            icon.Image.color = Color.blue;
            _abilityDialog.View(icon);
            _abilityDialog.gameObject.SetActive(true);
        }
    }

    public void Exit()
    {
        if (_selected) _selected.Image.color = Color.white;
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
