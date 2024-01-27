using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private float _speed = 10f;
    
    [SerializeField]
    private float _lifetime = 4f;


    private void Update()
    {
        Vector3 moveDirection = transform.forward * _speed * Time.deltaTime;
        transform.Translate(moveDirection, Space.World); //moving
        Destroy(gameObject, _lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
       // if (other.gameObject.GetComponent<TerritoryInfo>() && other.gameObject.GetComponent<TerritoryInfo>().Type != TerritoryType.Undefined && other.gameObject.GetComponent<TerritoryInfo>().Type != TerritoryType.Decor)
       //     Destroy(gameObject);
    }
}
