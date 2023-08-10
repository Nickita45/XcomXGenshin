using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EnemyIconClick : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private GameObject _enemy;
    private Image _image;

    [SerializeField]
    private TextMeshProUGUI _textPercent;

    public void OnPointerClick(PointerEventData data)
    {
        GameManagerMap.Instance.CameraController.MoveTo(_enemy.transform.position);
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
