using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICameraAnimation
{
    IEnumerator CameraRotate(Transform target);

    bool CanBeUsed(Unit target);
}
