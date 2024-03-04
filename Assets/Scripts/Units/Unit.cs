using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

// The base class that manages the unit.
public abstract class Unit : MonoBehaviour
{
    [SerializeField]
    protected UnitStats _stats;
    public virtual UnitStats Stats => _stats;

    [SerializeField]
    protected UnitCanvas _canvas;
    public virtual UnitCanvas Canvas => _canvas;

    [SerializeField]
    protected UnitAnimator _animator;
    public virtual UnitAnimator Animator => _animator;

    protected int _countHp;
    protected ModifierSet _modifiers = new();
    public virtual ModifierSet Modifiers => _modifiers;

    // The amount of action points, which can be used for moving (dashing) and abilities.
    public abstract int ActionsLeft { get; set; }

    public abstract TerritroyReaded ActualTerritory { get; set; }

    public abstract Transform GetBulletSpawner(string name);
    public abstract void Kill();

    public virtual void Start()
    {
        _countHp = Stats.MaxHP();
        Canvas.UpdateHealthUI(Stats.MaxHP()); //update visual hp of unit
    }

    // Deal a set amount of elemental damage to the unit.
    // 
    // damageSource is an object that caused the damage.
    // Right now it can either be a Unit, a Modifier, or a null (other).
    public void MakeHit(int hit, Element element, object damageSource)
    {
        if (hit <= 0) return; // Do not do anything if the attack deals no damage

        List<ElementalReaction> reactions = _modifiers.AddElement(this, element);

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
                    _modifiers.AddModifier(this, new ElectroCharged());
                    break;

                case ElementalReaction.Overloaded:
                    foreach (Unit ally in GetAdjancentAllies(1))
                    {
                        if (ally == this) ally.MakeHit((int)(hit * 1.5), Element.Physical, this);
                        else { ally.MakeHit((int)(hit * 0.5), Element.Physical, this); }
                    }
                    break;

                case ElementalReaction.Superconduct:
                    _modifiers.AddModifier(this, new Superconduct());
                    break;
                case ElementalReaction.SuperconductActivate:
                    hit = (int)(hit * 1.5);
                    break;

                case ElementalReaction.Freeze:
                    ActionsLeft = 0;
                    _modifiers.AddModifier(this, new Freeze());
                    break;
                case ElementalReaction.Shatter:
                    hit += UnityEngine.Random.Range(1, 2);
                    break;

                case ElementalReaction.SwirlPyro:
                    foreach (Unit ally in GetAdjancentAllies(1))
                    {
                        if (ally == this) hit += 1;
                        else
                        {
                            ally.MakeHit(1, Element.Pyro, this);
                            ally.Canvas.UpdateModifiersUI(ally.Modifiers);
                        }
                    }
                    break;
                case ElementalReaction.SwirlCryo:
                    foreach (Unit ally in GetAdjancentAllies(1))
                    {
                        if (ally == this) hit += 1;
                        else
                        {
                            ally.MakeHit(1, Element.Cryo, this);
                            ally.Canvas.UpdateModifiersUI(ally.Modifiers);
                        }
                    }
                    break;
                case ElementalReaction.SwirlHydro:
                    foreach (Unit ally in GetAdjancentAllies(1))
                    {
                        if (ally == this) hit += 1;
                        else
                        {
                            ally.MakeHit(1, Element.Hydro, this);
                            ally.Canvas.UpdateModifiersUI(ally.Modifiers);
                        }
                    }
                    break;
                case ElementalReaction.SwirlElectro:
                    foreach (Unit ally in GetAdjancentAllies(1))
                    {
                        if (ally == this) hit += 1;
                        else
                        {
                            ally.MakeHit(1, Element.Electro, this);
                            ally.Canvas.UpdateModifiersUI(ally.Modifiers);
                        }
                    }
                    break;

                case ElementalReaction.CrystallizePyro:
                    if (damageSource is Unit attacker)
                    {
                        attacker.Modifiers.AddModifier(this, new Crystallize());
                        attacker.Canvas.UpdateModifiersUI(attacker.Modifiers);
                    }
                    break;
                case ElementalReaction.CrystallizeCryo:
                    if (damageSource is Unit attacker1)
                    {
                        attacker1.Modifiers.AddModifier(this, new Crystallize());
                        attacker1.Canvas.UpdateModifiersUI(attacker1.Modifiers);
                    }
                    break;
                case ElementalReaction.CrystallizeHydro:
                    if (damageSource is Unit attacker2)
                    {
                        attacker2.Modifiers.AddModifier(this, new Crystallize());
                        attacker2.Canvas.UpdateModifiersUI(attacker2.Modifiers);
                    }
                    break;
                case ElementalReaction.CrystallizeElectro:
                    if (damageSource is Unit attacker3)
                    {
                        attacker3.Modifiers.AddModifier(this, new Crystallize());
                        attacker3.Canvas.UpdateModifiersUI(attacker3.Modifiers);
                    }
                    break;
            }
        }

        hit = _modifiers.OnHit(this, hit, element);
        StartCoroutine(Canvas.PanelShow(Canvas.PanelHit(hit), 4));

        _countHp -= hit;
        if (_countHp <= 0)
            Kill();
        else
            Canvas.UpdateHealthUI(_countHp);  //update visual hp of unit

        Canvas.UpdateModifiersUI(_modifiers);
        Canvas.ShowReactions(reactions);
    }

    public bool IsKilled => _countHp <= 0;

    // Moves the unit through the list of points on the map. 
    // To find the list of points, use the approppriate functions
    // in the MovementManager (CalculateAllPossible / CalculateAllPath).
    //
    // This assumes the current StatusMain is a waiting status. Make sure to setup the status
    // before calling the function. Otherwise, the player might still be able to act
    // while the unit is moving.
    public IEnumerator Move(List<Vector3> targets)
    {
        int index = 0;
        Vector3 target = targets[++index]; // ignore first, because its for line

        // Setup run animation
        yield return StartCoroutine(Animator.StopCrouching());
        yield return StartCoroutine(Animator.StartRunning());

        while (true)
        {
            Animator.RotateLookAtImmediate(target);

            while (Vector3.Distance(transform.localPosition, target) > 0.1f)
            {
                float elapsedTime = Time.deltaTime * Stats.Speed();
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, elapsedTime);


                foreach (var unitOverwatched in Manager.TurnManager.CheckOverwatchMake(this))
                {
                    _animator.SetSpeedAnimatorSlow(true);
                    this.ActualTerritory = new TerritroyReaded(transform);

                    Time.timeScale = 0.5f;
                    if (unitOverwatched is Character)
                    {
                        yield return StartCoroutine(((Character)unitOverwatched).Abilities[0].Activate(unitOverwatched, this));
                    }
                    else if (unitOverwatched is Enemy)
                    {
                        yield return StartCoroutine(((Enemy)unitOverwatched).AI.Attack((Character)this));
                    }
                    Time.timeScale = 1f;
                    _animator.SetSpeedAnimatorSlow(false);
                }

                if (this.IsKilled)
                    break;

                yield return null;
            }

            if (this.IsKilled)
                break;

            transform.localPosition = target;

            if (target == targets[^1]) //last
            {
                break;
            }
            target = targets[++index];
            yield return null;
        }

        if (!this.IsKilled)
        {
            // Setup idle crouching animation
            yield return StartCoroutine(Animator.StopRunning());
            yield return StartCoroutine(Animator.StartCrouching());
            yield return StartCoroutine(Animator.CrouchRotateHideBehindShelter(transform.localPosition));
        }
    }

    // Get a list of all allies of the unit (including the unit themselves)
    public List<Unit> GetAllies()
    {
        if (this is Character)
        {
            return Manager.Map.Characters.Select(c => (Unit)c).ToList();
        }
        else if (this is Enemy)
        {
            return Manager.Map.Enemies.Select(e => (Unit)e).ToList();
        }
        else
        {
            return new();
        }
    }

    // Get a list of allies of the unit within n squares from them (including the unit themselves)
    public List<Unit> GetAdjancentAllies(int n)
    {
        Vector3 coordinats = ActualTerritory.GetCordinats();

        return GetAllies().Where(ally =>
        {
            Vector3 otherCoordinats = ally.ActualTerritory.GetCordinats();
            // Find if any are within 1 square from the unit
            return
                Mathf.Abs(coordinats.x - otherCoordinats.x) <= n &&
                Mathf.Abs(coordinats.y - otherCoordinats.y) <= n &&
                Mathf.Abs(coordinats.z - otherCoordinats.z) <= n;
        }).ToList();
    }
}
