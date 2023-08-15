using System;
using UnityEngine;

public class FixedCameraController : MonoBehaviour
{
    private Camera _camera;

    private Vector3 _startPosition;
    private Vector3 _targetPosition;

    private Quaternion _startRotation;
    private Quaternion _targetRotation;

    private float _t = int.MaxValue;
    private float _duration = 0;

    private bool _finished;
    private Action _onFinish;

    private void Start()
    {
        _camera = GetComponent<Camera>();
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
    public void Init(Vector3 targetPosition, Quaternion targetRotation, float transitionDuration)
    {
        Init(targetPosition, targetRotation, transitionDuration, null);
    }

    // Switches the game to this camera. Creates a smooth transition to the given position and rotation of the camera.
    // Performs an action when the transition ends.
    public void Init(Vector3 targetPosition, Quaternion targetRotation, float transitionDuration, Action onFinish)
    {
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
    }
}
