using System;
using System.Linq;
using UnityEngine;

// Sets up the camera, UI, state etc. to be able to select targets for the character's abilities.
//
// This class will be extended in future as new target types appear.
// For example, there might be an AOE attack that requires you to select an area.
public class TargetSelectManager : MonoBehaviour
{
    [SerializeField]
    private EnemyTargetPanel _enemyPanel;

    // Allows for the player to select a single enemy target.
    // 
    // The selection is done in a special viewing mode
    // where you could cycle through enemies with a dynamic camera.
    public void TargetEnemy(Action<object> onTargetSelect)
    {
        _enemyPanel.OnSelect(onTargetSelect);

        EnemyIcon selected = _enemyPanel.Selected;
        if (!selected) _enemyPanel.SelectLast();
        Manager.StatusMain.SetStatusSelectEnemy();
    }

    // Makes neccessary adjustments to make it clearer
    // that the character doing the ability IS the target for it.
    public void TargetSelf(Action<object> onTargetSelect)
    {
        Manager.OutlineManager.TargetCharacter(Manager.TurnManager.SelectedCharacter.Animator.Outline);
        Manager.CameraManager.EnableFreeCameraMovement();
    }

    private void Update()
    {
        // As an alternate way to select enemies, you could click on their in-game models.
        // Doing this would automatically select the shoot ability.
        if (Manager.HasPermission(Permissions.ActionSelect)
            && Manager.TurnManager.SelectedCharacter != null
            && Manager.CameraManager.FreeCamera.IsMainCamera())
        {
            Camera camera = Camera.main;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);

            float closestDistance = int.MaxValue;
            Enemy closestEnemy = null;

            foreach (Enemy enemy in Physics.RaycastAll(ray)
                .Select(hit => hit.transform.GetComponent<Enemy>())
                .Where(info => info != null))
            {
                float distance = Vector3.Distance(enemy.transform.position, camera.transform.position);
                if (closestEnemy == null || distance < closestDistance)
                {
                    closestEnemy = enemy;
                    closestDistance = distance;
                }
            }

            Character character = Manager.TurnManager.SelectedCharacter;

            if (closestEnemy && TargetUtils.CanTarget(character.transform, closestEnemy.transform, character.Stats.AttackRangedDistance()))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Manager.AbilityPanel.SelectShootAbility();
                    _enemyPanel.SelectEnemy(_enemyPanel.GetIconForEnemy(closestEnemy));
                }
                else
                {
                    Manager.OutlineManager.TargetEnemy(closestEnemy.Animator.Outline);
                }
            }
            else
            {
                Manager.OutlineManager.ClearEnemyTargets();
            }
        }
    }
}
