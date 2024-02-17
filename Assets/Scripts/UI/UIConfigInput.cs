using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class UIConfigInput : MonoBehaviour
{
    public GameObject characterUIPrefab;

    public Transform parentTransform;
    private CharacterData[] _characterList;
    private GlobalDataJson _globalData;
    private Dictionary<string, EnemyData> _enemyData;
    private int _statusConfig = 0; // 0 - GlobalDataJson, 1 - CharactersData
    private void Start()
    {
        _characterList = ConfigurationManager.CharactersData;
        _globalData = ConfigurationManager.GlobalDataJson;
        _enemyData = ConfigurationManager.EnemiesDataJson;
        GenerateContentByStatus(0);
    }
    // Removed all previous items in content. Generate content value inside scroll by status value.
    // Special check if its array content array
    // TODO: automation this process by parsing all values and make a small structure. At the moment put all singe value before array
    public void GenerateContentByStatus(int status)
    {
        _statusConfig = status;
        ObjectUtils.DestroyAllChildren(parentTransform.gameObject);
        switch (_statusConfig)
        {
            // Global Info
            case 0:
                FillValuesUI(_globalData);
                foreach (GunTypeConfig gun in _globalData.typeGun)
                {
                    FillValuesUI(gun);
                }
                break;
            // Fillter for array type
            // ChaRacters
            case 1:
                foreach (CharacterData character in _characterList)
                {
                    /*
                    GameObject characterUI = Instantiate(characterUIPrefab, parentTransform);
                    characterUI.GetComponentInChildren<TextMeshProUGUI>().text = $"{character.characterName}:";
                    characterUI.GetComponentInChildren<TMP_InputField>().gameObject.SetActive(false);*/
                    FillValuesUI(character);
                }
                break;
            case 2:
                foreach (EnemyData enemy in _enemyData.Values)
                {
                    FillValuesUI(enemy);
                }
                break;
        }
    }
    // Parse generic data by fields, put all data in prefab with text and inputvalue. Using a inputfield name as storage of info 
    private void FillValuesUI<T>(T data)
    {
        FieldInfo[] fields = data.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (FieldInfo field in fields)
        {
            GameObject InputFieldUI = Instantiate(characterUIPrefab, parentTransform);

            TextMeshProUGUI textComponent = InputFieldUI.GetComponentInChildren<TextMeshProUGUI>();
            TMP_InputField inputFieldComponent = InputFieldUI.GetComponentInChildren<TMP_InputField>();

            string propertyName = field.Name;
            var propertyValue = field.GetValue(data);
            var propertyType = propertyValue.GetType();

            textComponent.text = $"{propertyName}";

            if (propertyType.IsArray)
            {
                inputFieldComponent.gameObject.SetActive(false);
            }

            switch (propertyType.ToString())
            {
                case "System.Int32":
                    inputFieldComponent.contentType = TMP_InputField.ContentType.IntegerNumber;
                    break;
                case "System.Single":
                    inputFieldComponent.contentType = TMP_InputField.ContentType.DecimalNumber;
                    break;
                case "System.Boolean":
                    inputFieldComponent.contentType = TMP_InputField.ContentType.Standard;
                    break;
            }
            inputFieldComponent.name = $"InputField:{propertyName}:{propertyType}";
            inputFieldComponent.text = $"{propertyValue}";
        }
    }
    // Save all inputs information. Splitted by special values parametres. If it array can be run one time and that's all. 
    // TODO: change a bit logic, automate process and added a class in singleton to update all values.
    public void UpdateConfigValues()
    {
        TMP_InputField[] tMP_Inputs = parentTransform.GetComponentsInChildren<TMP_InputField>();

        switch (_statusConfig)
        {
            case 0:
                List<List<TMP_InputField>> splittedArray = SplitArrayByObjectTypeArray(_globalData, tMP_Inputs);

                UpdateArrayValues(_globalData, splittedArray[0].ToArray());
                for (int i = 0; i < _globalData.typeGun.Length; i++)
                {
                    TMP_InputField[] tMP_InputsSubset = GetSubsetOfInputs(splittedArray[1].ToArray(), i, _globalData.typeGun[i]);
                    UpdateArrayValues(_globalData.typeGun[i], tMP_InputsSubset);
                }
                break;
            case 1:
                for (int i = 0; i < _characterList.Length; i++)
                {
                    TMP_InputField[] tMP_InputsSubset = GetSubsetOfInputs(tMP_Inputs, i, _characterList[i]);
                    UpdateArrayValues(_characterList[i], tMP_InputsSubset);
                }
                break;
            case 2:
                foreach (string key in _enemyData.Keys)
                {
                    // TODO TMP_InputField[] tMP_InputsSubset = GetSubsetOfInputs(tMP_Inputs, i, _enemyData[i]);
                    // UpdateArrayValues(_enemyData[key], tMP_InputsSubset);
                }
                break;
        }
        // Updating singleton data and parameters
        ConfigurationManager.CharactersData = _characterList;
        ConfigurationManager.GlobalDataJson = _globalData;
        ConfigurationManager.EnemiesDataJson = _enemyData;

        ConfigurationManager.Instance.SaveAllConfigsFile();
        ConfigurationManager.Instance.LoadAllConfigsFile();
    }
    // Get all information from fields for entiring tmp inputs. Set the value for generic value
    public void UpdateArrayValues<T>(T data, TMP_InputField[] tMP_Inputs)
    {
        FieldInfo[] fields = data.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        for (int j = 0; j < fields.Length; j++)
        {
            if (tMP_Inputs.Length == j)
                break;

            string[] splittedName = tMP_Inputs[j].name.Split(':');

            var fieldValue = ParseFieldValue(splittedName[2], tMP_Inputs[j].text);
            fields[j].SetValue(data, fieldValue);
        }
    }
    // Splitted object by 2 variants: scalar values and array values
    public List<List<TMP_InputField>> SplitArrayByObjectTypeArray<T>(T data, TMP_InputField[] tMP_Inputs)
    {
        List<TMP_InputField> scalarInputFields = new List<TMP_InputField>();
        List<TMP_InputField> arrayInputFields = new List<TMP_InputField>(tMP_Inputs);
        FieldInfo[] fields = data.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        for (int i = 0; i < tMP_Inputs.Length; i++)
        {
            if (fields.Length - 1 == i)
                break;

            scalarInputFields.Add(arrayInputFields[0]);
            arrayInputFields.Remove(arrayInputFields[0]);
        }
        List<List<TMP_InputField>> splitArrays = new()
        {
            scalarInputFields,
            arrayInputFields
        };
        return splitArrays;
    }
    // Return input fields by count of row in generic data
    private TMP_InputField[] GetSubsetOfInputs<T>(TMP_InputField[] allInputs, int characterIndex, T data)
    {
        int fieldsPerCharacter = data.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length;
        int startIndex = characterIndex * fieldsPerCharacter;
        int endIndex = startIndex + fieldsPerCharacter;

        TMP_InputField[] subsetInputs = new TMP_InputField[fieldsPerCharacter];
        for (int i = startIndex, j = 0; i < endIndex; i++, j++)
        {
            subsetInputs[j] = allInputs[i];
        }

        return subsetInputs;
    }
    // Parse value from object to different type from inputfield type
    private object ParseFieldValue(string fieldType, string textValue)
    {
        switch (fieldType)
        {
            case "System.Int32":
                return int.Parse(textValue);
            case "System.Single":
                return float.Parse(textValue);
            case "System.Boolean":
                return bool.Parse(textValue);
            default:
                return textValue;
        }
    }
}

