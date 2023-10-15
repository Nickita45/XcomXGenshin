using System;
using System.Collections;
using UnityEngine;

public class ShootManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _bulletPrefab;

    [SerializeField]
    private string _nameBulletSpawner = "BulletSpawner";

    public IEnumerator Shoot(
        Unit shooter,
        Unit defender,
        GunType actualGun,
        IEnumerator afterShootingBullets
    )
    {
        Transform firePoint = shooter.GetBulletSpawner(_nameBulletSpawner);

        int randomGenerate = UnityEngine.Random.Range(0, 101); // [0;101)

        (int percent, _) =
        // Manager.map for enemy?
            AimUtils.CalculateHitChance(shooter.ActualTerritory, defender.ActualTerritory, actualGun, shooter.Stats.BaseAimPercent());

        for (int i = 0; i < ConfigurationManager.GlobalDataJson.typeGun[(int)actualGun].countBullets; i++)
        {
            Vector3 addShootRange = GenerateCoordinatesFromResult(randomGenerate > percent);

            GameObject bullet = Instantiate(_bulletPrefab, firePoint.position, firePoint.rotation);
            Bullet bulletScript = bullet.GetComponent<Bullet>();

            if (bulletScript != null && defender != null)
            {
                Vector3 directionToTarget = defender.transform.position - firePoint.position;
                bullet.transform.forward = directionToTarget + addShootRange;
            }

            yield return new WaitForSeconds(UnityEngine.Random.Range(ConfigurationManager.GlobalDataJson.typeGun[(int)actualGun].minTimeBetweenShooting,
                ConfigurationManager.GlobalDataJson.typeGun[(int)actualGun].maxTimeBetweenShooting));
        }

        if (afterShootingBullets != null)
            yield return StartCoroutine(afterShootingBullets);

        if (randomGenerate > percent)
        {
            StartCoroutine(defender.Canvas.PanelShow(defender.Canvas.PanelMiss, 4));
        }
        else
        {
            int dmg;
            if (shooter is Enemy enemy)
            {
                dmg = enemy.GetRandomDmg();
            }
            else
            {
                dmg = UnityEngine.Random.Range(ConfigurationManager.GlobalDataJson.typeGun[(int)actualGun].minHitValue, ConfigurationManager.GlobalDataJson.typeGun[(int)actualGun].maxHitValue + 1);
            }

            StartCoroutine(defender.Canvas.PanelShow(defender.Canvas.PanelHit(dmg), 4));
            defender.MakeHit(dmg);
        }
        yield return new WaitForSeconds(ConfigurationManager.GlobalDataJson.timeAfterShooting);
    }

    private Vector3 GenerateCoordinatesFromResult(bool miss)
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
