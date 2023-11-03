using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private float _speed = 10f;

    [SerializeField]
    private float _lifetime = 4f; 
    
    [SerializeField]
    private int _dmg = 30;

    public GameObject SpawnedGameObjcet { get; set; }
    public bool IsHit { get; set; }

    private void Update()
    {
        Vector3 moveDirection = transform.forward * _speed * Time.deltaTime;
        transform.Translate(moveDirection, Space.World);
        Destroy(gameObject, _lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsHit) {
            if (other is CapsuleCollider && other.gameObject.GetComponent<EnemyInfo>())
                Destroy(gameObject);
        } else
        {
            if(other is BoxCollider && !other.gameObject.GetComponent<EnemyInfo>() && Vector3.Distance(SpawnedGameObjcet.transform.position, transform.position) > 3)
            {
                if(!other.GetComponent<TerritoryInfo>())
                {
                    return;
                }

                other.gameObject.GetComponent<TerritoryInfo>().MakeDmg(_dmg);
                Destroy(gameObject);
            }
        }
    }
}
