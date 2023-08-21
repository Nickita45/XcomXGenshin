using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

public class ConfigurationManager : MonoBehaviour
{
    private readonly string PATH = Application.streamingAssetsPath + "/configs/configTest.json"; //ask aplication for folder than didnt build

    public static ConfigurationManager _instance;
    public static ConfigurationManager Instance => _instance;

    private CharacterData _characterData;
    /*
    Example of using: 
    ConfigurationManager.Instance.CharacterData.characterSpeed - return characterSpeed
    ConfigurationManager.Instance.CharacterData.typeGun[0].name - return first object name, AssultRifle for example
    */
    public CharacterData CharacterData => _characterData;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        LoadConfig();
        _instance = this;
    }

    private void LoadConfig()
    {
        if (File.Exists(PATH)) //works like a file 
        {
            _characterData = JsonUtility.FromJson<CharacterData>(File.ReadAllText(PATH));
        }
        else
        {
            Debug.LogError("Config file not found!");
        }
    }
}
