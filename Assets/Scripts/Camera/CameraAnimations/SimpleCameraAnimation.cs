using System.Collections;
using UnityEngine;

public class SimpleCameraAnimation : ICameraAnimation
{
    private const float rotationSpeed = 2f;
    private const float timeToEndAnimation = 2f;
    private Camera _camera;

    public SimpleCameraAnimation(Camera camera) => _camera = camera;

    public IEnumerator CameraRotate(Transform target)
    {
        float timer = 0;
        _camera.transform.position = target.position + new Vector3(-2.0f, 1.0f, 0);

        while (timer < timeToEndAnimation)
        {
            float angle = rotationSpeed * Time.deltaTime;

            _camera.transform.RotateAround(target.position, Vector3.up, angle);
            _camera.transform.LookAt(target.position);
            timer += Time.deltaTime;

            yield return null;
        }
    }

    public bool CanBeUsed(Unit target) => true;
}
