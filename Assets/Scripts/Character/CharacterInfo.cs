using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;

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

    [Header("Other")]
    [SerializeField]
    private GameObject[] _gunGameObjects;
    [SerializeField]
    private GameObject _gunPrefab;

    private bool _selected;

    public Action<CharacterInfo> OnSelected;
    public Action OnDeselected;

    private int _countActions;
    public int CountActions
    {
        get => _countActions;
        set
        {
            {
                _countActions = value;

                CanvasController.SetCountActons(value);
                if (_countActions <= 0)
                    GameManagerMap.Instance.TurnController.CharacterEndHisTurn(this);
            }
        }
    }
    public TerritroyReaded ActualTerritory { get; set; }

    public CharacterCanvasController CanvasController { get; set; }
    public GameObject GunPrefab => _gunPrefab;

    [SerializeField]
    private CharacterAnimation _animation;
    public CharacterAnimation Animation => _animation;


    //Config atributes
    public int Index { get; set; }
    public string NameCharacter => ConfigurationManager.Instance.CharactersData.characters[Index].characterName;
    public float SpeedCharacter => ConfigurationManager.Instance.CharactersData.characters[Index].characterSpeed;
    public int MoveDistanceCharacter => ConfigurationManager.Instance.CharactersData.characters[Index].characterMoveDistance;
    public float VisibilityCharacter => ConfigurationManager.Instance.CharactersData.characters[Index].characterVisionDistance;
    public int BaseAimCharacter => ConfigurationManager.Instance.CharactersData.characters[Index].characterBaseAim;
    public int MaxHealthCharacter => ConfigurationManager.Instance.CharactersData.characters[Index].characterBaseHealth;

    private void Start()
    {
        Index = -1;
        _selectItem.SetActive(false);
        _mover.SetActive(false);
        //_basicMaterial = _selectItem.GetComponent<MeshRenderer>().material;

        OnSelected += Select;
        OnSelected += GameManagerMap.Instance.CharacterMovemovent.CharacterSelect;
        OnSelected += GameManagerMap.Instance.TurnController.SetCharacter;


        OnDeselected += Disable;
        OnDeselected += GameManagerMap.Instance.CharacterMovemovent.CharacterDeselect;

        ActualTerritory = GameManagerMap.Instance.Map[transform.localPosition];
        ActualTerritory.TerritoryInfo = TerritoryType.Character;
        SetGunByIndex((int)GameManagerMap.Instance.Gun);

        CanvasController = GetComponent<CharacterCanvasController>();
        CountActions = 2;

        GameManagerMap.Instance.StatusMain.OnStatusChange += OnStatusChange;
        GameManagerMap.Instance.TurnController.CharacterBegining += BeginOfTurn;

    }

    public void OnIndexSet() => CanvasController.SetStartHealth(MaxHealthCharacter);

    private void BeginOfTurn() => CountActions = 2;

    private void OnMouseEnter()
    {
        if (!_selected && GameManagerMap.Instance.StatusMain.ActualPermissions.Contains(Permissions.SelectCharacter) && CountActions > 0)
            _selectItem.SetActive(true);
    }

    private void OnMouseExit()
    {
        if (!_selected && GameManagerMap.Instance.StatusMain.ActualPermissions.Contains(Permissions.SelectCharacter) && CountActions > 0)
            _selectItem.SetActive(false);
    }

    private void OnMouseDown()
    {
        if (ActualTerritory != null &&
            GameManagerMap.Instance.StatusMain.ActualPermissions.Contains(Permissions.SelectCharacter) && CountActions > 0)
        {
            if (_selected)
                OnDeselected();
            else
                OnSelected(this);
        }
    }

    private void Select(CharacterInfo character)
    {
        SelectChanges();
        MoverActive(true);
    }

    public void SelectChanges()
    {
        SetGunByIndex((int)GameManagerMap.Instance.Gun);
        _selectItem.SetActive(true);

        _selected = true;
        _selectItem.GetComponent<MeshRenderer>().material = _materialSelect;
    }

    public void DisableChanges()
    {
        _selectItem.SetActive(false);
        _selected = false;
        _selectItem.GetComponent<MeshRenderer>().material = _basicMaterial;
        _mover.transform.localPosition = new Vector3(0, _mover.transform.localPosition.y, 0);

        foreach (GameObject item in _shelterSize)
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

    private void OnStatusChange(HashSet<Permissions> permissions)
    {
        if (permissions.Count == 0)
        {
            GameManagerMap.Instance.StatusMain.OnStatusChange -= OnStatusChange;
            GameManagerMap.Instance.TurnController.CharacterBegining -= BeginOfTurn;

            return;
        }

        if (permissions.Contains(Permissions.ActionSelect) && !permissions.Contains(Permissions.SelectEnemy))
        {
            if (GameManagerMap.Instance.CharacterMovemovent.SelectedCharacter == this && GameManagerMap.Instance.CharacterMovemovent.SelectedCharacter != null)
                SetSelecter(true);
        }
        else if (permissions.Contains(Permissions.SelectEnemy))
        {
            if (GameManagerMap.Instance.CharacterMovemovent.SelectedCharacter == this && GameManagerMap.Instance.CharacterMovemovent.SelectedCharacter != null)
            {
                _selectItem.GetComponent<MeshRenderer>().material = _basicMaterial;
                SetSelecter(false);
            }
        }
    }

    private void SetSelecter(bool value)
    {
        _selectItem.SetActive(value);
        _selected = value;
    }

    public void MoverActive(bool result) => _mover.SetActive(result);
    public bool MoverActive() => _mover.activeSelf;

    public void SetCoordinatsToMover(Vector3 vector) => _mover.transform.position = vector;

    public bool IsActualTerritory(TerritroyReaded territory) => territory == ActualTerritory;
}
