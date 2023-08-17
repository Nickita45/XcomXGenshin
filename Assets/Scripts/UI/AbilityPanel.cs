using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject _abilityDialog;

    [SerializeField]
    private TextMeshProUGUI _titleText;

    [SerializeField]
    private TextMeshProUGUI _descriptionText;

    [SerializeField]
    private Button _confirm;

    private AbilityIcon _selected;

    void Start()
    {
        GameManagerMap.Instance.OnClearMap += () => ClearSelection();
        _confirm.onClick.AddListener(() => ClearSelection());
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
        AbilityIcon _wasSelected = ClearSelection();

        if (_wasSelected != icon)
        {
            icon.Image.color = Color.blue;

            _titleText.text = icon.Title;
            _descriptionText.text = icon.Description;
            _abilityDialog.SetActive(true);
            _confirm.onClick.AddListener(icon.Event.Invoke);

            _selected = icon;
        }
    }

    // Clears selection. If any ability was selected, returns it.
    public AbilityIcon ClearSelection()
    {
        if (_selected)
        {
            AbilityIcon tmp = _selected;

            _selected.Image.color = Color.white;
            _abilityDialog.SetActive(false);
            _confirm.onClick.RemoveListener(_selected.Event.Invoke);
            _selected = null;

            return tmp;
        }

        return null;
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
