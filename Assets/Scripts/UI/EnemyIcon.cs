using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
                .InitAsMainCamera(position, Manager.CameraManager.FreeCamera.TargetRotation, 0.5f);
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
                0.5f,
                freeCamera.InitAsMainCamera
            );
        }
    }

    public void SetEnemy(Enemy enemy)
    {
        _enemy = enemy;
        _image.sprite = enemy.Icon;
    }

    public void SetPercent()
    {
        Character character = Manager.TurnManager.SelectedCharacter;

        (int percent, ShelterType status) =
            AimUtils.CalculateHitChance(
                character.ActualTerritory,
                _enemy.ActualTerritory,
                character.Stats.Weapon,
                character.Stats.BaseAimCharacter
            );

        _textPercent.text = percent.ToString() + "%";

        if (status == ShelterType.None)
            _textPercent.color = Color.yellow;
        else
            _textPercent.color = Color.red;
    }
}
