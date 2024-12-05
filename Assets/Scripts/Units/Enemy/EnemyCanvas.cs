using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyCanvas : UnitCanvas
{
    [SerializeField]
    private List<GameObject> _listDidntRotateObjects; //need to be HashSet

    [SerializeField]
    private csFogVisibilityAgentEnemy csFogVisibilityAgent;

    private HashSet<GameObject> _objectCantBeRotated;

    public override void Start()
    {
        base.Start();
        _objectCantBeRotated = new HashSet<GameObject>(_listDidntRotateObjects);
    }

    public override void OnStatusChange(HashSet<Permissions> permissions)
    {
        if (permissions.Count == 0)
        {
            Manager.StatusMain.OnStatusChange -= OnStatusChange;
            return;
        }




        if (permissions.Contains(Permissions.SelectEnemy) || permissions.Contains(Permissions.AnimationShooting)
            || permissions.Contains(Permissions.NonFog)) //mb in future new permission Show Enemy Canvas or only NonFog
        {
            if ((ShootManager.TargetUnit == null || _canvas != ShootManager.TargetUnit.Canvas.CanvasGameObject)
                     && (Manager.EnemyPanel.Enemy == null || _canvas != Manager.EnemyPanel.Enemy.Canvas.CanvasGameObject))
                _canvas.gameObject.SetActive(false);
            else
                _canvas.gameObject.SetActive(true);
        }
        else
        {
            if (csFogVisibilityAgent.IsVisible)
            {
                _canvas.gameObject.SetActive(true);

                foreach (Transform child in _canvas.transform)
                {
                    if (!_objectCantBeRotated.Contains(child.gameObject))
                        child.localEulerAngles = new Vector3(0, 0, 0);
                }
            }

        }
    }
}
