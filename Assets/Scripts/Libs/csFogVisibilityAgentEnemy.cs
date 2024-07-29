using FischlWorks_FogWar;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class csFogVisibilityAgentEnemy : csFogVisibilityAgent
{
    [SerializeField]
    private Enemy _enemy;

    public bool IsVisible => visibility;

    protected override void Update()
    {
        base.Update();

        if(!Manager.HasPermission(Permissions.NonFog))
            _enemy.Canvas.CanvasGameObject.SetActive(visibility);
    }
}
