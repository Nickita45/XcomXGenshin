using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Character : Unit
{
    public new CharacterStats Stats => (CharacterStats)_stats;
    public new CharacterCanvas Canvas => (CharacterCanvas)_canvas;
    public new CharacterAnimator Animator => (CharacterAnimator)_animator;

    private List<Ability> _abilities;
    public List<Ability> Abilities => _abilities;

    [Header("Selector")]
    [SerializeField]
    private GameObject _selectItem;
    public GameObject SelectItem => _selectItem; //circle under the character

    [SerializeField]
    private Material _materialSelect; //when character is selected
    private Material _basicMaterial; //when character is non selected

    [Header("Mover")]
    [SerializeField]
    private GameObject _mover; //circle on the selected block
    public GameObject Mover => _mover;

    [Header("Shelters")]
    [SerializeField]
    private GameObject[] _shelterSize; // on indexes SidesShelter in ShelterDetecter //???
    public GameObject this[int index] => _shelterSize[index]; //???

    [Header("Other")]
    [SerializeField]
    private GameObject[] _gunGameObjects; //guns on model
    [SerializeField]
    private GameObject _gunPrefab; //the parant of all guns models

    private bool _selected;

    public Action<Character> OnSelected;
    public Action OnDeselected;

    public GameObject GunPrefab => _gunPrefab;

    private int _actionsLeft;
    public override int ActionsLeft
    {
        get => _actionsLeft;
        set
        {
            _actionsLeft = value;
            Canvas.SetCountActions(value);
            Manager.TurnManager.AfterCharacterAction();
        }
    }

    public override void Start()
    {
        base.Start();
        Stats.Index = -1;
        _selectItem.SetActive(false);
        _mover.SetActive(false);
        _basicMaterial = _selectItem.GetComponent<MeshRenderer>().material;

        OnSelected += Select;
        OnSelected += Manager.MovementManager.OnCharacterSelect;
        OnSelected += Manager.EnemyPanel.OnCharacterSelect;
        OnSelected += Manager.AbilityPanel.SetCharacter;

        OnDeselected += Disable;
        OnDeselected += Manager.EnemyPanel.OnCharacterDeselect;
        OnDeselected += Manager.MovementManager.OnCharacterDeselect;

        ActualTerritory = Manager.Map[transform.localPosition];
        ActualTerritory.TerritoryInfo = TerritoryType.Character;
    }

    public void OnIndexSet()
    {
        gameObject.name = Stats.Name(); //set name

        Resurrect(); //set max hp
        Canvas.UpdateHealthUI(Stats.MaxHP());

        SetGunByIndex((int)Stats.Weapon); //set gun

        _abilities = new() {
            new AbilityShoot(),
            new AbilityShoot(Stats.Element),
            new AbilityOverwatch(),
            new AbilityHunkerDown(),
            new AbilityElementalSkill(Stats.Element)
        }; //set abilities

        Animator.InitCharacter(ConfigurationManager.CharactersData[Stats.Index].characterAvatarPath); //
        Animator.GetComponentInChildren<GunModel>().Init(); //
    }

    private void OnMouseEnter()
    {
        if (!_selected && Manager.HasPermission(Permissions.SelectCharacter) && ActionsLeft > 0)
            _selectItem.SetActive(true);
    }

    private void OnMouseExit()
    {
        if (!_selected && Manager.HasPermission(Permissions.SelectCharacter) && ActionsLeft > 0)
            _selectItem.SetActive(false);
    }

    private void OnMouseDown()
    {
        if (ActualTerritory != null &&
            Manager.HasPermission(Permissions.SelectCharacter) && ActionsLeft > 0)
        {
            if (_selected)
                Manager.TurnManager.DeselectCharacter();
            else
                Manager.TurnManager.SelectCharacter(this);
        }
    }

    private void Select(Character character)
    {
        SelectChanges();
        MoverActive(true);
    }

    public void SelectChanges()
    {
        SetGunByIndex((int)Stats.Weapon);
        _selectItem.SetActive(true);

        _selected = true;
        _selectItem.GetComponent<MeshRenderer>().material = _materialSelect; //set material to matrial for selecting
    }

    private void DisableChanges()
    {
        _selectItem.SetActive(false);
        _selected = false;
        _selectItem.GetComponent<MeshRenderer>().material = _basicMaterial; //set matetial to basic material
        _mover.transform.localPosition = new Vector3(0, _mover.transform.localPosition.y, 0); //change position for mover to under the character

        foreach (GameObject item in _shelterSize) //??? ebu
        {
            foreach (Transform item2 in item.transform)
            {
                item2.gameObject.SetActive(false);
            }
        }
    }

    private void Disable()
    {
        DisableChanges();
        MoverActive(false);
    }

    public void SetGunByIndex(int index)
    {
        foreach (var item in _gunGameObjects)
        {
            item.SetActive(false);
        }
        _gunGameObjects[index].SetActive(true);
    }

    public override Transform GetBulletSpawner(string name) => GunPrefab.transform.GetChild((int)Stats.Weapon).Find(name);

    public void MoverActive(bool result) => _mover.SetActive(result);
    public bool MoverActive() => _mover.activeSelf;

    public void SetCoordinatsToMover(Vector3 vector) => _mover.transform.position = vector;

    public bool IsActualTerritory(TerritroyReaded territory) => territory == ActualTerritory;

    public override TerritroyReaded ActualTerritory { get; set; }

    public override void Kill()
    {
        Manager.Map.Characters.Remove(this); //remove from map character list
        foreach (Modifier m in _modifiers.Modifiers) m.DestroyModel(this);
        Manager.TurnManager.OutOfActions(this);
        _canvas.DisableAll(); //disable canvas elements

        Animator.Model.SetActive(false); //disable animator
        GunPrefab.SetActive(false); //disable gun

        GetComponent<CapsuleCollider>().enabled = false; //disable collider

        ActualTerritory.TerritoryInfo = TerritoryType.Air; //set character's block to air

        Manager.StatisticsUtil.SoldierDeathCount++;
        
    }
}
