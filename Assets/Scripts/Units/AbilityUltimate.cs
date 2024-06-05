using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbilityUltimate : Ability
{
    private const int KOEFICIENT_DAMAGE = 3;
    private const int KOEFICIENT_DAMAGE_WITH_ELEMENT = 3;

    private int _actualEnergy = 0;

    public abstract int MaxEnergy();
    public abstract Element ElementAbility();
    public override bool IsAvailable => _actualEnergy >= MaxEnergy();
    public override int ActualCooldown
    {
        get
        {
            var result =(int)Mathf.Round((float)_actualEnergy * 100 / MaxEnergy());
            if(result > 100) result = 100;
            return result;
        }
        set { }
    }

    public void GetEnergy(int makedHit, Element element)
    {
        if (element == global::Element.Physical)
            _actualEnergy += makedHit * KOEFICIENT_DAMAGE;
        else
            _actualEnergy += makedHit * KOEFICIENT_DAMAGE * KOEFICIENT_DAMAGE_WITH_ELEMENT;
    }
}
