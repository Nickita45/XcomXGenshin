using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
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
    private Quaternion? _targetRotation;

    private bool _isMoving = false;
    private Vector3 _targetPosition;

    private Quaternion _savedRotation;

    private void Update()
    {
        if (GameManagerMap.Instance.Map == null) return;

        if (_isMoving) HandleMovement();
        if (_isRotating) HandleRotation();

        switch (GameManagerMap.Instance.State)
        {
            case GameState.FreeMovement:

                if (!_isMoving) HandleMovementInput();

                if (!_isRotating)
                {
                    HandleRotationInput(KeyCode.E, 90f);
                    HandleRotationInput(KeyCode.Q, -90f);
                }

                if (!_isMoving && !_isRotating)
                    HandleZoomInput();
                HandleZoom();

                break;
            case GameState.ViewEnemy:
                break;
        }
    }

    // Gets the difference vector between world coordinates at the center of the screen
    // and the camera position.
    //
    // Only applicable when the camera angle is 45 degrees down, such as in free movement mode.
    private Vector3 GetVectorFromCameraToProjectedPoint()
    {
        float cameraHeight = _camera.transform.position.y - transform.position.y;
        return _camera.transform.forward * cameraHeight * Mathf.Sqrt(2);
    }

    private void HandleMovement()
    {
        _camera.transform.position = Vector3.MoveTowards(_camera.transform.position, _targetPosition, _speed * Time.deltaTime * 4);

        if (Vector3.Distance(_camera.transform.position, _targetPosition) < 0.5f)
        {
            _isMoving = false;
            _camera.transform.position = _targetPosition;
        }
    }

    private void HandleMovementInput()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Get the movement vectors
        Vector3 cameraForward = _camera.transform.forward;
        Vector3 cameraRight = _camera.transform.right;

        // Remove the Y component to make sure there is no vertical movement
        cameraForward.y = 0.0f;
        cameraRight.y = 0.0f;

        // Normalize to get direction vectors
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Calculate the movement input vector
        Vector3 input = cameraForward * verticalInput + cameraRight * horizontalInput;

        Vector3 newPosition = _camera.transform.position + input * _speed * Time.deltaTime;
        ApplyBounds(ref newPosition);

        _camera.transform.position = newPosition;
    }

    private void HandleRotation()
    {
        if (_targetRotation.HasValue)
        {
            float step = _speedRotation * Time.deltaTime;
            _camera.transform.rotation = Quaternion.RotateTowards(_camera.transform.rotation, _targetRotation.Value, step);

            if (Quaternion.Angle(_camera.transform.rotation, _targetRotation.Value) < 0.01f)
            {
                _isRotating = false;
                _camera.transform.rotation = _targetRotation.Value;
            }

            Vector3 position = _camera.transform.position;
            bool moved = ApplyBounds(ref position);
            if (moved) MoveCamera(position);
        }
    }

    private void HandleRotationInput(KeyCode keyCode, float angle)
    {
        if (Input.GetKeyDown(keyCode) && !_isRotating)
        {
            _targetRotation = Quaternion.AngleAxis(angle, Vector3.up) * _camera.transform.rotation;
            _isRotating = true;
        }
    }

    private void HandleZoom()
    {
        // Find the difference vector between the projected point
        // and the camera position (assuming the camera angle is 45 degrees)
        Vector3 diff = GetVectorFromCameraToProjectedPoint();

        // Find the coordinates of the projected point
        Vector3 projected = _camera.transform.position + diff;

        // Find the target position after zoom
        Vector3 target = projected - _camera.transform.forward * _zoom;

        // Move to the target position
        if (Vector3.Distance(_camera.transform.position, target) > 0.001f)
        {
            Vector3 newPosition = Vector3.MoveTowards(_camera.transform.position, target, _speedZoom * Time.deltaTime);
            _camera.transform.position = newPosition;
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
        Vector3 center = transform.position;
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

    public void MoveToPlayer()
    {
        if (GameManagerMap.Instance.State == GameState.FreeMovement)
        {
            CharacterInfo selectedCharacter = GameManagerMap.Instance.CharacterMovemovent.SelectedCharacter;

            // Only start moving if there is a selected character and not rotating
            if (selectedCharacter != null && !_isRotating)
            {
                // Get the projected point (the center of the screen)
                Vector3 diff = GetVectorFromCameraToProjectedPoint();
                Vector3 position = selectedCharacter.gameObject.transform.position - diff;
                MoveCamera(position);
            }
        }
    }

    public void ViewEnemy(CharacterInfo selectedCharacter, GameObject enemy)
    {
        if (selectedCharacter)
        {
            Vector3 position = selectedCharacter.transform.position + (selectedCharacter.transform.position - enemy.transform.position).normalized * 2 + Vector3.up;
            MoveCamera(position);

            Quaternion rotation = Quaternion.LookRotation(enemy.transform.position - position);
            RotateCamera(rotation);
        }
    }

    // Save the camera rotation
    public void SaveCamera()
    {
        _savedRotation = _targetRotation ?? _camera.transform.rotation;
    }

    // Restore the camera rotation, as well as place it on top of the selected character
    public void RestoreCamera()
    {
        CharacterInfo selectedCharacter = GameManagerMap.Instance.CharacterMovemovent.SelectedCharacter;

        _camera.transform.rotation = _savedRotation;
        _targetRotation = _savedRotation;

        Vector3 position = selectedCharacter.transform.position + Vector3.up;
        _camera.transform.position = position;
        _targetPosition = position;
    }
}
