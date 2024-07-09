using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AnimationCameras
{
    public interface ICameraAnimation
    {
        IEnumerator CameraRotate(Transform target);

        bool CanBeUsed(Unit target);
    }

    public class AnimationCameraBase
    {
        protected const float timeToEndAnimation = 0.5f;
        protected Camera _camera;

        public AnimationCameraBase(Camera camera) => _camera = camera;
    }
}
