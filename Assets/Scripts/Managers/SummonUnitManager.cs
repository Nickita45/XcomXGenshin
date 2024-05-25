using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class SummonUnitManager : MonoBehaviour //mb make to non monobehaviour
{
    private void Start()
    {
        Manager.StatusMain.OnStatusChange += OnStatusChange;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && Manager.HasPermission(Permissions.SummonObjectOnMap))
        {
            Manager.AbilityPanel.ActivateAbility();
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
