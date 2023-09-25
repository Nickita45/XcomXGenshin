using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyController
{
    public void OnTriggerCharacter();
    public void OnTriggerMakeAction();
    public void MakeAction(Action finalAction);
    public IEnumerator MoveEnemy(Action finalAction, Func<Dictionary<TerritroyReaded, TerritroyReaded>, TerritroyReaded> findTerritoryMoveTo);

    public IEnumerator MakeAttack(Action finalAction, CharacterInfo character);
}
