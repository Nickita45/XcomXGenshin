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
    public void SetStatusSelectCharacter()
    {
        ActualPermissions = new HashSet<Permissions> { Permissions.SelectCharacter, Permissions.CameraMovements };
        OnStatusChange(ActualPermissions);
    }

    public void SetStatusSelectAction()
    {
        ActualPermissions = new HashSet<Permissions> { Permissions.SelectCharacter, Permissions.CameraMovements, Permissions.ActionSelect, Permissions.SelectPlaceToMovement};
        OnStatusChange(ActualPermissions);
    }
    public void SetStatusSelectEnemy()
    {
        ActualPermissions = new HashSet<Permissions> { Permissions.ActionSelect, Permissions.SelectEnemy };
        OnStatusChange(ActualPermissions);
    }
}

public enum Permissions
{
    CameraMovements,
    SelectCharacter,
    ActionSelect,
    SelectPlaceToMovement,
    SelectEnemy
}

//Status:
//1) Select Character status
//2) Select Action status
//3) Select Enemy Status
