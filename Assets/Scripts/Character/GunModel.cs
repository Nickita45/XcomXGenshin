using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunModel : MonoBehaviour
{
    [SerializeField]
    GameObject _rightHand;

    [SerializeField]
    GameObject _leftHand;

    void Update()
    {
        Vector3 position = (_rightHand.transform.position + _leftHand.transform.position) / 2f;

        Vector3 direction = _leftHand.transform.position - _rightHand.transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

        transform.position = position;
        transform.rotation = rotation;
    }
}