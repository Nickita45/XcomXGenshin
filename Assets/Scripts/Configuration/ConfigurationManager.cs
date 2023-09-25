using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

public class ConfigurationManager : MonoBehaviour
{
    private readonly string PATH_GLOBAL_INFO = Application.streamingAssetsPath + "/configs/configGlobalInfo.json"; //ask aplication for folder than didnt build
    private readonly string PATH_CHARACTERS = Application.streamingAssetsPath + "/configs/configCharacters.json"; //ask aplication for folder than didnt build


    private static ConfigurationManager _instance;
    public static ConfigurationManager Instance => _instance;

    private CharactersData _charactersData;
    /*
    Example of using: 
    ConfigurationManager.Instance.CharacterData.characterSpeed - return characterSpeed
    ConfigurationManager.Instance.CharacterData.typeGun[0].name - return first object name, AssultRifle for example
    */
    public CharactersData CharactersData
    {
        get => _charactersData;
        set => _charactersData = value;
    }
    private GlobalDataJson _globalDataJson;
    
    public GlobalDataJson GlobalDataJson
    {
        get => _globalDataJson;
        set => _globalDataJson = value;
    }
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }
        LoadAllConfigsFile();

        _instance = this;
    }

    private T LoadConfig<T>(string filePath)
    {
        if (File.Exists(filePath))
        {
            string jsonContent = File.ReadAllText(filePath);
            T data = JsonUtility.FromJson<T>(jsonContent);
            return data;
        }
        else
        {
            Debug.LogError($"Config file with path {filePath} not found!");
            return default;
        }
        
    }
    private void SaveConfig<T>(string filePath, T data)
    {
        string jsonContent = JsonUtility.ToJson(data, true); // The second parameter enables pretty-printing
        File.WriteAllText(filePath, jsonContent);
        Debug.Log($"Config file saved to {filePath}");
    }
    public void SaveAllConfigsFile()
    {
        SaveConfig(PATH_GLOBAL_INFO, _globalDataJson);
        SaveConfig(PATH_CHARACTERS, _charactersData);
    }
    public void LoadAllConfigsFile()
    {
        _globalDataJson = LoadConfig<GlobalDataJson>(PATH_GLOBAL_INFO);
        _charactersData = LoadConfig<CharactersData>(PATH_CHARACTERS); 
    }
}
