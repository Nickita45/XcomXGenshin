using UnityEngine;

public class ConfigInfoPanelDisplay : MonoBehaviour
{
    [SerializeField]
    private GameObject _openButton;

    public void Toggle()
    {
        bool enabled = !gameObject.activeSelf;

        gameObject.SetActive(enabled);
        _openButton.SetActive(!enabled);
        GameManagerMap.Instance.FreeCameraController.enabled = !enabled;
    }
}
