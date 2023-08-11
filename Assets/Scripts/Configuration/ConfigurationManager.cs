using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

public class ConfigurationManager : MonoBehaviour
{
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
        TextAsset configText = Resources.Load<TextAsset>("configs/configTest");
        if (configText != null)
        {
            _characterData = JsonUtility.FromJson<CharacterData>(configText.text);
        }
        else
        {
            Debug.LogError("Config file not found!");
        }
    }
}
[System.Serializable]
public class GunType
{
    public string name;
    public int distanceValue;
    public int baseValue;
    public int minHitValue;
    public int maxHitValue;
}

[System.Serializable]
public class CharacterData
{
    public int characterSpeed;
    public float characterMoveDistance;
    public float characterVisionDistance;
    public int characterBaseAim;
    public int characterBaseHealth;

    public int bonusAimFromFullCover;
    public int bonusAimFromHalfCover;
    public int bonusAimFromNoneCover;

    public int bonusAimFromHighGround;
    public int bonusAimFromLowGround;
    public int bonusAimFromNoneGround;

    public GunType[] typeGun;
}