using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Manages the cameras for the game.
//
// There are two different cameras at the moment:
// 1) Free Camera. Allows for free movemement, rotation and zooming of the camera.
// 2) Fixed Camera. Much more strict version of the camera,
//    used for static camera positions (e.g. when selecting an enemy target)
//    and for smooth camera transitions.
public class CameraManager : MonoBehaviour
{
    //TODO: maybe change all cameras types to non MonoBehaiorObjects

    [SerializeField]
    private FreeCamera _freeCamera;
    public FreeCamera FreeCamera => _freeCamera;

    [SerializeField]
    private FixedCamera _fixedCamera;
    public FixedCamera FixedCamera => _fixedCamera;

    [SerializeField]
    private AnimatedCamera _animatedCamera;
    public AnimatedCamera AnimatedCamera => _animatedCamera;

    public void EnableFreeCameraMovement() //TODO: change to OnStatusChange
    {
        _freeCamera.InitAsMainCamera();

        // If coming from enemy selection or shooting, perform additional setup
        if (
            Manager.HasPermission(Permissions.SelectEnemy) 
           // ||Manager.HasPermission(Permissions.AnimationShooting)
        )
        {
            Manager.OutlineManager.ClearTargets();
            Manager.EnemyPanel.ClearSelection();
            _freeCamera.TeleportToSelectedCharacter();
        }
    }

    // Transitions the camera to look at the given enemy.
    //
    // Used while selecting the enemy target for an ability.
    public void FixCameraOnEnemy(EnemyIcon icon) //TODO: change to OnStatusChange
    {
        if (Manager.HasPermission(Permissions.ActionSelect))
        {
            _fixedCamera.ClearListHide();

            (Vector3 position, Quaternion rotation)
                = CameraUtils.CalculateEnemyView(Manager.TurnManager.SelectedCharacter.gameObject, icon.Enemy.gameObject);
            _fixedCamera.InitAsMainCamera(position, rotation, icon.Enemy.gameObject, 0.5f);
        }
    }
}
