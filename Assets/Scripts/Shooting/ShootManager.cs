using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class ShootManager : MonoBehaviour
{
    private const int MAX_SIZE = 32;
    private const int DEFAULT_SIZE = 16;
    

    [SerializeField]
    private GameObject _bulletPrefab;

    [SerializeField]
    private float _lifetime = 4f;

    [SerializeField]
    private string _nameBulletSpawner = "BulletSpawner"; //script ill find gameobject for spawning bullets with such name 

    private IObjectPool<Bullet> _pool;
    private bool collectionCheck = true;

    private void Start()
    {
        _pool = new ObjectPool<Bullet>(CreateParticleSystem,
                    OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject,
                    collectionCheck: collectionCheck, defaultCapacity: DEFAULT_SIZE, maxSize: MAX_SIZE);
    }


    public IEnumerator Shoot(
        Unit shooter,
        Unit defender,
        GunType actualGun,
        Element element,
        IEnumerator afterShootingBullets
    )
    {
        Transform firePoint = shooter.GetBulletSpawner(_nameBulletSpawner); //position for spawning bullets

        int dmg = (shooter.Stats as IShooter).RandomShootDmg();

        (int percent, _) =
            AimUtils.CalculateHitChance(shooter.ActualTerritory, defender.ActualTerritory, actualGun, shooter.Stats.BaseAimPercent()); //get chance

        defender.ChanceResistance.GetResistance(shooter, defender, actualGun, element, ref percent, ref dmg);

        var result = RandomExtensions.GetChance(percent) || defender is Entity; //mb change in future
        //Debug.Log($"{shooter.Stats.Name()} has next {percent} to hit and got {result}");
        for (int i = 0; i < ConfigurationManager.GlobalDataJson.typeGun[(int)actualGun].countBullets; i++)
        {
            Vector3 addShootRange = GenerateCoordinatesFromResult(!result); //getting a spread depending on the result of a hit

            //GameObject bullet = Instantiate(_bulletPrefab, firePoint.position, firePoint.rotation); //create bullet
            //Bullet bulletScript = bullet.GetComponent<Bullet>();
            Bullet bulletScript = _pool.Get();
            bulletScript.SetBasicSettings(firePoint);

            if (bulletScript != null && defender != null)
            {
                Vector3 directionToTarget = defender.transform.position - firePoint.position; //setting the direction
                bulletScript.transform.forward = directionToTarget + addShootRange;
            }
            StartCoroutine(ReturnToPoolAfterLifetime(bulletScript, _lifetime));
            yield return new WaitForSeconds(UnityEngine.Random.Range(ConfigurationManager.GlobalDataJson.typeGun[(int)actualGun].minTimeBetweenShooting,
                ConfigurationManager.GlobalDataJson.typeGun[(int)actualGun].maxTimeBetweenShooting));
        }

        if (afterShootingBullets != null)
            yield return StartCoroutine(afterShootingBullets); //callback

        if (!result)
            StartCoroutine(defender.Canvas.PanelShow(defender.Canvas.PanelMiss, 4)); //show panel miss
        else
            defender.Health.MakeHit(dmg, element, shooter);
        
        yield return new WaitForSeconds(ConfigurationManager.GlobalDataJson.timeAfterShooting);
    }

    private Vector3 GenerateCoordinatesFromResult(bool miss)
    {
        if (!miss)
            return new Vector3(UnityEngine.Random.Range(-0.15f, 0.15f), UnityEngine.Random.Range(-0.15f, 0.15f), UnityEngine.Random.Range(-0.15f, 0.15f));//spread for hit
        else
        { //spread for missing
            int minus = UnityEngine.Random.Range(0, 2); //because [0;2)
            return new Vector3(UnityEngine.Random.Range(-0.55f, -0.25f) * minus + UnityEngine.Random.Range(0.25f, 0.55f) * ((minus + 1) % 2),
                UnityEngine.Random.Range(-0.55f, -0.25f) * minus + UnityEngine.Random.Range(0.25f, 0.55f) * ((minus + 1) % 2),
                UnityEngine.Random.Range(-0.55f, -0.25f) * minus + UnityEngine.Random.Range(0.25f, 0.55f) * ((minus + 1) % 2));
        }
    }

    #region Object Pool
    private Bullet CreateParticleSystem()
    {
        GameObject obj = Instantiate(_bulletPrefab);
        Bullet particleSystem = obj.GetComponent<Bullet>();

        return particleSystem;
    }
    private void OnGetFromPool(Bullet pooledObject)
    {
        pooledObject.gameObject.SetActive(true);
    }
    private void OnReleaseToPool(Bullet pooledObject)
    {
        pooledObject.gameObject.SetActive(false);
    }
    private void OnDestroyPooledObject(Bullet pooledObject)
    {   
        Destroy(pooledObject.gameObject);
    }

    public IEnumerator ReturnToPoolAfterLifetime(Bullet pooledObject, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        _pool.Release(pooledObject);
    }
    #endregion
}
