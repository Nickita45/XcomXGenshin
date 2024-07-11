using System;
using System.Linq;
using UnityEngine;

public static class AimUtils
{
    public static int FULLGROUNDPERCENT = ConfigurationManager.GlobalDataJson.bonusAimFromFullCover;
    public static int SEMIGROUNDPERCENT = ConfigurationManager.GlobalDataJson.bonusAimFromHalfCover;
    public static int HIGHGROUNDPERCENT = ConfigurationManager.GlobalDataJson.bonusAimFromHighGround;
    public static int LOWGROUNDPERCENT = ConfigurationManager.GlobalDataJson.bonusAimFromLowGround;

    public static (int percent, ShelterType status) CalculateHitChance(TerritroyReaded shooter, TerritroyReaded defender, GunType gun, int baseAim)
    {
        ShelterType maxValue = GetMaxShelterType(shooter, defender);

        int groundPercent = 0;  //getting data on how high the opponent is relative to the shooter
        if (shooter.YPosition - defender.YPosition < 0)
            groundPercent = HIGHGROUNDPERCENT;
        else if (shooter.YPosition - defender.YPosition > 0)
            groundPercent = LOWGROUNDPERCENT;

        //getting the result according to the formula
        int result = baseAim + GetPercentByType(maxValue) +
            groundPercent + GetPercentFromGunType(gun, (int)Mathf.Round(Vector3.Distance(defender.GetCordinats(), shooter.GetCordinats())));
        return (Mathf.Min(Mathf.Max(result, 1), 100), maxValue);
    }

    public static (CordinatesSide xSide, CordinatesSide zSide) GetSidesFromShooterAndDefender(TerritroyReaded shooter, TerritroyReaded defender)
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
            xSide = CordinatesSide.Nope; //flank
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
            zSide = CordinatesSide.Nope; //flank
        }

        return (xSide, zSide);
    }

    public static ShelterType GetMaxShelterType(TerritroyReaded shooter, TerritroyReaded defender)
    {
        //getting data on which side the opponent is relative to the shooter
        (CordinatesSide xSide, CordinatesSide zSide) side =
            GetSidesFromShooterAndDefender(shooter, defender);

        //getting the type of maximum shelter
        ShelterType maxValue = (ShelterType)Mathf.Max((int)GetShelterTypeByCordinateSide(defender, side.xSide), (int)GetShelterTypeByCordinateSide(defender, side.zSide));

        return maxValue;
    }

    private static ShelterType GetShelterTypeByCordinateSide(TerritroyReaded teritory, CordinatesSide cordinatesSide)
    {
        switch (cordinatesSide)
        {
            case CordinatesSide.Left:
                return teritory.IndexLeft.Count > 0 ? Manager.Map[teritory.IndexLeft.First()].ShelterType.Right : ShelterType.None;
            case CordinatesSide.Right:
                return teritory.IndexRight.Count > 0 ? Manager.Map[teritory.IndexRight.First()].ShelterType.Left : ShelterType.None;
            case CordinatesSide.Bottom:
                return teritory.IndexBottom.Count > 0 ? Manager.Map[teritory.IndexBottom.First()].ShelterType.Front : ShelterType.None;
            case CordinatesSide.Front:
                return teritory.IndexFront.Count > 0 ? Manager.Map[teritory.IndexFront.First()].ShelterType.Bottom : ShelterType.None;
            default:
                return ShelterType.None; // if it is a flank
        }
    }

    private static int GetPercentByType(ShelterType shelterType)
    {
        switch (shelterType)
        {
            case ShelterType.None:
                return 0;
            case ShelterType.Semi:
                return SEMIGROUNDPERCENT;
            case ShelterType.Full:
                return FULLGROUNDPERCENT;
        }
        return 0;
    }

    private static int GetPercentFromGunType(GunType gunType, int distance)
    {
        return ConfigurationManager.GlobalDataJson.typeGun[(int)gunType].baseValue +
            ConfigurationManager.GlobalDataJson.typeGun[(int)gunType].distanceValue * distance;
    }
}

public enum CordinatesSide
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

