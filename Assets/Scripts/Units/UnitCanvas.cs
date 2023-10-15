using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A part of the UI that is attached to the unit, showing
// useful info such as their health.
public abstract class UnitCanvas : MonoBehaviour
{
    [SerializeField]
    protected GameObject _canvas;

    [Header("Basic")]
    [SerializeField]
    protected GameObject _panelMiss, _panelHit, _panelAction;

    [SerializeField]
    protected TextMeshProUGUI _textHit, _textAction;

    [SerializeField]
    protected GameObject _panelHealthBar, _hpPrefab;

    [SerializeField]
    protected Image _actionIconPopUp;

    public GameObject PanelMiss => _panelMiss;
    public GameObject PanelAction => _panelAction;

    public virtual void Start()
    {
        Manager.StatusMain.OnStatusChange += OnStatusChange;
    }

    public virtual void OnDestroy()
    {
        Manager.StatusMain.OnStatusChange -= OnStatusChange;
    }

    public virtual void Update()
    {
        if (Manager.CameraManager.FixedCamera.IsMainCamera())
        {
            Vector3 direction = Manager.CameraManager.FixedCamera.transform.position - _canvas.transform.position;
            _canvas.transform.localRotation = Quaternion.LookRotation(-direction);
        }

        else if (Manager.CameraManager.FreeCamera.IsMainCamera())
            _canvas.transform.localRotation = Manager.CameraManager.FreeCamera.transform.localRotation;
    }

    public abstract void OnStatusChange(HashSet<Permissions> permissions);

    public void SetStartHealth(int hp)
    {
        int childCount = _panelHealthBar.transform.childCount;
        if (hp > childCount)
        {
            for (int i = 0; i < hp - childCount; i++)
                Instantiate(_hpPrefab, _panelHealthBar.transform);
        }
        else if (hp < childCount)
        {
            for (int i = 0; i < childCount - hp; i++)
                Destroy(_panelHealthBar.transform.GetChild(childCount - i - 1).gameObject);
        }
    }

    public GameObject PanelHit(int dmg)
    {
        _textHit.text = dmg.ToString();
        return _panelHit;
    }


    public GameObject PanelActionInfo(string action) => PanelActionInfo(action, action);

    public GameObject PanelActionInfo(string action, string iconName)
    {
        _textAction.text = action;
        _actionIconPopUp.sprite = Manager.UIIcons.GetIconByName(iconName)?.sprite;
        return _panelAction;
    }

    public IEnumerator PanelShow(GameObject panel, float timerBeforeDisable = 0)
    {
        yield return StartCoroutine(PanelAnimation(panel: panel, speed: 1.4f, toNumberEverything: 1, toNumberPanel: panel.GetComponent<Image>().color.a, timeFinish: 2));
        yield return new WaitForSeconds(timerBeforeDisable);
        yield return StartCoroutine(PanelAnimation(panel: panel, speed: -0.5f, toNumberEverything: 0, toNumberPanel: 0, timeFinish: 2));
        panel.SetActive(false);
    }



    //Method than makes animathion appear or disappear panel with its children
    //If toNumberEverything = 0, it seems than panel is disappear
    //if disppear => speed < 0, if appear => speed > 0
    public IEnumerator PanelAnimation(GameObject panel, float speed, float toNumberEverything, float toNumberPanel, float timeFinish)
    {

        List<Image> colors = panel.GetComponentsInChildren<Image>().ToList(); //can be array?
        List<TextMeshProUGUI> texts = panel.GetComponentsInChildren<TextMeshProUGUI>().ToList();

        Image panelColor = panel.GetComponent<Image>();
        float gValuePanelColor = panelColor.color.a;

        if (toNumberEverything != 0)
        {
            panelColor.color = new Color(panelColor.color.r, panelColor.color.g, panelColor.color.b, 0);
            colors.ForEach(n => n.color = new Color(n.color.r, n.color.g, n.color.b, 0));
            texts.ForEach(n => n.color = new Color(n.color.r, n.color.g, n.color.b, 0));
        }

        panel.SetActive(true);
        while ((panelColor.color.a < toNumberPanel && toNumberPanel != 0) || (panelColor.color.a > toNumberPanel && toNumberPanel == 0)
            || (colors.First().color.a < toNumberEverything && toNumberEverything != 0) || (colors.First().color.a > toNumberEverything && toNumberEverything == 0))
        {
            if ((panelColor.color.a < toNumberPanel && toNumberPanel != 0) || (panelColor.color.a > toNumberPanel && toNumberPanel == 0))
                panelColor.color = new Color(panelColor.color.r, panelColor.color.g, panelColor.color.b, panelColor.color.a + speed * Time.deltaTime);

            if (colors.First().color.a != toNumberEverything)
            {
                colors.ForEach(n => n.color = new Color(n.color.r, n.color.g, n.color.b, panelColor.color.a + speed * Time.deltaTime));
                texts.ForEach(n => n.color = new Color(n.color.r, n.color.g, n.color.b, panelColor.color.a + speed * Time.deltaTime));
            }
            yield return null;
        }

        yield return new WaitForSeconds(timeFinish);

        panelColor.color = new Color(panelColor.color.r, panelColor.color.g, panelColor.color.b, gValuePanelColor);

        colors.ForEach(n => n.color = new Color(n.color.r, n.color.g, n.color.b, 1));
        texts.ForEach(n => n.color = new Color(n.color.r, n.color.g, n.color.b, 1));
    }

    public void DisableAll()
    {
        foreach (Transform child in _canvas.transform)
        {
            if (child.gameObject != _panelHit && child.gameObject != _panelMiss)
                child.gameObject.SetActive(false);
        }
    }
}
