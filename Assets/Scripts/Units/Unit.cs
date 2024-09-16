using ChanceResistance;
using System;
using System.Collections;
using System.Collections.Generic;
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

    protected UnitHealth _health;
    public UnitHealth Health => _health;

    public int CountHp => _health.CountHp;

    protected ModifierList _modifiers;
    public virtual ModifierList Modifiers => _modifiers;

    public UnitChanceResistance ChanceResistance { get; private set; }

    // The amount of action points, which can be used for moving (dashing) and abilities.
    public abstract int ActionsLeft { get; set; }

    public abstract TerritroyReaded ActualTerritory { get; set; }

    public abstract Transform GetBulletSpawner(string name);
    public abstract void Kill();
    public bool IsKilled => _health.IsKilled;
    public virtual void Resurrect() => _health = new UnitHealth(this, Stats.MaxHP(), _modifiers, _canvas);


    public virtual void Start()
    {
        _modifiers = new ModifierList(this);
        ChanceResistance = new UnitChanceResistance();
        Resurrect();

        Canvas?.UpdateHealthUI(Stats.MaxHP()); //update visual hp of unit
    }


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
                    ActualTerritory = new TerritroyReaded(transform);

                    Time.timeScale = 0.5f;
                    if (unitOverwatched is Character)
                    {
                        yield return StartCoroutine(((Character)unitOverwatched).Abilities.First(ab => ab is AbilityShoot).Activate(unitOverwatched, this));
                    }
                    else if (unitOverwatched is Enemy)
                    {
                        yield return StartCoroutine(((Enemy)unitOverwatched).AI.Attack((Character)this));
                    }
                    unitOverwatched.Canvas.PanelOverwatch.SetActive(false);
                    Time.timeScale = 1f;
                    _animator.SetSpeedAnimatorSlow(false);
                    Manager.StatusMain.SetStatusWaiting();
                }

                if (IsKilled)
                    break;

                yield return null;
            }

            if (IsKilled)
                break;

            transform.localPosition = target;

            if (target == targets[^1]) //last
            {
                break;
            }
            target = targets[++index];
            yield return null;
        }

        if (!IsKilled)
        {
            // Setup idle crouching animation
            yield return StartCoroutine(Animator.StopRunning());
            yield return StartCoroutine(Animator.StartCrouching());
            yield return StartCoroutine(Animator.CrouchRotateHideBehindShelter(transform.localPosition));
        }
    }

}
