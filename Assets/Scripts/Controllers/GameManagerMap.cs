using System;
using UnityEngine;

public class GameManagerMap : MonoBehaviour
{
    [Header("Serialize elemnets")]
    [SerializeField]
    private DeReadingMap _deReadingMap;

    public DeReadingMap DeReadingMap => _deReadingMap;

    [Header("Game elements")]
    [SerializeField]
    private MatrixMap _map;

    [SerializeField]
    private CharacterMovemovent _characterMovemovent;

    [SerializeField]
    private CameraController _cameraController;

    [SerializeField]
    private CharacterVisibility _characterVisibility;

    [Header("MainObjects")]
    [SerializeField]
    private GameObject _mainParent;
    [SerializeField]
    private GameObject _genereteTerritoryMove;

    [Header("Plane to movement")]
    [SerializeField]
    private GameObject _prefabPossibleTerritory;


    private static GameManagerMap _instance;
    public static GameManagerMap Instance => _instance;

    public CharacterMovemovent CharacterMovemovent => _characterMovemovent;
    public CameraController CameraController => _cameraController;
    public CharacterVisibility CharacterVisibility => _characterVisibility;
    public GameObject MainParent => _mainParent;
    public GameObject GenereteTerritoryMove => _genereteTerritoryMove;

    public MatrixMap Map { get => _map; set => _map = value; }

    public Action OnClearMap;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    private void Start()
    {
        OnClearMap += ClearMap;
    }

    public GameObject CreatePlatformMovement(TerritroyReaded item)
    {
        var obj = Instantiate(_prefabPossibleTerritory, GameManagerMap.Instance.GenereteTerritoryMove.transform);
        obj.transform.localPosition = item.GetCordinats() - CharacterMovemovent.POSITIONFORSPAWN;
        obj.SetActive(false);
        return obj;
    }

    private void ClearMap()
    {
        Map = null;
        DeleteAllChildren(MainParent);
        DeleteAllChildren(GenereteTerritoryMove);

       /* foreach(GameObject item in MainParent.transform)
        {
            Destroy(item);
        }

        foreach (GameObject item in GenereteTerritoryMove.transform)
        {
            Destroy(item);
        }*/
    }

    private static void DeleteAllChildren(GameObject parent)
    {
        int childCount = parent.transform.childCount;

        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = parent.transform.GetChild(i);
            DestroyImmediate(child.gameObject);
        }
    }
}
