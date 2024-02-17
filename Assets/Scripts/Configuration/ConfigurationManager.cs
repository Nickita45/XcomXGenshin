using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

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

    private Dictionary<string, EnemyData> _enemiesDataJson;

    public static Dictionary<string, EnemyData> EnemiesDataJson
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
            Debug.LogError($"Config directory with path {fileDirectory} not found!");
            return default;
        }
    }

    private Dictionary<string, T> LoadConfigDictionary<T>(string fileDirectory)
    {
        if (Directory.Exists(fileDirectory))
        {
            string[] jsonFiles = Directory.GetFiles(fileDirectory, "*.json");
            Dictionary<string, T> dataDictionary = new();

            for (int i = 0; i < jsonFiles.Length; i++)
            {
                string jsonContent = File.ReadAllText(jsonFiles[i]);
                string key = Path.GetFileNameWithoutExtension(jsonFiles[i])["config".Length..];

                dataDictionary.Add(key, JsonUtility.FromJson<T>(jsonContent));
            }

            return dataDictionary;
        }
        else
        {
            Debug.LogError($"Config file with path {fileDirectory} not found!");
            return default;
        }
    }

    private void SaveConfig<T>(string filePath, T data)
    {
        string jsonContent = JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            FloatFormatHandling = FloatFormatHandling.String
        });

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

        // TODO SaveConfigDictionary(PATH_ENEMIES, _enemiesDataJson);
    }
    public void LoadAllConfigsFile()
    {
        _globalDataJson = LoadConfig<GlobalDataJson>(PATH_GLOBAL_INFO);
        _enemiesDataJson = LoadConfigDictionary<EnemyData>(PATH_ENEMIES);

        _charactersData = LoadConfigArray<CharacterData>(PATH_CHARACTERS);
    }
}
