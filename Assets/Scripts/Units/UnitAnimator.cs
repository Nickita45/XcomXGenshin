using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// The base class for the unit animations
public abstract class UnitAnimator : MonoBehaviour
{
    private const float KOEFSLOW = 0.25f;

    [SerializeField]
    private Animator _animator;

    [Tooltip("The gun model the unit is wielding. Leave this empty if the unit does not wield a gun.")]
    [SerializeField]
    private GunModel _gun;

    [SerializeField]
    private UnitOutline _outline;
    public UnitOutline Outline => _outline;

    public GameObject Model => _animator.gameObject;
    protected bool _hasAnimation = true;

    // Whether the unit crouches or not (e.g. hilichurl crouches to hide better,
    // while the slime is considered to be always standing)
    // 
    // Set to true if the animator contains "isCrouched" parameter.
    protected bool _canCrouch;

    private float _basicSpeed; //used to save speed. To reset slow effect animation

    public void Init(Animator animator)
    {
        _animator = animator;

        if (_gun) _outline.Init(Model, _gun.gameObject);
        else _outline.Init(Model);

        if (!animator.runtimeAnimatorController)
        {
            _hasAnimation = false;
            return;
        }

        _canCrouch = _animator.parameters.Any(param => param.name == "isCrouched");
        _basicSpeed = _animator.speed;
    }

    public void SetSpeedAnimatorSlow(bool active)
    {
        if (active)
            _animator.speed = _basicSpeed * KOEFSLOW;
        else
            _animator.speed = _basicSpeed;
    }

    public IEnumerator StartRunning()
    {
        if (!_hasAnimation) yield break;
        _animator.SetBool("isRunning", true);
        yield return null;
    }

    public IEnumerator StopRunning()
    {
        if (!_hasAnimation) yield break;
        _animator.SetBool("isRunning", false);
        yield return null;
    }

    public IEnumerator Rotate(Quaternion target)
    {
        if (!_hasAnimation) yield break;
        if (Model.transform.rotation != target)
        {
            IEnumerator rotate = ObjectUtils.RotateTransform(Model.transform, target, 0.5f);
            if (_canCrouch)
            {
                _animator.SetBool("isRotating", true);
                yield return new WaitForEndOfFrame();

                yield return StartCoroutine(rotate);

                _animator.SetBool("isRotating", false);
                yield return new WaitForEndOfFrame();
                while (_animator.IsInTransition(0)) yield return null;
            }
            else
            {
                yield return StartCoroutine(rotate);
            }
        }
    }

    public IEnumerator RotateLookAt(Vector3 target)
    {
        Vector3 rotationDirection = target - transform.localPosition;
        Quaternion? targetRotation = ObjectUtils.LookRotationXZ(rotationDirection);
        if (targetRotation.HasValue) yield return Rotate(targetRotation.Value);
        else yield return null;
    }

    public void RotateLookAtImmediate(Vector3 target)
    {
        Vector3 rotationDirection = target - transform.localPosition;
        Quaternion? targetRotation = ObjectUtils.LookRotationXZ(rotationDirection);
        if (targetRotation.HasValue) Model.transform.rotation = targetRotation.Value;
    }

    public virtual IEnumerator CrouchRotateHideBehindShelter(Vector3 position)
    {
        if (_canCrouch)
        {
            TerritroyReaded territoryReaded = Manager.Map[position];
            Dictionary<ShelterSide, ShelterType> shelters = ShelterDetectUtils.DetectShelters(territoryReaded);
            yield return StartCoroutine(CoroutineRotateToShelter(shelters));
        }
    }

    private IEnumerator CoroutineRotateToShelter(Dictionary<ShelterSide, ShelterType> shelters)
    {
        // Select sides of all non empty shelters
        List<ShelterSide> nonEmptyShelterSides = shelters
            .Where(shelter => shelter.Value != ShelterType.None)
            .Select(shelter => shelter.Key)
            .ToList();

        // Do not rotate if all shelters are empty
        if (nonEmptyShelterSides.Count == 0) yield break;

        // Select a random side
        ShelterSide side = nonEmptyShelterSides[Random.Range(0, nonEmptyShelterSides.Count)];

        // Find direction for the shelter side
        Vector3 direction = ShelterDetectUtils.ShelterSideToDirectionVector(side);

        // Find rotationDirection vector, perpendicular to the direction
        // in which the shelter is from the character.
        Vector3 rotationDirection = Vector3.Cross(Vector3.up, direction).normalized;

        // There are two perpendicular vectors which satisfy the condition.
        // We choose the one that is in the same half-plane as the object rotation.
        //
        // In other words, if the character was facing slightly in one direction,
        // they would fully rotate in that direction.
        float dotProduct = Vector3.Dot(rotationDirection, Model.transform.forward);
        if (dotProduct < 0) rotationDirection = -rotationDirection;

        // Rotate the object to match its rotation to the rotationDirection
        Quaternion target = Quaternion.LookRotation(rotationDirection, Vector3.up);
        yield return StartCoroutine(Rotate(target));
    }

    public IEnumerator AttackMelee()
    {
        if (!_hasAnimation) yield break;
        _animator.SetTrigger("attackMelee");
        yield return new WaitForEndOfFrame();
        while (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
        {
            yield return null;
        }
    }

    public IEnumerator AttackRanged()
    {
        if (!_hasAnimation) yield break;
        _animator.SetTrigger("attackRanged");
        yield return new WaitForEndOfFrame();
        while (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
        {
            yield return null;
        }
    }

    public IEnumerator StartAttackRanged()
    {
        if (!_hasAnimation) yield break;
        _animator.SetBool("isAttackingRanged", true);
        yield return new WaitForEndOfFrame();
        while (_animator.IsInTransition(0)) yield return null;
    }

    public IEnumerator StopAttackRanged()
    {
        if (!_hasAnimation) yield break;
        _animator.SetBool("isAttackingRanged", false);
        yield return new WaitForEndOfFrame();
        while (_animator.IsInTransition(0)) yield return null;
    }

    public IEnumerator StartCrouching()
    {
        if (!_hasAnimation || !_canCrouch) yield break;
        _animator.SetBool("isCrouched", true);
        yield return new WaitForEndOfFrame();
        while (_animator.IsInTransition(0)) yield return null;
    }

    public IEnumerator StopCrouching()
    {
        if (!_hasAnimation || !_canCrouch) yield break;
        _animator.SetBool("isCrouched", false);
        yield return new WaitForEndOfFrame();
        while (_animator.IsInTransition(0)) yield return null;
    }
}
