using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityCanvas : UnitCanvas
{
    public override void OnStatusChange(HashSet<Permissions> permissions)
    {
        if (permissions.Count == 0)
        {
            Manager.StatusMain.OnStatusChange -= OnStatusChange;
            return;
        }

        if (permissions.Contains(Permissions.SelectEnemy) || permissions.Contains(Permissions.AnimationShooting))
                _canvas.gameObject.SetActive(false);
        else
            _canvas.gameObject.SetActive(true);
    }
}
