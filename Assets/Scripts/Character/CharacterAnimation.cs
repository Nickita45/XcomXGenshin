using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    [SerializeField]
    private RuntimeAnimatorController _animatorController;

    private GameObject _avatar;
    public GameObject Avatar => _avatar;

    private Animator _animator;

    public void Init(string avatarPath)
    {
        Object prefab = Resources.Load("Models/" + avatarPath);
        _avatar = (GameObject)Instantiate(prefab, transform);

        _avatar.transform.localPosition = new(0, -0.88f, 0);
        _avatar.transform.localScale = new(2f, 2f, 2f);

        _animator = _avatar.GetComponent<Animator>();
        _animator.runtimeAnimatorController = _animatorController;
        _animator.applyRootMotion = false;
    }

    public IEnumerator Run()
    {
        _animator.SetBool("isRunning", true);
        yield return null;
    }

    public IEnumerator StopRunning()
    {
        _animator.SetBool("isRunning", false);
        yield return null;
    }

    public IEnumerator CrouchRotate(Quaternion target)
    {
        if (Quaternion.Angle(_avatar.transform.rotation, target) >= 15)
        {
            _animator.SetBool("isRotating", true);
            yield return new WaitForEndOfFrame();

            yield return StartCoroutine(ObjectHelpers.RotateTransform(_avatar.transform, target, 0.5f));

            _animator.SetBool("isRotating", false);
            yield return new WaitForEndOfFrame();
            while (_animator.IsInTransition(0)) yield return null;
        }
    }

    public IEnumerator Shoot()
    {
        _animator.SetBool("isShooting", true);
        yield return new WaitForEndOfFrame();
        while (_animator.IsInTransition(0)) yield return null;
    }

    public IEnumerator StopShooting()
    {
        _animator.SetBool("isShooting", false);
        yield return new WaitForEndOfFrame();
        while (_animator.IsInTransition(0)) yield return null;
    }

    public IEnumerator Crouch()
    {
        _animator.SetBool("isCrouched", true);
        yield return new WaitForEndOfFrame();
        while (_animator.IsInTransition(0)) yield return null;
    }

    public IEnumerator StopCrouching()
    {
        _animator.SetBool("isCrouched", false);
        yield return new WaitForEndOfFrame();
        while (_animator.IsInTransition(0)) yield return null;
    }
}
