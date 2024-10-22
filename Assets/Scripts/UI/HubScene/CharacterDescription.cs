using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterDescription : MonoBehaviour
{
    [SerializeField]
    private Image _spriteElement;
    [SerializeField]
    private TextMeshProUGUI _characterName;
    [SerializeField]
    private TextMeshProUGUI _healthValue, _mobilityValue, _aimValue;
    [SerializeField]
    private UIPoolController _uIPool;

    public void UpdateCharacterDescription(int characterID)
    {
        CharacterData character = ConfigurationManager.CharactersData[characterID];

        _spriteElement.sprite = _uIPool.spritesElementIcons.FirstOrDefault(sprite => sprite.name == "icon_element_" + character.element.ToString().ToLower());

        _characterName.text = character.element + " / " + character.characterName;

        _healthValue.text = character.characterBaseHealth.ToString();
        _mobilityValue.text = character.characterMoveDistance.ToString();
        _aimValue.text = character.characterBaseAim.ToString();

        Object prefab = Resources.Load("Models/" + character.characterAvatarPath);
        GameObject _avatar = (GameObject)Instantiate(prefab, _uIPool.descriptionMain.transform);

        _avatar.transform.position = _uIPool.descriptionMain.GetComponentInChildren<Animator>().transform.position;

        _avatar.transform.eulerAngles = new(0, 180, 0);
        _avatar.transform.localScale = _uIPool.descriptionMain.GetComponentInChildren<Animator>().transform.localScale;

        Destroy(_uIPool.descriptionMain.GetComponentInChildren<Animator>().gameObject);
    }
}
