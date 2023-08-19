using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusMain : MonoBehaviour
{
    public HashSet<Permissions> ActualPermissions { get; private set; }

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
        ActualPermissions = new HashSet<Permissions> { Permissions.CameraMovements, Permissions.AnimationRunning, Permissions.SelectPlaceToMovement };
        OnStatusChange(ActualPermissions);
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
    AnimationRunning
}

//Status:
//0) For clearing Map - Zero Status
//1) Character status
//2) Action status
//3) Enemy Status
//4) Shooting Status 
//5) Running Status 
