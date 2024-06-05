using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HubData : MonoBehaviour
{
    [SerializeField]
    private ParticleSystemFactory _particleSystemFactory;
    public static HubData Instance => _instance;
    private static HubData _instance;
    public int[] charactersPoolID = new int[4] { 0, 1, 2, 3 }; // -1 skip //not safety
    public string[] enemiesPaths; // might make this automatic later //not safety

    public ParticleSystemFactory ParticleSystemFactory => _particleSystemFactory;
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public string GetRandomEnemyPath() => enemiesPaths[UnityEngine.Random.Range(0, enemiesPaths.Length)];
}
