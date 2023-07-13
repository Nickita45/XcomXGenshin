using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectBlock : MonoBehaviour
{
    public event Action<GameObject> OnDetectItem;


    private void OnTriggerExit(Collider other)
    {
        OnDetectItem(null);
    }

    private void OnTriggerEnter(Collider other)
    {
        OnDetectItem(other.gameObject);

    }
}
