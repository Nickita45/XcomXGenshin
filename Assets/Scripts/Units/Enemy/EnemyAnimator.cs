using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimator : UnitAnimator
{
    [SerializeField]
    private Animator _enemyAnimator;

    private void Start()
    {
        Init(_enemyAnimator);
    }

    // To make it more visually apparent,
    // enemies that do not try to hide behind shelters will
    // always face the closest character instead.
    public override IEnumerator CrouchRotateHideBehindShelter(Vector3 position)
    {
        base.CrouchRotateHideBehindShelter(position);

        if (!_canCrouch)
        {
            Enemy enemy = transform.parent.GetComponent<Enemy>();
            Character closestCharacter = enemy.GetClosestVisibleCharacter();

            if (closestCharacter)
            {
                yield return StartCoroutine(RotateLookAt(closestCharacter.transform.localPosition));
                TerritroyReaded territoryReaded = Manager.Map[position];
                Dictionary<ShelterSide, ShelterType> shelters = ShelterDetectUtils.DetectShelters(territoryReaded);
            }
        }
    }
}
