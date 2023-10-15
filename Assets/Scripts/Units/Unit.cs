using System.Collections;
using System.Collections.Generic;
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

    // The amount of action points, which can be used for moving (dashing) and abilities.
    public abstract int ActionsLeft { get; set; }

    public abstract TerritroyReaded ActualTerritory { get; set; }

    public abstract Transform GetBulletSpawner(string name);
    public abstract void Kill();

    public virtual void Start()
    {
        _countHp = Stats.MaxHP();
        Canvas.SetStartHealth(Stats.MaxHP());
    }

    public void MakeHit(int hit)
    {
        _countHp -= hit;
        if (_countHp <= 0)
            Kill();
        else
            _canvas.SetStartHealth(_countHp);
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
                yield return null;
            }
            transform.localPosition = target;

            if (target == targets[^1]) //last
            {
                break;
            }
            target = targets[++index];
            yield return null;
        }

        // Setup idle crouching animation
        yield return StartCoroutine(Animator.StopRunning());
        yield return StartCoroutine(Animator.StartCrouching());
        yield return StartCoroutine(Animator.CrouchRotateHideBehindShelter(transform.localPosition));
    }
}
