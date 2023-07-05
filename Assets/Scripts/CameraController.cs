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
    private GameObject mainObject;

    [SerializeField]
    private float _speed = 5f;
    [SerializeField]
    private float _speedRotation = 5f;

    private bool _isRotating = false;
    private Quaternion _targetRotation;
    private readonly Vector3 rotationAxis = Vector3.up;
    
    private void Start()
    {
        ChangePositionForCamera();
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
        Vector3 newPosition = mainObject.transform.position + movement;
    
        // Check if the new position is within the boundary
        if (IsWithinBoundary(newPosition))
        {
            mainObject.transform.position = newPosition;
        }
        
    }
    private bool IsWithinBoundary(Vector3 position)
    {
        // Making the width more distance to working camera 
        float widthValuePlus = 3f;
        // Define the center and width of the boundary
        Vector3 boundaryCenter = GameManagerMap.Instance.MainParent.transform.position;
        boundaryCenter.x += 5;
        float boundaryWidth = GameManagerMap.Instance.Map.width + widthValuePlus;

        // Calculate the minimum and maximum values for the x and z coordinates of the boundary
        float minX = boundaryCenter.x - boundaryWidth / 2f;
        float maxX = boundaryCenter.x + boundaryWidth / 2f;
        float minZ = boundaryCenter.z - boundaryWidth / 2f;
        float maxZ = boundaryCenter.z + boundaryWidth / 2f;

        // Check if the position is within the boundary
        if (position.x >= minX && position.x <= maxX && position.z >= minZ && position.z <= maxZ)
        {
            return true;
        }

        return false;
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

            ChangePositionForCamera();

            if (Quaternion.Angle(_cameraParent.transform.rotation, _targetRotation) < 0.01f)
            {
                _isRotating = false;
                _cameraParent.transform.rotation = _targetRotation;
            }
        }
    }
    public void ChangePositionForCamera()
    {
        Vector3 newPositionForRotate = _targetObject.position - _camera.transform.forward * Vector3.Distance(_targetObject.position, _camera.transform.position);
        newPositionForRotate.y = _cameraParent.transform.position.y;
        _cameraParent.transform.position = newPositionForRotate;
    }
}
