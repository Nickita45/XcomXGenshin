using System;
using System.Collections;
using UnityEngine;

public class ShootController : MonoBehaviour
{
    [SerializeField]
    private GameObject _bulletPrefab;

    [SerializeField]
    private string _nameBulletSpawner = "BulletSpawner";

    public IEnumerator Shoot(PersonInfo target, GunType actualGun, int procent, IEnumerator afterShootingBullets, PersonInfo character)
    {
        Transform firePoint = character.GetBulletSpawner(_nameBulletSpawner);

        int randomGenerate = UnityEngine.Random.Range(0, 101); // [0;101)

        for (int i = 0; i < ConfigurationManager.Instance.GlobalDataJson.typeGun[(int)actualGun].countBullets; i++)
        {
            Vector3 addShootRange = GenereteCordinatesFromResult(randomGenerate > procent);

            GameObject bullet = Instantiate(_bulletPrefab, firePoint.position, firePoint.rotation);
            Bullet bulletScript = bullet.GetComponent<Bullet>();

            if (bulletScript != null && target != null)
            {
                Vector3 directionToTarget = (target.transform.position - firePoint.position);
                bullet.transform.forward = directionToTarget + addShootRange;
            }

            yield return new WaitForSeconds(UnityEngine.Random.Range(ConfigurationManager.Instance.GlobalDataJson.typeGun[(int)actualGun].minTimeBetweenShooting,
                ConfigurationManager.Instance.GlobalDataJson.typeGun[(int)actualGun].maxTimeBetweenShooting));
        }
        //Here stop animation shooting
        if(afterShootingBullets != null)
            yield return StartCoroutine(afterShootingBullets);

        if (randomGenerate > procent)
        {
            StartCoroutine(target.CanvasController().PanelShow(target.CanvasController().PanelMiss, 4));
        }
        else
        {
            int dmg = UnityEngine.Random.Range(ConfigurationManager.Instance.GlobalDataJson.typeGun[(int)actualGun].minHitValue, ConfigurationManager.Instance.GlobalDataJson.typeGun[(int)actualGun].maxHitValue + 1);
            StartCoroutine(target.CanvasController().PanelShow(target.CanvasController().PanelHit(dmg), 4));
            target.MakeHit(dmg);

        }
        yield return new WaitForSeconds(ConfigurationManager.Instance.GlobalDataJson.timeAfterShooting);
    }

    private Vector3 GenereteCordinatesFromResult(bool miss)
    {
        if (!miss)
            return new Vector3(UnityEngine.Random.Range(-0.15f, 0.15f), UnityEngine.Random.Range(-0.15f, 0.15f), UnityEngine.Random.Range(-0.15f, 0.15f));
        else
        {
            int minus = UnityEngine.Random.Range(0, 2); //because [0;2)
            return new Vector3(UnityEngine.Random.Range(-0.55f, -0.25f) * minus + UnityEngine.Random.Range(0.25f, 0.55f) * ((minus + 1) % 2),
                UnityEngine.Random.Range(-0.55f, -0.25f) * minus + UnityEngine.Random.Range(0.25f, 0.55f) * ((minus + 1) % 2),
                UnityEngine.Random.Range(-0.55f, -0.25f) * minus + UnityEngine.Random.Range(0.25f, 0.55f) * ((minus + 1) % 2));
        }
    }
}
