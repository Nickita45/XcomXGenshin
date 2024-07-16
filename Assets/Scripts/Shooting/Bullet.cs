using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private float _speed = 10f;


    private void Update()
    {
        Vector3 moveDirection = transform.forward * _speed * Time.deltaTime;
        transform.Translate(moveDirection, Space.World);
    }

    public void SetBasicSettings(Transform firepoint)
    {
        transform.position = firepoint.position;
        transform.rotation = firepoint.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
       // if (other.gameObject.GetComponent<TerritoryInfo>() && other.gameObject.GetComponent<TerritoryInfo>().Type != TerritoryType.Undefined && other.gameObject.GetComponent<TerritoryInfo>().Type != TerritoryType.Decor)
       //     Destroy(gameObject);
    }
}
