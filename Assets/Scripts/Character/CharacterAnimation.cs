using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{

    [SerializeField]
    private Animator _animator;

    void Start()
    {
        _animator.SetFloat("Blend", 1f);
    }

    public void RunForward()
    {
        _animator.Play("run_forward");
    }

    public IEnumerator CrouchedToStanding()
    {
        _animator.Play("idle_to_crouched_idle");
        yield return StartCoroutine(BlendValueChange(0.15f, 1f, 0f));
    }

    public IEnumerator StandingToCrouched()
    {
        _animator.Play("idle_to_crouched_idle");
        yield return StartCoroutine(BlendValueChange(0.15f, 0f, 1f));
    }

    private IEnumerator BlendValueChange(float duration, float startValue, float endValue)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float blendValue = Mathf.Lerp(startValue, endValue, elapsedTime / duration);
            _animator.SetFloat("Blend", blendValue);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _animator.SetFloat("Blend", endValue);
    }
}
