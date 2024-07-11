using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace ChanceResistance
{
    public class UnitChanceResistance
    {
        Dictionary<int, IChanceResistance> _percentsResistance = new();

        public void GetResistance(
            Unit shooter,
            Unit defender,
            GunType actualGun,
            Element element,
            ref int percent,
            ref int hit)
        {
            if (_percentsResistance.Count == 0) return;

            foreach (var resistance in _percentsResistance)
                resistance.Value.CalculateChance(shooter, defender, actualGun, element, ref percent, ref hit);
        }

        public void ClearResistance() => _percentsResistance.Clear();
        public void AddResistance(object sender, IChanceResistance resistance) => _percentsResistance.Add(sender.GetHashCode(), resistance);
        public void RemoveResistance(object sender) => _percentsResistance.Remove(sender.GetHashCode());

    }
}
