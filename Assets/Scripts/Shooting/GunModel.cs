using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A utility class which makes the gun model align with the character's hands.
// Only works with mixamo models.
public class GunModel : MonoBehaviour
{
    GameObject _rightHand;

    GameObject _leftHand;

    public void Init()
    {
        _rightHand = ObjectUtils.FindDescendantByName(transform.parent, "mixamorig:RightHand");
        _leftHand = ObjectUtils.FindDescendantByName(transform.parent, "mixamorig:LeftHand");
    }

    void Update()
    {
        if (_leftHand && _rightHand)
        {
            Vector3 position = (_rightHand.transform.position + _leftHand.transform.position) / 2f;

            Vector3 direction = _leftHand.transform.position - _rightHand.transform.position;
            Quaternion rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

            transform.position = position;
            transform.rotation = rotation;
        }
    }
}