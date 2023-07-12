using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MenuController : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField _inputCharacterSpeed;

    [SerializeField]
    private TMP_InputField _inputCharacterMove;


    private void Start()
    {
        _inputCharacterMove.text = GameManagerMap.Instance.CharacterMovemovent.CountMoveCharacter.ToString();
        _inputCharacterSpeed.text = GameManagerMap.Instance.CharacterMovemovent.SpeedCharacter.ToString();

        _inputCharacterMove.onValueChanged.AddListener(n =>
        {

            int result = 0;
            int.TryParse(n,out result);
            GameManagerMap.Instance.CharacterMovemovent.CountMoveCharacter = result;
        });

        _inputCharacterSpeed.onValueChanged.AddListener(n =>
        {
            float result = 0;
            float.TryParse(n, out result);
            GameManagerMap.Instance.CharacterMovemovent.SpeedCharacter = result;
        });
    }

}
