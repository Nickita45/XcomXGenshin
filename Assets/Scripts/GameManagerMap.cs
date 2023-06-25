using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerMap : MonoBehaviour
{
    public MatrixMap Map { get; set; }

    [SerializeField]
    private CharacterMovemovent _characterMovemovent;
    [SerializeField]
    private GameObject _mainParent;
    [SerializeField]
    private GameObject _genereteTerritoryMove;


    private static GameManagerMap _instance;
    public static GameManagerMap Instance => _instance;

    public CharacterMovemovent CharacterMovemovent => _characterMovemovent;
    public GameObject MainParent => _mainParent;
    public GameObject GenereteTerritoryMove => _genereteTerritoryMove;

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
