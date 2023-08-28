using System;
using System.Linq;
using UnityEngine;

public class AimCalculater : MonoBehaviour
{
    public static int FULLGROUNDPROCENT = -40, SEMIGROUNDPROCENT = -20, HIGHGROUNDPROCENT = -20, LOWGRONTPROCENT = 30;


    private void Start()
    {
        //Config
        FULLGROUNDPROCENT = ConfigurationManager.Instance.GlobalDataJson.bonusAimFromFullCover;
        SEMIGROUNDPROCENT = ConfigurationManager.Instance.GlobalDataJson.bonusAimFromHalfCover;
        LOWGRONTPROCENT = ConfigurationManager.Instance.GlobalDataJson.bonusAimFromHighGround;
        HIGHGROUNDPROCENT = ConfigurationManager.Instance.GlobalDataJson.bonusAimFromLowGround;

    }

    public static (int procent, ShelterType status) CalculateShelterPercent(TerritroyReaded defender, TerritroyReaded shooter, GunType gun, params int[] parameters)
    {
        CordinatesSide xSide, zSide;
        if (shooter.ZPosition - defender.ZPosition < 0 && shooter.ZPosition - defender.ZPosition != -1)
        {
            xSide = CordinatesSide.Left;
        }
        else if (shooter.ZPosition - defender.ZPosition > 0 && shooter.ZPosition - defender.ZPosition != 1)
        {
            xSide = CordinatesSide.Right;
        }
        else
        {
            xSide = CordinatesSide.Nope;
        }

        if (shooter.XPosition - defender.XPosition < 0 && shooter.XPosition - defender.XPosition != -1)
        {
            zSide = CordinatesSide.Bottom;
        }
        else if (shooter.XPosition - defender.XPosition > 0 && shooter.XPosition - defender.XPosition != 1)
        {
            zSide = CordinatesSide.Front;
        }
        else
        {
            zSide = CordinatesSide.Nope;
        }

        int groundProcent = 0;
        if (shooter.YPosition - defender.YPosition < 0)
            groundProcent = HIGHGROUNDPROCENT;
        else if (shooter.YPosition - defender.YPosition > 0)
            groundProcent = LOWGRONTPROCENT;

        ShelterType maxValue = (ShelterType)Mathf.Max((int)GetShelterTypeByCordinateSide(defender, xSide), (int)GetShelterTypeByCordinateSide(defender, zSide));
        int result = (parameters.Sum() + GetProcentByType(maxValue) + groundProcent + GetProcentFromGunType(gun, (int)Mathf.Round(Vector3.Distance(defender.GetCordinats(), shooter.GetCordinats()))));
        //Debug.Log($"{parameters.Sum()}, {GetProcentByType(maxValue)}, {groundProcent}, {GetProcentFromGunType(gun, (int)Mathf.Round(Vector3.Distance(defender.GetCordinats(), shooter.GetCordinats())))}");
        return (Mathf.Min(result, 100), maxValue);

    }

    private static ShelterType GetShelterTypeByCordinateSide(TerritroyReaded teritory, CordinatesSide cordinatesSide)
    {
        switch (cordinatesSide)
        {
            case CordinatesSide.Left:
                return GameManagerMap.Instance.Map[teritory.IndexLeft.First()].ShelterType.Right;
            case CordinatesSide.Right:
                return GameManagerMap.Instance.Map[teritory.IndexRight.First()].ShelterType.Left;
            case CordinatesSide.Bottom:
                return GameManagerMap.Instance.Map[teritory.IndexBottom.First()].ShelterType.Front;
            case CordinatesSide.Front:
                return GameManagerMap.Instance.Map[teritory.IndexFront.First()].ShelterType.Bottom;
            default:
                return ShelterType.Nope;
        }
    }

    private static int GetProcentByType(ShelterType shelterType)
    {
        switch (shelterType)
        {
            case ShelterType.Nope:
                return 0;
            case ShelterType.Semi:
                return SEMIGROUNDPROCENT;
            case ShelterType.Full:
                return FULLGROUNDPROCENT;
        }
        return 0;
    }

    private static int GetProcentFromGunType(GunType gunType, int distance)
    {
        /* switch(gunType)
         {
             case GunType.Automatic:
                 return 20 - 2 * distance;
             case GunType.Shotgun:
                 return 16 - 4 * distance;
             case GunType.Snipergun:
                 return 3 * distance - 15;
         }
     */
        return ConfigurationManager.Instance.GlobalDataJson.typeGun[(int)gunType].baseValue +
            ConfigurationManager.Instance.GlobalDataJson.typeGun[(int)gunType].distanceValue * distance;
    }
}

enum CordinatesSide
{
    Nope,
    Left,
    Right,
    Front,
    Bottom
}

public enum GunType
{
    Automatic,
    Shotgun,
    Snipergun
}

