using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UnitInfoDialog : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _unitName, _unitDescription;

    [SerializeField]
    private GameObject _modifiersUI;

    [SerializeField]
    private GameObject _unitInfoModifierPrefab;

    public void Open(Unit unit)
    {
        _unitName.text = unit.Stats.Name();
        _unitDescription.text = unit.Stats.Description();
        ObjectUtils.DestroyAllChildren(_modifiersUI);

        foreach (Modifier modifier in unit.Modifiers.Modifiers)
        {
            GameObject modifierItem = Instantiate(_unitInfoModifierPrefab, _modifiersUI.transform);
            modifierItem.GetComponent<UnitInfoDialogModifier>().Init(modifier);
        }
        gameObject.SetActive(true);
        Manager.CameraManager.FreeCamera.enabled = false;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        Manager.CameraManager.FreeCamera.enabled = true;
    }
}
