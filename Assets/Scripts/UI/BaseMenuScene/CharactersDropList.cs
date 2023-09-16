using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CharactersDropList : MonoBehaviour
{
    [SerializeField]
    private GameObject _prefab;
    public int pickedCharacter = 0;
    private Color _defaultColor;
    [SerializeField]
    private UIPoolController _uIPool;

    void Start()
    {
        InitCharacterList();
    }

    public void InitCharacterList()
    {
        for (int i = 0; i < ConfigurationManager.Instance.CharactersData.characters.Count(); i++)
        {
            CharacterData item = ConfigurationManager.Instance.CharactersData.characters[i];
            GameObject gameObject = Instantiate(_prefab, transform);

            Sprite desiredSpriteCharacterIcon = _uIPool.spritesCharacterIcons.FirstOrDefault(sprite => sprite.name == item.characterName.ToLower() + "-icon");

            Sprite desiredSpriteElement = _uIPool.spritesElementIcons.FirstOrDefault(sprite => sprite.name == "icon_element_" + item.element.ToLower());

            gameObject.GetComponentsInChildren<Image>()[1].sprite = desiredSpriteCharacterIcon;
            gameObject.GetComponentsInChildren<Image>()[2].sprite = desiredSpriteElement;

            Button button = gameObject.GetComponentInChildren<Button>();
            int characterIndex = i;
            _defaultColor = gameObject.GetComponentsInChildren<Image>()[0].color;
            button.onClick.AddListener(() => OnClickButton(characterIndex));
        }

        UpdateCharacterListColor();
    }
    private void OnClickButton(int characterID)
    {
        if (_uIPool.IsIDCharacterInArray(characterID))
        {
            Debug.Log("Is already in array");
        }
        else
        {
            pickedCharacter = characterID;
            UpdateCharacterListColor(); // maybe needs to be removed
            _uIPool.UpdateInfoCharacter(pickedCharacter);
        }
    }
    public void OnClickRemoveCharacter()
    {
        _uIPool.RemovedCharacter();
    }
    public void UpdateCharacterListColor()
    {
        if (transform.childCount != 0)
        {
            // 896B6B
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform childTransform = transform.GetChild(i);
                Image[] images = childTransform.GetComponentsInChildren<Image>();

                if (i == pickedCharacter)
                {
                    images[0].color = Color.red;
                }
                else if(_uIPool.IsIDCharacterInArray(i))
                {
                    images[0].color = Color.yellow;
                }
                else
                {
                    images[0].color = _defaultColor;
                }
            }
        }
    }
}
