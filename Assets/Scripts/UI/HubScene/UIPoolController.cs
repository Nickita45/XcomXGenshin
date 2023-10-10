using System.Collections;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class UIPoolController : MonoBehaviour
{
    [SerializeField]
    private GameObject[] _charactersModels;
    [SerializeField]
    private GameObject _gameObjectPoolCharacters;
    private CharactersDropList _poolCharacters;
    [SerializeField]
    private CharacterDescription _characterDescription;
    private int _characterPoolID = 0;
    public Sprite[] spritesElementIcons;
    public Sprite[] spritesCharacterIcons;
    [SerializeField]
    private RuntimeAnimatorController _animatorController;
    [SerializeField]

    private GameObject _characterPoolMain, _characterPoolUI;

    public GameObject descriptionMain, descriptionUI;
    [SerializeField]
    private Button _buttonStartGame;
    void Start()
    {
        _poolCharacters = _gameObjectPoolCharacters.GetComponentInChildren<CharactersDropList>();
        spritesElementIcons = Resources.LoadAll<Sprite>("Textures/elementsIcons");
        spritesCharacterIcons = Resources.LoadAll<Sprite>("Textures/CharactersIcon");

        for (int i = 0; i < HubData.Instance.charactersPoolID.Length; i++)
        {
            UpdateInfoCharacter(HubData.Instance.charactersPoolID[i]);
            _characterPoolID++;
        }
    }
    public void SelectCharacter(int id)
    {
        if (_gameObjectPoolCharacters.activeSelf)
        {
            _gameObjectPoolCharacters.SetActive(false);
        }
        else
        {
            _characterPoolID = id;
            _gameObjectPoolCharacters.SetActive(true);

            _poolCharacters.pickedCharacter = HubData.Instance.charactersPoolID[id];
            _poolCharacters.UpdateCharacterListColor();
        }
    }
    public void SelectInfoCharacter(int id)
    {
        // this equal should be changed
        if (HubData.Instance.charactersPoolID[id] != -1)
        {
            // todo: need to change object active and deactive
            _characterPoolMain.SetActive(false);
            _characterPoolUI.SetActive(false);

            descriptionMain.SetActive(true);
            descriptionUI.SetActive(true);

            _characterDescription.UpdateCharacterDescription(HubData.Instance.charactersPoolID[id]);
        }
    }
    public void SetPoolCharacter()
    {
        _characterPoolMain.SetActive(true);
        _characterPoolUI.SetActive(true);

        _gameObjectPoolCharacters.SetActive(false);

        descriptionMain.SetActive(false);
        descriptionUI.SetActive(false);
    }
    public void UpdateInfoCharacter(int pickedCharacter)
    {
        HubData.Instance.charactersPoolID[_characterPoolID] = pickedCharacter;
        _gameObjectPoolCharacters.SetActive(false);
        _charactersModels[_characterPoolID].SetActive(true);

        CharacterData character = ConfigurationManager.CharactersData.characters[pickedCharacter];
        _charactersModels[_characterPoolID].GetComponentInChildren<TextMeshProUGUI>().text = character.characterName;

        Sprite desiredSpriteElement = spritesElementIcons.FirstOrDefault(sprite => sprite.name == "icon_element_" + character.element.ToLower());
        _charactersModels[_characterPoolID].GetComponentInChildren<Image>().sprite = desiredSpriteElement;

        Object prefab = Resources.Load("Models/" + character.characterAvatarPath);
        GameObject _avatar = (GameObject)Instantiate(prefab, _charactersModels[_characterPoolID].gameObject.transform);

        _avatar.transform.position = _charactersModels[_characterPoolID].GetComponentInChildren<Animator>().transform.position;

        _avatar.transform.eulerAngles = new(0, 180, 0);
        _avatar.transform.localScale = _charactersModels[_characterPoolID].GetComponentInChildren<Animator>().transform.localScale;

        Destroy(_charactersModels[_characterPoolID].GetComponentInChildren<Animator>().gameObject);

        Animator _animator = _avatar.GetComponent<Animator>();
        _animator.runtimeAnimatorController = _animatorController;
        SetButtonSceneIfEmptyPool();
    }
    public void RemovedCharacter()
    {
        _gameObjectPoolCharacters.SetActive(false);
        _charactersModels[_characterPoolID].SetActive(false);
        HubData.Instance.charactersPoolID[_characterPoolID] = -1;
        SetButtonSceneIfEmptyPool();

    }
    private void SetButtonSceneIfEmptyPool()
    {
        bool isAllNegative = HubData.Instance.charactersPoolID.All(x => x == -1);
        _buttonStartGame.gameObject.SetActive(!isAllNegative);
    }
    public bool IsIDCharacterInArray(int id)
    {
        return HubData.Instance.charactersPoolID.Contains(id);
    }
    // TODO: MOVE TO ANOTHER SCRIPT AND MAKE A CHECK IF ANY CHAEACTER PICKED
    public void SceneLoad(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}