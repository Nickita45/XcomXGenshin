using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private GameObject _cameraParent;
    [SerializeField]
    private Transform _targetObject;
    [SerializeField]
    private GameObject _mainObject;

    [SerializeField]
    private float _speed = 5f;
    [SerializeField]
    private float _speedRotation = 5f;

    private bool _isRotating = false;
    private Quaternion _targetRotation;
    private readonly Vector3 rotationAxis = Vector3.up;

    private void Start()
    {
        //ChangePositionForCamera();
    }

    private void Update()
    {
        HandleMovementInput();
        HandleRotationInput(KeyCode.E, -90f);
        HandleRotationInput(KeyCode.Q, 90f);
        HandleRotation();
    }

    private void HandleMovementInput()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = -Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(verticalInput, 0f, horizontalInput) * _speed * Time.deltaTime;

        movement = _cameraParent.transform.TransformDirection(movement);
        Vector3 newPosition = _mainObject.transform.position + movement;

        IsWithinBoundary(ref newPosition);
        _mainObject.transform.position = newPosition;
    }

    private void IsWithinBoundary(ref Vector3 position)
    {
        Vector3 center = transform.position;
        (float width, float height) = (GameManagerMap.Instance.Map.width, GameManagerMap.Instance.Map.width);
        float cameraHeight = position.y - transform.position.y;

        // Find the difference vector between the projected point
        // and the camera position (assuming the camera angle is 45 degrees)
        Vector3 diff = _camera.transform.forward * cameraHeight * Mathf.Sqrt(2);

        // Find the coordinates of the projected point
        Vector3 projected = position + diff;
        //Debug.DrawRay(position, _camera.transform.forward * 8f, Color.red, duration: 1f);

        // Create the bounding rect
        Rect bounds = new(center.x - width / 2f, center.z - height / 2f, width, height);

        // Keep the position inside bounds
        if (projected.x < bounds.xMin) projected.x = bounds.xMin;
        else if (projected.x > bounds.xMax) projected.x = bounds.xMax;

        if (projected.z < bounds.yMin) projected.z = bounds.yMin;
        else if (projected.z > bounds.yMax) projected.z = bounds.yMax;

        position = projected - diff;
    }

    private void HandleRotationInput(KeyCode keyCode, float angle)
    {
        if (Input.GetKeyDown(keyCode) && !_isRotating)
        {
            _targetRotation = _cameraParent.transform.rotation * Quaternion.AngleAxis(angle, rotationAxis);
            _isRotating = true;
        }
    }
    private void HandleRotation()
    {
        if (_isRotating)
        {
            Vector3 directionToTarget = _targetObject.position - _cameraParent.transform.position;
            Vector3 rotatedDirection = _targetRotation * directionToTarget;

            float step = _speedRotation * Time.deltaTime;
            _cameraParent.transform.rotation = Quaternion.RotateTowards(_cameraParent.transform.rotation, _targetRotation, step);

            //ChangePositionForCamera();

            if (Quaternion.Angle(_cameraParent.transform.rotation, _targetRotation) < 0.01f)
            {
                _isRotating = false;
                _cameraParent.transform.rotation = _targetRotation;
            }
        }
    }
    /*public void ChangePositionForCamera()
    {
        Vector3 newPositionForRotate = _targetObject.position - _camera.transform.forward * Vector3.Distance(_targetObject.position, _camera.transform.position);
        newPositionForRotate.y = _cameraParent.transform.position.y;
        _cameraParent.transform.position = newPositionForRotate;
    }*/
}
