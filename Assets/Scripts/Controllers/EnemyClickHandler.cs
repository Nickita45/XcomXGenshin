using System.Linq;
using UnityEngine;

public class EnemyClickHandler : MonoBehaviour
{
    private EnemyUI _enemyUI;

    private void Start()
    {
        _enemyUI = FindObjectOfType<EnemyUI>();
    }

    private void Update()
    {
        if (GameManagerMap.Instance.State == GameState.FreeMovement
            && GameManagerMap.Instance.CharacterMovemovent.SelectedCharacter != null
            && Input.GetMouseButtonDown(0))
        {
            Camera camera = Camera.main;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);

            float closestDistance = int.MaxValue;
            GameObject closestEnemy = null;

            foreach (TerritoryInfo enemy in Physics.RaycastAll(ray)
                .Select(hit => hit.transform.GetComponent<TerritoryInfo>())
                .Where(info => info != null && info.Type == TerritoryType.Enemy))
            {
                float distance = Vector3.Distance(enemy.transform.position, camera.transform.position);
                if (closestEnemy == null || distance < closestDistance)
                {
                    closestEnemy = enemy.gameObject;
                    distance = closestDistance;
                }
            }

            if (closestEnemy && GameManagerMap.Instance.CharacterVisibility.VisibleEnemies.Contains(closestEnemy))
            {
                GameManagerMap.Instance.ViewEnemy(_enemyUI.GetIconForEnemy(closestEnemy));
            }
        }
    }
}
