using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitHealth
{
    private ModifierList _modifiers;
    private UnitCanvas _canvas;
    private Unit _mainUnit;
    private int _countHp;

    public Func<int, Element, int> OnResistance;

    public UnitHealth(Unit mainUnit, int maxHealth, ModifierList modifierList, UnitCanvas canvas)
    {
        _mainUnit = mainUnit;
        _countHp = maxHealth;
        _modifiers = modifierList;
        _canvas = canvas;
    }

    public int CountHp => _countHp;


    // Deal a set amount of elemental damage to the unit.
    // 
    // damageSource is an object that caused the damage.
    // Right now it can either be a Unit, a Modifier, or a null (other).
    public void MakeHit(int hit, Element element, object damageSource)
    {

        if ((Unit)damageSource != _mainUnit && hit <= 0) return; // Unless is the source is the unit themselves, do not do anything if the attack deals no damage

        List<ElementalReaction> reactions = _modifiers.AddElement(element);

        foreach (ElementalReaction reaction in reactions)
        {
            switch (reaction)
            {
                case ElementalReaction.MeltStrong:
                    hit *= 2;
                    break;
                case ElementalReaction.MeltWeak:
                    hit = (int)(hit * 1.5);
                    break;

                case ElementalReaction.VaporizeStrong:
                    hit *= 2;
                    break;
                case ElementalReaction.VaporizeWeak:
                    hit = (int)(hit * 1.5);
                    break;

                case ElementalReaction.ElectroCharged:
                    _modifiers.AddModifier(new ElectroCharged());
                    break;

                case ElementalReaction.Overloaded:
                    foreach (Unit ally in Manager.Map.GetAdjancentAllies(1, _mainUnit))
                    {
                        if (ally == _mainUnit) ally.Health.MakeHit((int)(hit * 1.5), Element.Physical, _mainUnit);
                        else { ally.Health.MakeHit((int)(hit * 0.5), Element.Physical, _mainUnit); }
                    }
                    break;

                case ElementalReaction.Superconduct:
                    _modifiers.AddModifier(new Superconduct());
                    break;

                case ElementalReaction.Freeze:
                    _mainUnit.ActionsLeft = 0;
                    _modifiers.AddModifier(new Freeze());
                    break;
                case ElementalReaction.Shatter:
                    hit += UnityEngine.Random.Range(1, 2);
                    break;

                case ElementalReaction.SwirlPyro:
                    foreach (Unit ally in Manager.Map.GetAdjancentAllies(1, _mainUnit))
                    {
                        if (ally == _mainUnit) hit += 1;
                        else
                        {
                            ally.Health.MakeHit(1, Element.Pyro, _mainUnit);
                        }
                    }
                    break;
                case ElementalReaction.SwirlCryo:
                    foreach (Unit ally in Manager.Map.GetAdjancentAllies(1, _mainUnit))
                    {
                        if (ally == _mainUnit) hit += 1;
                        else
                        {
                            ally.Health.MakeHit(1, Element.Cryo, _mainUnit);
                        }
                    }
                    break;
                case ElementalReaction.SwirlHydro:
                    foreach (Unit ally in Manager.Map.GetAdjancentAllies(1, _mainUnit))
                    {
                        if (ally.Health == this) hit += 1;
                        else
                        {
                            ally.Health.MakeHit(1, Element.Hydro, this);
                        }
                    }
                    break;
                case ElementalReaction.SwirlElectro:
                    foreach (Unit ally in Manager.Map.GetAdjancentAllies(1, _mainUnit))
                    {
                        if (ally.Health == this) hit += 1;
                        else
                        {
                            ally.Health.MakeHit(1, Element.Electro, this);
                        }
                    }
                    break;

                case ElementalReaction.CrystallizePyro:
                    if (damageSource is Unit attacker)
                    {
                        attacker.Modifiers?.AddModifier(new Crystallize());
                    }
                    break;
                case ElementalReaction.CrystallizeCryo:
                    if (damageSource is Unit attacker1)
                    {
                        attacker1.Modifiers?.AddModifier(new Crystallize());
                    }
                    break;
                case ElementalReaction.CrystallizeHydro:
                    if (damageSource is Unit attacker2)
                    {
                          attacker2.Modifiers?.AddModifier(new Crystallize());
                    }
                    break;
                case ElementalReaction.CrystallizeElectro:
                    if (damageSource is Unit attacker3)
                    {
                        Debug.Log(attacker3.Modifiers);
                          attacker3.Modifiers?.AddModifier(new Crystallize());
                    }
                    break;
            }
        }

        hit = _modifiers.OnHit(hit, element);
        if (OnResistance != null)
        {
            int saveHit = hit; //helping to see if was any dmg 
            hit = OnResistance(hit, element);

            if(hit <= 0 && saveHit != hit) _canvas?.StartCoroutine(_canvas.PanelShow(_canvas.PanelActionInfo("Immunity"), 4));
            else if(hit > 0) _canvas?.StartCoroutine(_canvas.PanelShow(_canvas.PanelHit(hit, element), 4));
        }
        else
        {
            if (hit > 0) _canvas?.StartCoroutine(_canvas.PanelShow(_canvas.PanelHit(hit, element), 4));
        }

        _countHp -= hit;
        if (_mainUnit is Character character)
            Manager.StatisticsUtil.AddCharactersWonded(character);

        if (_countHp <= 0)
            _mainUnit.Kill();
        else
            _canvas?.UpdateHealthUI(_countHp);  //update visual hp of unit

        _canvas?.UpdateModifiersUI(_modifiers);
        _canvas?.ShowReactions(reactions);
    }

    public bool IsKilled => _countHp <= 0;


}
