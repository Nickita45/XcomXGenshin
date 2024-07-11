using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public abstract class Entity : Unit
{
    protected Unit _creator;
    protected int _lifeTime { get; set; }
    public override TerritroyReaded ActualTerritory { get; set; }
    public override int ActionsLeft { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public abstract void Activate();
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
        Manager.TurnManager.RoundBeginEvent += OnRoundBegin;
        Manager.TurnManager.EntityTurnBeginEvenet += Activate; //mb change in future
    }

    public virtual void OnCreate(Unit creater, TerritroyReaded newPosition)
    {
        Manager.Map.Entities.Add(this);
        _creator = creater;
    }
    private void OnRoundBegin()
    {
        LifeTimerDecrease();
    }
    public override void Kill()
    {
        Manager.Map.Entities.Remove(this); //remove from map entity list
        _canvas.DisableAll(); //disable canvas elements
        if (_modifiers != null)
            foreach (Modifier m in _modifiers.Modifiers) m.DestroyModel(this);

        ActualTerritory.TerritoryInfo = TerritoryType.Air; //set character's block to air
        foreach (var col in gameObject.GetComponentsInChildren<CapsuleCollider>()) col.enabled = false;
        foreach (Transform obj in gameObject.transform) obj.gameObject.SetActive(false);

        Manager.TurnManager.RoundBeginEvent -= OnRoundBegin;
        Manager.TurnManager.EntityTurnBeginEvenet -= Activate; //mb change in future

    }

    private void OnDisable()
    {
        Manager.TurnManager.RoundBeginEvent -= OnRoundBegin;
        Manager.TurnManager.EntityTurnBeginEvenet -= Activate; //mb change in future
    }

}
