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

        // Apply damping factor for smooth movement
        float smoothness = 0.1f;
        transform.position = Vector3.Lerp(transform.position, position, smoothness);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, smoothness);
    }
}