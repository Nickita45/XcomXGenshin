using AnimationCameras;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimatedCamera : MonoBehaviour
{
    private Camera _camera;
    private List<ICameraAnimation> _animations; 

    private Unit _target;
    public Unit Targets { 
        get
        {
            return _target;
        }
        set
        {
            _target = value;
            StopAllCoroutines();
            var animation = _animations[Random.Range(0, _animations.Count())]; 
            if(animation.CanBeUsed(_target))
            {
                Manager.StatusMain.SetStatusShooting();

                StartCoroutine(animation.CameraRotate(_target.transform));
                _camera.enabled = true;
            }
        }
    }

    public bool IsCameraActive => _camera.enabled;

    private void Instalization()
    {
        _animations = new List<ICameraAnimation>() { 
            new BehindCameraAnimation(_camera),
            new FrontFaceCameraAnimation(_camera),
            new SimpleCameraAnimation(_camera) }; 
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
        if(!permissions.Contains(Permissions.AnimationShooting)) // remake
            _camera.enabled = false;
    }

    

}
