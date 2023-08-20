using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class EnemyIcon : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private EnemyUI _enemyUI;

    private GameObject _enemy;
    public GameObject Enemy => _enemy;

    [SerializeField]
    private Image _image;
    public Image Image => _image;

    [SerializeField]
    private TextMeshProUGUI _textPercent;

    public int Procent { get; set; }

    void Start()
    {
        _enemyUI = transform.parent.GetComponent<EnemyUI>();
        SetPercent();
    }

    public void OnPointerClick(PointerEventData data)
    {
        GameManagerMap.Instance.ViewEnemy(this);
    }

    public void OnPointerEnter(PointerEventData data)
    {
        Color tempColor = _image.color;
        tempColor.a = 0.5f;
        _image.color = tempColor;

        if (!GameManagerMap.Instance.StatusMain.ActualPermissions.Contains(Permissions.SelectEnemy) 
            && GameManagerMap.Instance.StatusMain.ActualPermissions.Contains(Permissions.SelectPlaceToMovement))//(GameManagerMap.Instance.State == GameState.FreeMovement)
        {
            Vector3 position = CameraUtils.CalculateCameraLookAt(_enemy, Camera.main);
            GameManagerMap.Instance.FixedCameraController
                .Init(position, GameManagerMap.Instance.CameraController.TargetRotation, 0.5f);
        }
    }

    public void OnPointerExit(PointerEventData data)
    {
        Color tempColor = _image.color;
        tempColor.a = 1f;
        _image.color = tempColor;

        if (!GameManagerMap.Instance.StatusMain.ActualPermissions.Contains(Permissions.SelectEnemy) 
            && GameManagerMap.Instance.StatusMain.ActualPermissions.Contains(Permissions.SelectPlaceToMovement))//(GameManagerMap.Instance.State == GameState.FreeMovement)
            GameManagerMap.Instance.FixedCameraController.Init(
                GameManagerMap.Instance.CameraController.transform.position,
                GameManagerMap.Instance.CameraController.transform.rotation,
                0.5f,
                GameManagerMap.Instance.CameraController.Init
            );
        //GameManagerMap.Instance.FixedCameraController.ClearListHide();
    }

    public void SetEnemy(GameObject enemy)
    {
        _enemy = enemy;
    }

    public void SetPercent()
    {
        var resultCaclulations = AimCalculater.CalculateShelterPercent(defender: GameManagerMap.Instance.Map[_enemy.transform.localPosition],
                                    shooter: GameManagerMap.Instance.CharacterMovemovent.SelectedCharacter.ActualTerritory,
                                    gun: GameManagerMap.Instance.Gun,
                                    0, GameManagerMap.Instance.CharacterMovemovent.SelectedCharacter.BasicAimCharacter);

        Procent = resultCaclulations.procent;
        _textPercent.text = resultCaclulations.procent.ToString() + "%";

        if (resultCaclulations.status == ShelterType.Nope)
            _textPercent.color = Color.yellow;
        else
            _textPercent.color = Color.red;
    }

}
