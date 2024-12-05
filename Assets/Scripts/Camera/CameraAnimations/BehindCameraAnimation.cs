using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace AnimationCameras
{
    public class BehindCameraAnimation : AnimationCameraBase, ICameraAnimation
    {
        public BehindCameraAnimation(Camera camera) : base(camera) {}

        public IEnumerator CameraRotate(Transform target)
        {
            float timer = 0;
            var unitAnimator = target.GetComponent<Unit>().Animator;
            Transform bulletSpawner = unitAnimator.GunModel != null ? unitAnimator.GunModel.transform : target.GetComponent<Unit>().GetBulletSpawner("");

            _camera.transform.position = bulletSpawner.transform.position +
                -bulletSpawner.transform.forward * 1f + Vector3.up * 0.5f;


            while (timer < timeToEndAnimation)
            {
                Vector3 positionModel = ShootManager.TargetUnit.gameObject.transform.position;
                Vector3 positionGun = bulletSpawner != null ?
                    bulletSpawner.transform.position : unitAnimator.Model.transform.position;

                _camera.transform.LookAt((positionModel + positionGun) / 2);
                timer += Time.deltaTime;

                yield return null;
            }
        }

        public ICameraAnimation GetNextAnimation() => null;
    }
}
