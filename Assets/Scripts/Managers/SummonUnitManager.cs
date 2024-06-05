using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEditor.Progress;

public class SummonUnitManager : MonoBehaviour //mb make to non monobehaviour
{
    private void Start()
    {
        Manager.StatusMain.OnStatusChange += OnStatusChange;
        Manager.MovementManager.OnSelectNewTerritory += SummonAreaAbility;
    }

    private void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0) 
            && Manager.HasPermission(Permissions.SummonObjectOnMap) && Manager.MovementManager.GetSelectedTerritory != null)
        {
            Manager.AbilityPanel.ActivateAbility();
        }
    }

    public void SummonAreaAbility((TerritroyReaded aktualTerritoryReaded, List<Vector3> path) aktual, Character character)
    {
        if (Manager.HasPermission(Permissions.SummonObjectOnMap) && Manager.AbilityPanel.Selected?.Ability is IAbilityArea area)
        {
            area.SummonArea();
        }

    }

    public (GameObject, TerritroyReaded) SummonEntity(string path)
    {
        var selectedTerritory = Manager.MovementManager.GetSelectedTerritory;
        selectedTerritory.TerritoryInfo = TerritoryType.ShelterGround;
        GameObject basePrefab = Resources.Load<GameObject>(path);
        GameObject obj = Instantiate(basePrefab, Manager.MainParent.transform);
        obj.transform.localPosition = new Vector3(selectedTerritory.XPosition, selectedTerritory.YPosition, selectedTerritory.ZPosition);
        return (obj, selectedTerritory);
    }

    private void OnStatusChange(HashSet<Permissions> permissions)
    {
        if(permissions.Count > 0 && permissions.Contains(Permissions.SummonObjectOnMap))
        {

        }
    }
}
