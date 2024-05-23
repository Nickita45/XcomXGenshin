using System;
using System.Collections.Generic;
using UnityEngine;

public class StatusMain : MonoBehaviour
{
    public HashSet<Permissions> ActualPermissions { get; private set; } //TODO: encapsulation problem

    public bool HasPermisson(Permissions permission) => ActualPermissions.Contains(permission);

    public Action<HashSet<Permissions>> OnStatusChange;

    private void Start()
    {
        ActualPermissions = new HashSet<Permissions>();
    }

    public void SetStatusZero()
    {
        ActualPermissions = new HashSet<Permissions> { };
        OnStatusChange(ActualPermissions);
    }

    public void SetStatusSelectCharacter()
    {
        ActualPermissions = new HashSet<Permissions> {
            Permissions.SelectCharacter,
            Permissions.CameraMovements
        };
        OnStatusChange(ActualPermissions);
    }

    public void SetStatusSelectAction()
    {
        ActualPermissions = new HashSet<Permissions> {
            Permissions.SelectCharacter,
            Permissions.CameraMovements,
            Permissions.ActionSelect,
            Permissions.SelectPlaceToMovement
        };
        OnStatusChange(ActualPermissions);
    }
    public void SetStatusSelectEnemy()
    {
        ActualPermissions = new HashSet<Permissions> {
            Permissions.ActionSelect,
            Permissions.SelectEnemy
        };
        OnStatusChange(ActualPermissions);
    }

    public void SetStatusShooting()
    {
        ActualPermissions = new HashSet<Permissions> { Permissions.AnimationShooting };
        OnStatusChange(ActualPermissions);
    }

    public void SetStatusRunning()
    {
        ActualPermissions = new HashSet<Permissions>
        {
            Permissions.CameraMovements,
            Permissions.AnimationRunning,
            Permissions.SelectPlaceToMovement
        };
        OnStatusChange(ActualPermissions);
    }

    public void SetStatusWaiting()
    {
        ActualPermissions = new HashSet<Permissions>
        {
            Permissions.CameraMovements,
            Permissions.Waiting
        };
        OnStatusChange(ActualPermissions);
    }

    public void SetStatusSummon()
    {
        ActualPermissions = new HashSet<Permissions> {
            Permissions.CameraMovements, 
            Permissions.SummonObjectOnMap, 
            Permissions.ActionSelect,
            Permissions.SelectCharacter
        };
        OnStatusChange(ActualPermissions);
    }


    private void Update() //for debug only
    {
       // Debug.Log(string.Join(",", Manager.StatusMain.ActualPermissions));
    }
}

public enum Permissions
{
    CameraMovements,
    SelectCharacter,
    ActionSelect,
    SelectPlaceToMovement,
    SelectEnemy,
    AnimationShooting,
    AnimationRunning,
    Waiting,
    SummonObjectOnMap
}
