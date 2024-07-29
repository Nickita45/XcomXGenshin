using ParticleSystemFactory;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [SerializeField]
    private float _speed = 10f;

    [SerializeField]
    private TerritoryTypeFlags _destroyOnHit;

    private Rigidbody _rigidbody;
    private bool isHitBullet;
    private void Start()
    {
    }

    private void Update()
    {
        //Vector3 moveDirection = transform.forward * _speed * Time.deltaTime;
        //transform.Translate(moveDirection, Space.World);
    }

    public void SetBasicSettings(Transform firepoint, Vector3 directionToTarget, Vector3 addShootRange, bool isHit)
    {
        if (_rigidbody == null)
            _rigidbody = GetComponent<Rigidbody>();

        isHitBullet = isHit;
        transform.position = firepoint.position;
        Vector3 shootDirection = (directionToTarget + addShootRange).normalized;

        transform.forward = shootDirection;

        _rigidbody.AddForce(shootDirection * _speed, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        

        if (isHitBullet)
            if (other.tag == "BulletsDontDestroyer")
                BulletHit(other);
        else
            if (other.gameObject.TryGetComponent(out TerritoryInfo info) && _destroyOnHit.HasFlag((TerritoryTypeFlags) info.Type))
                BulletHit(other);
    }

    private void BulletHit(Collider other)
    {
        Vector3 pointOnOtherCollider = other.ClosestPoint(transform.position);
        ParticleSystemFactoryCreator.CreateParticle(ParticleType.HitEffect, new ParticleData
        (
                position: pointOnOtherCollider
        ));
        gameObject.SetActive(false);
    }
}
