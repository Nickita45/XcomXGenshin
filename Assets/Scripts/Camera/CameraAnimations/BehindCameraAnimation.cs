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

            _camera.transform.position = unitAnimator.GunModel.transform.position +
                -unitAnimator.GunModel.transform.forward * 1f + Vector3.up * 0.5f;


            while (timer < timeToEndAnimation)
            {
                Vector3 positionModel = Manager.EnemyPanel.Enemy.gameObject.transform.position;
                Vector3 positionGun = unitAnimator.GunModel != null ?
                    unitAnimator.GunModel.transform.position : unitAnimator.Model.transform.position;

                _camera.transform.LookAt((positionModel + positionGun) / 2);
                timer += Time.deltaTime;

                yield return null;
            }
        }

        public bool CanBeUsed(Unit target) => target is Character &&
                                    Manager.EnemyPanel.Enemy != null;
    }
}
