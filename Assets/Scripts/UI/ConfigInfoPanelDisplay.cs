using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigInfoPanelDisplay : MonoBehaviour
{
    public GameObject _openButton;
    public GameObject _exitButton;
    public void Show()
    {
        gameObject.SetActive(true);
        _openButton.SetActive(false);
        _exitButton.SetActive(true);
        GameManagerMap.Instance.FreeCameraController.enabled = false;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        _exitButton.SetActive(false);
        _openButton.SetActive(true);
        GameManagerMap.Instance.FreeCameraController.enabled = true;
    }
}
