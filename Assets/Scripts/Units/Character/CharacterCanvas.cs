using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCanvas : UnitCanvas
{
    [Header("Character")]
    [SerializeField]
    private GameObject[] _actions;

    public override void Start()
    {
        base.Start();
    }

    public void SetCountActions(int count) //make children
    {
        for (int i = 0; i < _actions.Length; i++)
        {
            _actions[i].SetActive(i < count);
        }
    }

    public override void OnStatusChange(HashSet<Permissions> permissions)
    {
        if (permissions.Count == 0)
        {
            Manager.StatusMain.OnStatusChange -= OnStatusChange;
            return;
        }

        if (permissions.Contains(Permissions.SelectEnemy) || permissions.Contains(Permissions.AnimationShooting) || permissions.Contains(Permissions.NonFog))
            _canvas.gameObject.SetActive(false);
        else
            _canvas.gameObject.SetActive(true);
    }
}
