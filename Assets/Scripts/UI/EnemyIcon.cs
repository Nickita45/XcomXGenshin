using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class EnemyIcon : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private EnemyPanel _enemyPanel;

    private GameObject _enemy;
    public GameObject Enemy => _enemy;

    [SerializeField]
    private Image _image;
    public Image Image => _image;

    [SerializeField]
    private TextMeshProUGUI _textPercent;

    public int Percent { get; set; }

    void Start()
    {
        _enemyPanel = transform.parent.GetComponent<EnemyPanel>();
        SetPercent();
    }

    public void OnPointerClick(PointerEventData data)
    {
        _enemyPanel.SelectEnemy(this);
    }

    public void OnPointerEnter(PointerEventData data)
    {
        Color tempColor = _image.color;
        tempColor.a = 0.5f;
        _image.color = tempColor;

        if (!GameManagerMap.Instance.StatusMain.ActualPermissions.Contains(Permissions.SelectEnemy)
            && GameManagerMap.Instance.StatusMain.ActualPermissions.Contains(Permissions.SelectPlaceToMovement))//(GameManagerMap.Instance.State == GameState.FreeMovement)
        {
            Vector3 position = CameraHelpers.CalculateCameraLookAt(_enemy, Camera.main);
            GameManagerMap.Instance.FixedCameraController
                .InitAsMainCamera(position, GameManagerMap.Instance.FreeCameraController.TargetRotation, 0.5f);
        }
    }

    public void OnPointerExit(PointerEventData data)
    {
        Color tempColor = _image.color;
        tempColor.a = 1f;
        _image.color = tempColor;

        if (!GameManagerMap.Instance.StatusMain.ActualPermissions.Contains(Permissions.SelectEnemy)
            && GameManagerMap.Instance.StatusMain.ActualPermissions.Contains(Permissions.SelectPlaceToMovement))//(GameManagerMap.Instance.State == GameState.FreeMovement)
            GameManagerMap.Instance.FixedCameraController.InitAsMainCamera(
                GameManagerMap.Instance.FreeCameraController.transform.position,
                GameManagerMap.Instance.FreeCameraController.transform.rotation,
                0.5f,
                GameManagerMap.Instance.FreeCameraController.InitAsMainCamera
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
                                    shooter: GameManagerMap.Instance.CharacterMovement.SelectedCharacter.ActualTerritory,
                                    gun: GameManagerMap.Instance.CharacterMovement.SelectedCharacter.WeaponCharacter,
                                    GameManagerMap.Instance.CharacterMovement.SelectedCharacter.BaseAimCharacter);

        Percent = resultCaclulations.percent;
        _textPercent.text = resultCaclulations.percent.ToString() + "%";

        if (resultCaclulations.status == ShelterType.None)
            _textPercent.color = Color.yellow;
        else
            _textPercent.color = Color.red;
    }

}
