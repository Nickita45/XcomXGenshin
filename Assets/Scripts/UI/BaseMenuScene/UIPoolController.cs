using System.Collections;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class UIPoolController : MonoBehaviour
{
    [SerializeField]
    private GameObject[] charactersModels;
    [SerializeField]
    private GameObject gameObjectPoolCharacters;
    private CharactersDropList poolCharacters;
    [SerializeField]
    private CharacterDescription _characterDescription;
    private int _characterPoolID = 0;
    public Sprite[] spritesElementIcons;
    public Sprite[] spritesCharacterIcons;
    [SerializeField]
    private RuntimeAnimatorController _animatorController;
    [SerializeField]
    private int[] charactersPoolID = new int[4] { 0, 1, 2, 3 }; // -1 skip

    [SerializeField]
    private GameObject _characterPoolMain, _characterPoolUI;

    public GameObject descriptionMain, descriptionUI;
    void Start()
    {
        poolCharacters = gameObjectPoolCharacters.GetComponentInChildren<CharactersDropList>();
        spritesElementIcons = Resources.LoadAll<Sprite>("Textures/elementsIcons");
        spritesCharacterIcons = Resources.LoadAll<Sprite>("Textures/CharactersIcon");

        for (int i = 0; i < charactersPoolID.Length; i++)
        {
            UpdateInfoCharacter(charactersPoolID[i]);
            _characterPoolID++;
        }
    }
    public void SelectCharacter(int id)
    {
        if (gameObjectPoolCharacters.activeSelf)
        {
            gameObjectPoolCharacters.SetActive(false);
        }
        else
        {
            _characterPoolID = id;
            gameObjectPoolCharacters.SetActive(true);

            poolCharacters.pickedCharacter = charactersPoolID[id];
            poolCharacters.UpdateCharacterListColor();
        }
    }
    public void SelectInfoCharacter(int id)
    {
        // this equal should be changed
        if (charactersPoolID[id] != -1)
        {
            // todo: need to change object active and deactive
            _characterPoolMain.SetActive(false);
            _characterPoolUI.SetActive(false);

            descriptionMain.SetActive(true);
            descriptionUI.SetActive(true);

            _characterDescription.UpdateCharacterDescription(charactersPoolID[id]);
        }
    }
    public void SetPoolCharacter()
    {
        _characterPoolMain.SetActive(true);
        _characterPoolUI.SetActive(true);

        gameObjectPoolCharacters.SetActive(false);

        descriptionMain.SetActive(false);
        descriptionUI.SetActive(false);
    }
    public void UpdateInfoCharacter(int pickedCharacter)
    {
        charactersPoolID[_characterPoolID] = pickedCharacter;
        gameObjectPoolCharacters.SetActive(false);
        charactersModels[_characterPoolID].SetActive(true);

        CharacterData character = ConfigurationManager.Instance.CharactersData.characters[pickedCharacter];
        charactersModels[_characterPoolID].GetComponentInChildren<TextMeshProUGUI>().text = character.characterName;

        Sprite desiredSpriteElement = spritesElementIcons.FirstOrDefault(sprite => sprite.name == "icon_element_" + character.element.ToLower());
        charactersModels[_characterPoolID].GetComponentInChildren<Image>().sprite = desiredSpriteElement;

        Object prefab = Resources.Load("Models/" + character.characterAvatarPath);
        GameObject _avatar = (GameObject)Instantiate(prefab, charactersModels[_characterPoolID].gameObject.transform);

        // Установите позицию _avatar так, чтобы он находился на том же уровне, что и charactersModels[_characterPoolID].
        _avatar.transform.position = charactersModels[_characterPoolID].GetComponentInChildren<Animator>().transform.position;

        // Установите угол поворота _avatar так, чтобы он соответствовал charactersModels[_characterPoolID].
        _avatar.transform.eulerAngles = new(0, 180, 0);
        _avatar.transform.localScale = charactersModels[_characterPoolID].GetComponentInChildren<Animator>().transform.localScale;

        Destroy(charactersModels[_characterPoolID].GetComponentInChildren<Animator>().gameObject);

        Animator _animator = _avatar.GetComponent<Animator>();
        _animator.runtimeAnimatorController = _animatorController;
    }
    public void RemovedCharacter()
    {
        gameObjectPoolCharacters.SetActive(false);
        charactersModels[_characterPoolID].SetActive(false);
        charactersPoolID[_characterPoolID] = -1;
    }
    public bool IsIDCharacterInArray(int id)
    {
        return charactersPoolID.Contains(id);
    }
}
