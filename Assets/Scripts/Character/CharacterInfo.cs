using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterInfo : MonoBehaviour
{
    [Header("Selector")]
    [SerializeField]
    private GameObject _selectItem;

    [SerializeField]
    private Material _materialSelect;
    private Material _basicMaterial;

    [Header("Mover")]
    [SerializeField]
    private GameObject _mover;

    [Header("Shelters")]
    [SerializeField]
    private GameObject[] _shelterSize;//on indexes SidesShelter in ShelterDetecter
    public GameObject this[int index] => _shelterSize[index];


    private bool _selected;

    public Action<CharacterInfo> OnSelected;
    public Action OnDeselected;

    public TerritroyReaded ActualTerritory { get; set; }



    private void Start()
    {
        _selectItem.SetActive(false);
        _mover.SetActive(false);
        _basicMaterial = _selectItem.GetComponent<MeshRenderer>().material;

        OnSelected += Select;
        OnSelected += GameManagerMap.Instance.CharacterMovemovent.CharacterSelect;


        OnDeselected += Disable;
        OnDeselected += GameManagerMap.Instance.CharacterMovemovent.CharacterDeselect;

        ActualTerritory = GameManagerMap.Instance.Map[transform.localPosition];
    }


    private void OnMouseEnter()
    {
        if(_selected == false)
        _selectItem.SetActive(true);
    }

    private void OnMouseExit()
    {
        if (_selected == false)
            _selectItem.SetActive(false);
    }

    private void OnMouseDown()
    {
        if (ActualTerritory != null)
        {
            if (_selected)
                OnDeselected();
            else
                OnSelected(this);
        }
    }

    private void Select(CharacterInfo character)
    {

        _selected = true;
        _selectItem.GetComponent<MeshRenderer>().material = _materialSelect;
        MoverActive(true);
    }

    private void Disable()
    {
        _selected = false;
        _selectItem.GetComponent<MeshRenderer>().material = _basicMaterial;
        _mover.transform.localPosition = new Vector3(0, _mover.transform.localPosition.y, 0);
        MoverActive(false);
    }

    public void MoverActive(bool result) => _mover.SetActive(result);
    public bool MoverActive() => _mover.activeSelf;

    public void SetCordintasToMover(Vector3 vector) => _mover.transform.position = vector;

    public bool isAktualTerritory(TerritroyReaded territory) => territory == ActualTerritory;
}
