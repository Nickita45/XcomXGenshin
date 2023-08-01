using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerMap : MonoBehaviour
{
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


    private static GameManagerMap _instance;
    public static GameManagerMap Instance => _instance;

    public CharacterMovemovent CharacterMovemovent => _characterMovemovent;
    public CameraController CameraController => _cameraController;
    public CharacterVisibility CharacterVisibility => _characterVisibility;
    public GameObject MainParent => _mainParent;
    public GameObject GenereteTerritoryMove => _genereteTerritoryMove;

    public MatrixMap Map { get => _map; set => _map = value; }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }
}
