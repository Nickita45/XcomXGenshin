using System;
using UnityEditor;
using UnityEngine;

public class FreeCameraController : MonoBehaviour
{
    private Camera _camera;

    [SerializeField]
    private float _speed = 5f;
    [SerializeField]
    private float _speedRotation = 150;
    [SerializeField]
    private float _speedZoom = 60f;

    private float _zoom = 1f;
    [SerializeField]
    private float _zoomMin = 2.27f;
    [SerializeField]
    private float _zoomMax = 20.8f;

    private bool _isRotating = false;
    private Quaternion _targetRotation;
    public Quaternion TargetRotation => _targetRotation;

    private bool _isMoving = false;
    private Vector3 _targetPosition;

    private void Start()
    {
        _camera = GetComponent<Camera>();
        _targetRotation = transform.rotation;
    }

    private void Update()
    {
        if (GameManagerMap.Instance.Map == null) return;

        HandleMovement();
        HandleRotation();
        HandleZoom();
    }

    // Gets the difference vector between world coordinates at the center of the screen
    // and the camera position.
    //
    // Only applicable when the camera angle is 45 degrees down, such as in free movement mode.
    private Vector3 GetVectorFromCameraToProjectedPoint()
    {
        return transform.forward * transform.position.y * Mathf.Sqrt(2);
    }

    private void HandleMovement()
    {
        // Automatic movement
        if (_isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _speed * Time.deltaTime * 4);

            if (Vector3.Distance(transform.position, _targetPosition) < 0.5f)
            {
                _isMoving = false;
                transform.position = _targetPosition;
            }
        }
        // Manual movement
        else if (gameObject == Camera.main.gameObject)
        {
            HandleMovementInput();
        }
    }

    private void HandleMovementInput()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Get the movement vectors
        Vector3 cameraForward = transform.forward;
        Vector3 cameraRight = transform.right;

        // Remove the Y component to make sure there is no vertical movement
        cameraForward.y = 0.0f;
        cameraRight.y = 0.0f;

        // Normalize to get direction vectors
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Calculate the movement input vector
        Vector3 input = cameraForward * verticalInput + cameraRight * horizontalInput;

        Vector3 newPosition = transform.position + input * _speed * Time.deltaTime;
        if (GameManagerMap.Instance.Map != null && GameManagerMap.Instance.Map.width != 0) ApplyBounds(ref newPosition);

        transform.position = newPosition;
    }

    private void HandleRotation()
    {
        if (_isRotating)
        {
            float step = _speedRotation * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, step);

            if (Quaternion.Angle(transform.rotation, _targetRotation) < 0.01f)
            {
                _isRotating = false;
                transform.rotation = _targetRotation;
            }

            Vector3 position = transform.position;
            bool moved = ApplyBounds(ref position);
            if (moved) MoveCamera(position);
        }
        else if (gameObject == Camera.main.gameObject)
        {
            HandleRotationInput(KeyCode.E, 90f);
            HandleRotationInput(KeyCode.Q, -90f);
        }
    }

    private void HandleRotationInput(KeyCode keyCode, float angle)
    {
        if (Input.GetKeyDown(keyCode) && !_isRotating)
        {
            _targetRotation = Quaternion.AngleAxis(angle, Vector3.up) * transform.rotation;
            _isRotating = true;
        }
    }

    private void HandleZoom()
    {
        if (!_isMoving && !_isRotating)
        {
            if (gameObject == Camera.main.gameObject) HandleZoomInput();

            // Find the difference vector between the projected point
            // and the camera position (assuming the camera angle is 45 degrees)
            Vector3 diff = GetVectorFromCameraToProjectedPoint();

            // Find the coordinates of the projected point
            Vector3 projected = transform.position + diff;

            // Find the target position after zoom
            Vector3 target = projected - transform.forward * _zoom;

            // Move to the target position
            if (Vector3.Distance(transform.position, target) > 0.1f)
            {
                Vector3 newPosition = Vector3.MoveTowards(transform.position, target, _speedZoom * Time.deltaTime);
                transform.position = newPosition;
            }
        }
    }

    private void HandleZoomInput()
    {
        float input = -Input.mouseScrollDelta.y;
        _zoom += input * _speedZoom * 2 * Time.deltaTime;
        if (_zoom < _zoomMin) _zoom = _zoomMin;
        else if (_zoom > _zoomMax) _zoom = _zoomMax;
    }

    // Checks whether the given camera position is outside bounds of the map.
    // If it is, modifies the position to fit in the bounds.
    // 
    // Returns whether the position has been modified.
    private bool ApplyBounds(ref Vector3 position)
    {
        Vector3 center = GameManagerMap.Instance.transform.position;
        (float width, float height)
            = (GameManagerMap.Instance.Map.width, GameManagerMap.Instance.Map.width);

        // Find the difference vector between the projected point and the camera position
        Vector3 diff = GetVectorFromCameraToProjectedPoint();

        // Find coordinates of the projected point
        Vector3 projected = position + diff;

        // Create the bounding rect
        Rect bounds = new(center.x - width / 2f, center.z - height / 2f, width, height);

        bool moved = false;

        // Keep the position inside bounds
        if (projected.x < bounds.xMin)
        {
            moved = true;
            projected.x = bounds.xMin;
        }
        else if (projected.x > bounds.xMax)
        {
            moved = true;
            projected.x = bounds.xMax;
        }

        if (projected.z < bounds.yMin)
        {
            moved = true;
            projected.z = bounds.yMin;
        }
        else if (projected.z > bounds.yMax)
        {
            moved = true;
            projected.z = bounds.yMax;
        }

        position = projected - diff;
        return moved;
    }

    public void MoveCamera(Vector3 targetPosition)
    {
        _targetPosition = targetPosition;
        _isMoving = true;
    }

    public void RotateCamera(Quaternion rotation)
    {
        _targetRotation = rotation;
        _isRotating = true;
    }

    // Moves the camera to make it look at the given object.
    public void MoveTo(GameObject obj)
    {
        if (obj)
        {
            Vector3 diff = GetVectorFromCameraToProjectedPoint();

            Vector3 position = obj.transform.position;
            position.y = 0;

            MoveCamera(position - diff);
        }
    }

    public void MoveToSelectedCharacter()
    {
        CharacterInfo selectedCharacter = GameManagerMap.Instance.CharacterMovemovent.SelectedCharacter;
        if (selectedCharacter) MoveTo(selectedCharacter.gameObject);
    }

    public void TeleportToSelectedCharacter()
    {
        CharacterInfo selectedCharacter = GameManagerMap.Instance.CharacterMovemovent.SelectedCharacter;
        if (selectedCharacter)
        {
            Vector3 position = selectedCharacter.transform.position - transform.forward;
            transform.position = position;
            _targetPosition = position;
        }
    }

    // Switches the game to this camera.
    public void Init()
    {
        Camera.main.enabled = false;
        _camera.enabled = true;
    }
}
