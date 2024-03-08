using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : UnitAnimator
{
    [SerializeField]
    private RuntimeAnimatorController _animatorController;

    public void InitCharacter(string avatarPath)
    {
        Object prefab = Resources.Load("Models/" + avatarPath);
        GameObject avatar = (GameObject)Instantiate(prefab, transform.GetChild(0));

        avatar.transform.localPosition = new(0, -0.88f, 0);
        avatar.transform.localScale = new(2f, 2f, 2f);

        Animator animator = avatar.GetComponent<Animator>();
        animator.runtimeAnimatorController = _animatorController;
        animator.applyRootMotion = false;

        Init(animator);
    }
}
