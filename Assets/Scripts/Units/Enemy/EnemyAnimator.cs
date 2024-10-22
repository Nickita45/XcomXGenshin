using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimator : UnitAnimator
{
    // To make it more visually apparent,
    // enemies that do not try to hide behind shelters will
    // always face the closest character instead.
    public override IEnumerator CrouchRotateHideBehindShelter(Vector3 position)
    {
        base.CrouchRotateHideBehindShelter(position);

        if (!_canCrouch)
        {
            Enemy enemy = GetComponent<Enemy>(); //maybe better save reference?
            Unit closestCharacter = enemy.GetClosestVisibleCharacter();

            if (closestCharacter)
            {
                yield return StartCoroutine(RotateLookAt(closestCharacter.transform.localPosition));

                if (!enemy.IsKilled)
                {
                    TerritroyReaded territoryReaded = Manager.Map[position];
                    Dictionary<ShelterSide, ShelterType> shelters = ShelterDetectUtils.DetectShelters(territoryReaded);
                }
            }
        }
    }
}
