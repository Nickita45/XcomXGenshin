using AnimationCameras;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimatedCamera : MonoBehaviour
{


    private Camera _camera;
    private List<ICameraAnimation> _animations;
    private ICameraAnimation _actualAnimation;

    private Unit _target;
    public Func<Transform> ChangeCanvasRules;
    public Unit Targets { 
        get
        {
            return _target;
        }
        set
        {
            _target = value;
            StopAllCoroutines();
            if (_actualAnimation == null)
                _actualAnimation = _animations[UnityEngine.Random.Range(0, _animations.Count())];
            else
                _actualAnimation = _actualAnimation.GetNextAnimation();

            if(_actualAnimation != null)
            {
                Manager.StatusMain.SetStatusShooting();

                StartCoroutine(_actualAnimation.CameraRotate(_target.transform));

                Camera.main.enabled = false;
                _camera.enabled = true;
                //_camera.enabled = true;
            }
        }
    }

    public bool IsMainCamera() => Camera.main == _camera;

    private void Instalization()
    {
        _animations = new List<ICameraAnimation>() { 
            new BehindCameraAnimation(_camera),
            //new FrontFaceCameraAnimation(_camera),
            new SimpleCameraAnimation(_camera)
        }; 
    }

    private void Start()
    {
        _camera = GetComponent<Camera>();
        Manager.StatusMain.OnStatusChange += OnStatusChange;
        Manager.Instance.OnClearMap += Instalization;
    }

    private void OnDestroy()
    {
        Manager.StatusMain.OnStatusChange -= OnStatusChange;
        Manager.Instance.OnClearMap -= Instalization;
    }

    private void OnStatusChange(HashSet<Permissions> permissions)
    {
        if (!permissions.Contains(Permissions.AnimationShooting))
        {// remake
            if(_camera.enabled)
                Manager.CameraManager.EnableFreeCameraMovement();
            _camera.enabled = false;
            _actualAnimation = null;
        }
    }

    

}
