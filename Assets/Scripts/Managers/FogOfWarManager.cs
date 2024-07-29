using FischlWorks_FogWar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWarManager 
{
    private csFogWar _csFogWarObj; 

    public FogOfWarManager(csFogWar csFogWar)
    {
        foreach(var character in Manager.Map.Characters.GetList)
        {
            csFogWar.AddFogRevealer(new csFogWar.FogRevealer(character.gameObject.transform, (int)character.Stats.VisionDistance(), true));
        }

        foreach (var enemy in Manager.Map.Enemies.GetList)
        {
            enemy.csFogVisibilityAgent.SetcsFogWar(csFogWar);        
        }

        _csFogWarObj = csFogWar;

        Manager.StatusMain.OnStatusChange += OnStatusChange;
    }

    ~FogOfWarManager()
    {
        Manager.StatusMain.OnStatusChange -= OnStatusChange;
    }

    public void OnStatusChange(HashSet<Permissions> permissions)
    {
        bool visibility = !permissions.Contains(Permissions.NonFog);
        _csFogWarObj.FogPlane.SetActive(visibility);
        
    }



}
