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
    private float _speedRotation = 150;
    [SerializeField]
    private float _speedZoom = 60f;

    private bool _isRotating = false;

    private bool _isMoving = false;
    private Vector2 _targetMove;

    private float _zoom = 1f;
    [SerializeField]
    private float _zoomMin = 2.27f;
    [SerializeField]
    private float _zoomMax = 20.8f;

    private Quaternion _targetRotation;
    private readonly Vector3 rotationAxis = Vector3.up;

    private void Start()
    {
        //ChangePositionForCamera();
    }

    private void Update()
    {
        // Automatic movement
        if (_isMoving)
        {
            HandleMovement();
        }
        // Manual movement
        else
        {
            HandleMovementInput();
            HandleRotationInput(KeyCode.E, 90f);
            HandleRotationInput(KeyCode.Q, -90f);
            HandleZoomInput();
        }
        HandleZoom();
        HandleRotation();
    }

    // Find vector difference between the world coordinates at the center of the screen
    // and the camera position (assuming the camera angle is 45 degrees)
    private Vector3 GetVectorFromCameraToProjectedPoint(Vector3 position)
    {
        float cameraHeight = position.y - transform.position.y;
        return _camera.transform.forward * cameraHeight * Mathf.Sqrt(2);
    }

    private void HandleMovement()
    {
        // Get the projected point (the center of the screen)
        Vector3 diff = GetVectorFromCameraToProjectedPoint(_mainObject.transform.position);
        Vector3 projected = _mainObject.transform.position + diff;

        // Move the projected point to the target
        Vector2 projected2D = new(projected.x, projected.z);
        projected2D = Vector2.MoveTowards(projected2D, _targetMove, _speed * Time.deltaTime * 4);

        // Stop moving if reached the target
        if (Vector2.Distance(projected2D, _targetMove) < 0.0001f) _isMoving = false;

        // Convert back to the camera position and move
        (projected.x, projected.z) = (projected2D.x, projected2D.y);
        _mainObject.transform.position = projected - diff;
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
        Vector3 diff = GetVectorFromCameraToProjectedPoint(position);

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

    private void HandleZoom()
    {

        // Find the difference vector between the projected point
        // and the camera position (assuming the camera angle is 45 degrees)
        Vector3 diff = GetVectorFromCameraToProjectedPoint(_mainObject.transform.position);

        // Find the coordinates of the projected point
        Vector3 projected = _mainObject.transform.position + diff;

        // Find the target position after zoom
        Vector3 target = projected - _camera.transform.forward * _zoom;

        // Move to the target position
        if (Vector3.Distance(_mainObject.transform.position, target) > 0.001f)
        {
            Vector3 newPosition = Vector3.MoveTowards(_mainObject.transform.position, target, _speedZoom * Time.deltaTime);
            _mainObject.transform.position = newPosition;
        }
    }

    private void HandleZoomInput()
    {
        float input = -Input.mouseScrollDelta.y;
        _zoom += input * _speedZoom * 2 * Time.deltaTime;
        if (_zoom < _zoomMin) _zoom = _zoomMin;
        else if (_zoom > _zoomMax) _zoom = _zoomMax;
    }

    /*public void ChangePositionForCamera()
    {
        Vector3 newPositionForRotate = _targetObject.position - _camera.transform.forward * Vector3.Distance(_targetObject.position, _camera.transform.position);
        newPositionForRotate.y = _cameraParent.transform.position.y;
        _cameraParent.transform.position = newPositionForRotate;
    }*/

    public void MoveToPlayer()
    {
        CharacterInfo selectedCharacter = GameManagerMap.Instance.CharacterMovemovent.SelectedCharacter;
        // Only start moving if there is a selected character
        if (selectedCharacter != null)
        {
            _isMoving = true;

            Vector3 targetPosition = selectedCharacter.gameObject.transform.position;
            _targetMove = new(targetPosition.x, targetPosition.z);
        }
    }
}
