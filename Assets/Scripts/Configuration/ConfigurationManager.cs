using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

public class ConfigurationManager : MonoBehaviour
{
    private readonly string PATH_GLOBAL_INFO = Application.streamingAssetsPath + "/configs/configGlobalInfo.json"; //ask aplication for folder than didnt build
    private readonly string PATH_CHARACTERS = Application.streamingAssetsPath + "/configs/characters"; ///configs/configCharacters.json

    private readonly string PATH_ENEMIES = Application.streamingAssetsPath + "/configs/enemies/"; //ask aplication for folder than didnt build

    private static ConfigurationManager _instance;
    public static ConfigurationManager Instance => _instance;

    private CharacterData[] _charactersData;
    /*
    Example of using: 
    ConfigurationManager.CharacterData.characterSpeed - return characterSpeed
    ConfigurationManager.CharacterData.typeGun[0].name - return first object name, AssultRifle for example
    */
    public static CharacterData[] CharactersData
    {
        get => Instance._charactersData;
        set => Instance._charactersData = value;
    }
    private GlobalDataJson _globalDataJson;

    public static GlobalDataJson GlobalDataJson
    {
        get => Instance._globalDataJson;
        set => Instance._globalDataJson = value;
    }

    private EnemyData[] _enemiesDataJson;

    public static EnemyData[] EnemiesDataJson
    {
        get => Instance._enemiesDataJson;
        set => Instance._enemiesDataJson = value;
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
    private T[] LoadConfigArray<T>(string fileDirectory)
    {
        if (Directory.Exists(fileDirectory))
        {
            string[] jsonFiles = Directory.GetFiles(fileDirectory, "*.json");
            T[] dataArray = new T[jsonFiles.Length];

            for (int i = 0; i < jsonFiles.Length; i++)
            {
                string jsonContent = File.ReadAllText(jsonFiles[i]);
                dataArray[i] = JsonUtility.FromJson<T>(jsonContent);
            }
            
            return dataArray;
        }
        else
        {
            Debug.LogError($"Config file with path {fileDirectory} not found!");
            return default;
        }
    }
    private void SaveConfig<T>(string filePath, T data)
    {
        string jsonContent = JsonUtility.ToJson(data, true); // The second parameter enables pretty-printing
        File.WriteAllText(filePath, jsonContent);
        Debug.Log($"Config file saved to {filePath}");
    }
    private void SaveConfigArray<T>(string fileDirectory, T[] dataArray)
    {
        string[] jsonFiles = Directory.GetFiles(fileDirectory, "*.json");
        for (int i = 0; i < dataArray.Length; i++)
        {
            string jsonContent = JsonUtility.ToJson(dataArray[i], true);
            string filePath = Path.Combine(fileDirectory, jsonFiles[i]);
            File.WriteAllText(filePath, jsonContent);
        }

        Debug.Log($"Config array saved to {fileDirectory}");
    }
    public void SaveAllConfigsFile()
    {
        SaveConfig(PATH_GLOBAL_INFO, _globalDataJson);
        SaveConfigArray(PATH_CHARACTERS, _charactersData);
        
        SaveConfigArray(PATH_ENEMIES, _enemiesDataJson);
    }
    public void LoadAllConfigsFile()
    {
        _globalDataJson = LoadConfig<GlobalDataJson>(PATH_GLOBAL_INFO);
        _enemiesDataJson = LoadConfigArray<EnemyData>(PATH_ENEMIES);

        _charactersData = LoadConfigArray<CharacterData>(PATH_CHARACTERS);
    }
}
