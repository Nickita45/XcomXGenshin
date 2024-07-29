using System.Collections;
using UnityEngine;


namespace AnimationCameras
{
    public class SimpleCameraAnimation : AnimationCameraBase, ICameraAnimation
    {
        private const float rotationSpeed = 2f;

        public SimpleCameraAnimation(Camera camera) : base(camera) {}

        public IEnumerator CameraRotate(Transform target)
        {
            float timer = 0;
            _camera.transform.position = target.position + new Vector3(-3.0f, 2.0f, 0);

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
}
