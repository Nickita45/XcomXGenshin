using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCanvasController : EnemyCanvasController
{
    [Header("Player")]
    [SerializeField]
    private GameObject[] _actions;

    public void SetCountActons(int count) //make children
    {
        for (int i = 0; i < _actions.Length; i++)
        {
            _actions[i].SetActive(i < count);
        }
    }

    protected override void OnStatusChange(HashSet<Permissions> permissions)
    {
        if (permissions.Count == 0)
        {
            GameManagerMap.Instance.StatusMain.OnStatusChange -= OnStatusChange;
            return;
        }


        if (permissions.Contains(Permissions.SelectEnemy) || permissions.Contains(Permissions.AnimationShooting))
            _canvasToMove.gameObject.SetActive(false);
        else
            _canvasToMove.gameObject.SetActive(true);
    }
}
