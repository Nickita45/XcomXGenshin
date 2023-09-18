using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HubData : MonoBehaviour
{
    private static HubData _instance;
    public static HubData Instance => _instance;
    public int[] charactersPoolID = new int[4] { 0, 1, 2, 3 }; // -1 skip
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
    
}
