using System.Collections;
using Unity.VisualScripting;
using UnityEngine;


namespace AnimationCameras
{
    public class FrontFaceCameraAnimation : AnimationCameraBase, ICameraAnimation
    {
        private const float _distance = 2f;
        private const float height = 2.4f;
        private const float heightModel = 1.7f;

        public FrontFaceCameraAnimation(Camera camera) : base(camera) {}

        public IEnumerator CameraRotate(Transform target)
        {
            Debug.Log("I SHOW SPEED");
            float timer = 0;
            var unitAnimator = target.GetComponent<Unit>().Animator;


            _camera.transform.position = unitAnimator.Model.transform.position +
                unitAnimator.Model.transform.forward * _distance + Vector3.up * height;

            while (timer < timeToEndAnimation)
            {
                Vector3 positionModel = unitAnimator.Model.transform.position;
                Vector3 positionGun = unitAnimator.GunModel != null ?
                    unitAnimator.GunModel.transform.position : unitAnimator.Model.transform.position;

                _camera.transform.LookAt((positionModel + positionGun) / 2);
                //_camera.transform.LookAt(new Vector3(positionModel.x, heightModel, positionModel.z));//(positionModel + positionGun) / 2);
                timer += Time.deltaTime;

                yield return null;
            }
        }

        public ICameraAnimation GetNextAnimation() => null;
    }
}