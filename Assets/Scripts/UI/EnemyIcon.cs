using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class EnemyIcon : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private EnemyTargetPanel _panel;

    private Enemy _enemy;
    public Enemy Enemy => _enemy;

    [SerializeField]
    private Image _image;
    public Image Image => _image;

    [SerializeField]
    private TextMeshProUGUI _textPercent;

    void Start()
    {
        _panel = transform.parent.GetComponent<EnemyTargetPanel>();
        SetPercent();
    }

    public void OnPointerClick(PointerEventData data)
    {
        Manager.AbilityPanel.SelectShootAbility();
        _panel.SelectEnemy(this);
    }

    public void OnPointerEnter(PointerEventData data)
    {
        Color tempColor = _image.color;
        tempColor.a = 0.5f;
        _image.color = tempColor;

        if (!Manager.HasPermission(Permissions.SelectEnemy)
            && Manager.HasPermission(Permissions.SelectPlaceToMovement))
        {
            Vector3 position = CameraUtils.CalculateCameraLookAt(_enemy.gameObject, Camera.main);
            Manager.CameraManager.FixedCamera
                .InitAsMainCamera(position, Manager.CameraManager.FreeCamera.TargetRotation, _enemy.gameObject, 0.5f);
        }
    }

    public void OnPointerExit(PointerEventData data)
    {
        Color tempColor = _image.color;
        tempColor.a = 1f;
        _image.color = tempColor;

        if (!Manager.HasPermission(Permissions.SelectEnemy)
            && Manager.HasPermission(Permissions.SelectPlaceToMovement))
        {
            FreeCamera freeCamera = Manager.CameraManager.FreeCamera;
            Manager.CameraManager.FixedCamera.InitAsMainCamera(
                freeCamera.transform.position,
                freeCamera.transform.rotation,
                null,
                0.5f,
                freeCamera.InitAsMainCamera
            );
        }
    }

    public void SetEnemy(Enemy enemy)
    {
        _enemy = enemy;
        _image.sprite = enemy.Stats.Icon;
    }

    public void SetPercent()
    {
        (int percent, ShelterType shelter) calculation;

        if ( Manager.AbilityPanel.Selected?.Ability is IPercent percentCalculation)
        {
            calculation = percentCalculation.GetCalculationProcents(_enemy);
        } else
        {
            Character character = Manager.TurnManager.SelectedCharacter;

            calculation =
                AimUtils.CalculateHitChance(
                    character.ActualTerritory,
                    _enemy.ActualTerritory,
                    character.Stats.Weapon,
                    character.Stats.BaseAimPercent()
                );
        }


        _textPercent.text = calculation.percent.ToString() + "%";

        if (calculation.shelter == ShelterType.None)
            _textPercent.color = Color.yellow;
        else
            _textPercent.color = Color.red;
    }
}
