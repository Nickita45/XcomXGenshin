using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : Unit
{
    protected int _lifeTime { get; set; }
    public abstract void Activate();
    public override TerritroyReaded ActualTerritory { get; set; }
    public override int ActionsLeft { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public void LifeTimerDecrease()
    {
        _lifeTime--;
        if(_lifeTime == 0 )
            Kill();
    }
    public override Transform GetBulletSpawner(string name)
    {
        throw new NotImplementedException();
    }

    public override void Start()
    {
        Resurrect();
    }

    public virtual void OnCreate(Unit creater, TerritroyReaded newPosition)
    {
        Manager.Map.Entity.Add(this);
    }

    public override void Kill()
    {
        Manager.Map.Entity.Remove(this); //remove from map character list
        if(_modifiers != null)
            foreach (Modifier m in _modifiers.Modifiers) m.DestroyModel(this);

        ActualTerritory.TerritoryInfo = TerritoryType.Air; //set character's block to air
        foreach (var col in gameObject.GetComponentsInChildren<CapsuleCollider>()) col.enabled = false;
        foreach (Transform obj in gameObject.transform) obj.gameObject.SetActive(false);
    }

}
