using FischlWorks_FogWar;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class csFogVisibilityAgentEnemy : csFogVisibilityAgent
{
    [SerializeField]
    private Enemy _enemy;

    protected override void Update()
    {
        base.Update();

        _enemy.Canvas.CanvasGameObject.SetActive(visibility);
    }
}
