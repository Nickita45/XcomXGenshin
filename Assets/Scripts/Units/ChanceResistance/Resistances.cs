using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChanceResistance
{
    public interface IChanceResistance
    {
        void CalculateChance(
            Unit shooter,
            Unit defender,
            GunType actualGun,
            Element element,
            ref int percent,
            ref int hit
            );
    }


    public class HunkerDownChanceResistance : IChanceResistance
    {
        private int _semiShelterResistance = 10;
        private int _fullShelterResistance = 20;

        public void CalculateChance(Unit shooter, Unit defender, GunType actualGun, Element element, ref int percent, ref int hit)
        { 
            ShelterType maxShelter = AimUtils.GetMaxShelterType(shooter.ActualTerritory, defender.ActualTerritory);

            if (maxShelter == ShelterType.Semi)
                percent -= _semiShelterResistance;
            else if (maxShelter == ShelterType.Full)
                percent -= _fullShelterResistance;

            if (hit == (shooter.Stats as IShooter).MaxDmg()) hit--;

        }
    }
}
