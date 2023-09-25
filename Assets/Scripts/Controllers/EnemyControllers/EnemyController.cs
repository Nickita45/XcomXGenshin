using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class EnemyController : MonoBehaviour
{
    protected EnemyInfo _enemyInfo;


    protected virtual void Start()
    {
        _enemyInfo = GetComponent<EnemyInfo>();
    }

    protected CharacterInfo GetFirstPerson() //useless in animations
    {
        if (_enemyInfo.VisibleCharacters.Count > 0)
            return _enemyInfo.VisibleCharacters.OrderBy(n => Vector3.Distance(n.gameObject.transform.localPosition, gameObject.transform.localPosition)).First();
        else
            return null;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
