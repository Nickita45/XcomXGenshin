using UnityEngine;

public class ConfigInfoPanelDisplay : MonoBehaviour
{
    [SerializeField]
    private GameObject _openButton;

    private bool _enabled = false;

    public void Toggle()
    {
        _enabled = !_enabled;

        gameObject.SetActive(_enabled);
        _openButton.SetActive(!_enabled);
        GameManagerMap.Instance.FreeCameraController.enabled = !_enabled;
    }
}
