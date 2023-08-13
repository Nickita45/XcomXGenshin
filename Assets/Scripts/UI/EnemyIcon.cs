using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EnemyIcon : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private EnemyUI _enemyUI;

    private GameObject _enemy;
    private Image _image;

    [SerializeField]
    private TextMeshProUGUI _textPercent;
    public Image Image => _image;
    public GameObject Enemy => _enemy;

    public void OnPointerClick(PointerEventData data)
    {
        _enemyUI.SelectEnemy(this);
    }

    public void OnPointerEnter(PointerEventData data)
    {
        Color tempColor = _image.color;
        tempColor.a = 0.5f;
        _image.color = tempColor;
    }

    public void OnPointerExit(PointerEventData data)
    {
        Color tempColor = _image.color;
        tempColor.a = 1f;
        _image.color = tempColor;
    }

    public void SetEnemy(GameObject enemy)
    {
        _enemy = enemy;
    }

    // Start is called before the first frame update
    void Start()
    {
        _enemyUI = transform.parent.GetComponent<EnemyUI>();
        _image = GetComponent<Image>();

        SetProcent();
    }

    public void SetProcent()
    {
        var resultCaclulations = AimCalculater.CalculateShelterPercent(defender: GameManagerMap.Instance.Map[_enemy.transform.localPosition],
                                    shooter: GameManagerMap.Instance.CharacterMovemovent.SelectedCharacter.ActualTerritory,
                                    gun: GameManagerMap.Instance.Gun,
                                    0, GameManagerMap.Instance.CharacterMovemovent.SelectedCharacter.BasicAimCharacter);
        _textPercent.text = resultCaclulations.procent.ToString() + "%";

        if (resultCaclulations.status == ShelterType.Nope)
            _textPercent.color = Color.yellow;
        else
            _textPercent.color = Color.red;
    }

}
