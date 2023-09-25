using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PersonInfo : MonoBehaviour 
{
    [SerializeField]
    protected int _countHp = 5;
    public abstract TerritroyReaded ActualTerritory { get; set; }
    public abstract int CountActions { get; set; }
    public abstract float SpeedCharacter();
    public abstract int MoveDistanceCharacter();
    public abstract float VisibilityCharacter();
    protected abstract void KillPerson();
    public abstract EnemyCanvasController CanvasController();

    public abstract Transform GetBulletSpawner(string name);
    public void MakeHit(int hit)
    {
        _countHp -= hit;
        if (_countHp <= 0)
            KillPerson();
        else
            CanvasController().SetStartHealth(_countHp);
    }
}
