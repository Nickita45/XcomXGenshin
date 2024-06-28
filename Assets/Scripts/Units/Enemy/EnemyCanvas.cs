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
            if (Manager.EnemyPanel.Enemy == null || _canvas!= Manager.EnemyPanel.Enemy.Canvas.CanvasGameObject)
                _canvas.gameObject.SetActive(false);
            else
                _canvas.gameObject.SetActive(true);


            foreach (Transform child in _canvas.transform)
            {
                if (!_objectCantBeRotated.Contains(child.gameObject))
                    //child.localEulerAngles = new Vector3(0, 180, 0);

                if (permissions.Contains(Permissions.AnimationShooting))
                {
                   //_canvas.gameObject.SetActive(false);
                        //child.LookAt(Manager.TurnManager.SelectedCharacter.gameObject.transform);
                        //child.localEulerAngles += new Vector3(0, 180, 0);
                        if(Manager.CameraManager.AnimatedCamera.IsCameraActive)
                            _canvas.gameObject.SetActive(false);
                        //    child.LookAt(Manager.CameraManager.AnimatedCamera.transform.position);
                    }
            }
        }
        else
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
