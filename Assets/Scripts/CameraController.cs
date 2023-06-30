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
    private float _speed = 5f;
    [SerializeField]
    private float _speedRotation = 5f;

    private bool isRotating = false;
    private Quaternion targetRotation;
    public Vector3 rotationAxis = Vector3.up;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = -Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(verticalInput, 0f, horizontalInput) * _speed * Time.deltaTime;

        _cameraParent.transform.position += movement;

        if (Input.GetKeyDown(KeyCode.Space) && !isRotating)
        {
            targetRotation = _camera.transform.rotation * Quaternion.AngleAxis(90f, rotationAxis);

            isRotating = true;
        }

        if (isRotating)
        {
            Debug.Log(targetRotation);
            Debug.Log(Quaternion.Angle(_camera.transform.rotation, targetRotation));
            _camera.transform.RotateAround(_targetObject.position, rotationAxis, _speedRotation * Time.deltaTime);

            if (Quaternion.Angle(_camera.transform.rotation, targetRotation) < 1f)
            {
                isRotating = false;
            }
        }

    }
    IEnumerator Rotation()
    {
        while(true)
        {
            _camera.transform.Rotate(_targetObject.transform.position, _speedRotation);

//            if (_camera.transform.eulerAngles == targetRotation)
            {
                break;
            }
            yield return new WaitForSeconds(0.05f);
        }
        isRotating = false;

    }
}
