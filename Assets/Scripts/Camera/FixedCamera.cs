using System;
using System.Collections.Generic;
using UnityEngine;

public class FixedCamera : MonoBehaviour
{
    private Camera _camera;
    //private CameraObjectTransparency _cameraTransparentObjects;

    private Vector3 _startPosition;
    private Vector3 _targetPosition;

    private Quaternion _startRotation;
    private Quaternion _targetRotation;

    private float _t = int.MaxValue;
    private float _duration = 0;

    private bool _finished;
    private Action _onFinish;

    private List<GameObject> _objectsToHide = new(); //objects that we will hide
    private bool _canHide;

    private void Start()
    {
        _camera = GetComponent<Camera>();
        //_cameraTransparentObjects = GetComponent<CameraObjectTransparency>();
    }

    private void Update()
    {
        if (_t < 1f)
        {
            _t += Time.deltaTime / _duration;
            _t = Mathf.Clamp01(_t);

            _camera.transform.position = Vector3.Lerp(_startPosition, _targetPosition, _t);
            _camera.transform.rotation = Quaternion.Lerp(_startRotation, _targetRotation, _t);
        }
        else if (!_finished)
        {
            _finished = true;
            _onFinish?.Invoke();

        }
    }

    // Switches the game to this camera. Creates a smooth transition to the given position and rotation of the camera.
    public void InitAsMainCamera(Vector3 targetPosition, Quaternion targetRotation, GameObject focus, float transitionDuration)
    {
        InitAsMainCamera(targetPosition, targetRotation, focus, transitionDuration, null);
    }

    // Switches the game to this camera. Creates a smooth transition to the given position and rotation of the camera.
    // Performs an action when the transition ends.
    public void InitAsMainCamera(Vector3 targetPosition, Quaternion targetRotation, GameObject focus, float transitionDuration, Action onFinish)
    {
        // Do not init camera if the target is the same.
        // This prevents unnecessary resetting of the timer.
        if (IsMainCamera() && targetPosition == _targetPosition && targetRotation == _targetRotation)
        {
            return;
        }

        _startPosition = Camera.main.transform.position;
        _targetPosition = targetPosition;

        _startRotation = Camera.main.transform.rotation;
        _targetRotation = targetRotation;

        _duration = transitionDuration;

        Camera.main.enabled = false;
        _camera.enabled = true;

        _t = 0;

        _finished = false;
        _onFinish = onFinish;

        // Make all hidable objects from the target position to the focus transparent
        if (focus != null)
        {
            Manager.CameraObjectTransparency.HideObjectsInLine(_targetPosition, focus.transform.position);
        }

        // Update camera to setup starting position in the same frame
        Update();
    }

    public bool IsMainCamera() => Camera.main == _camera;

    private void FinishingDetect() //hide objects in textures
    {
        _canHide = true;
    }

    private void OnTriggerStay(Collider other) //we use this when camera is in object
    {
        if (_canHide)
        {
            _objectsToHide.Add(other.gameObject);
            other.gameObject.SetActive(false);
        }
    }

    public void ClearListHide() //return everything in active status
    {
        _objectsToHide.ForEach(n => n.SetActive(true));
        _objectsToHide.Clear();
        _canHide = false;
    }
}
