using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

// A part of the UI that is attached to the unit, showing
// useful info such as their health.
public abstract class UnitCanvas : MonoBehaviour
{
    private readonly Vector2 KOEFRANGEFORCREATINGCANVAS = new Vector2(50, 50);

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
    protected GameObject _panelModifiers, _modifierPrefab, _elementalReactionPrefab;

    [SerializeField]
    protected Image _actionIconPopUp;

    public GameObject CanvasGameObject => _canvas;

    public GameObject PanelMiss => CreateObjectPanel(_panelMiss);

    private Queue<(GameObject panel, float timer)> _panelsQueue = new ();
    private bool queueStarts = false;
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
        else if (Manager.CameraManager.AnimatedCamera.IsMainCamera())
            _canvas.transform.localRotation = Manager.CameraManager.AnimatedCamera.transform.localRotation;
    }

    public abstract void OnStatusChange(HashSet<Permissions> permissions);

    public void UpdateHealthUI(int hp)
    {
        int childCount = _panelHealthBar.transform.childCount; //get actual count of hp unit
        if (hp > childCount)
        {
            for (int i = 0; i < hp - childCount; i++) //its healing
                Instantiate(_hpPrefab, _panelHealthBar.transform);
        }
        else if (hp < childCount)
        {
            for (int i = 0; i < childCount - hp; i++) //its dmging
                Destroy(_panelHealthBar.transform.GetChild(childCount - i - 1).gameObject);
        }
    }

    public void UpdateModifiersUI(ModifierList modifiers)
    {
        ObjectUtils.DestroyAllChildren(_panelModifiers);

        foreach (Modifier modifier in modifiers.Modifiers)
        {
            GameObject modifierObject = Instantiate(_modifierPrefab, _panelModifiers.transform);
            modifierObject.GetComponent<ModifierUI>().Init(modifier);
        }
    }

    public void ShowReactions(List<ElementalReaction> reactions)
    {
        for (int i = 0; i < reactions.Count; i++)
        {
            ElementalReaction reaction = reactions[i];
            GameObject reactionObject = Instantiate(_elementalReactionPrefab, _canvas.transform);

            RectTransform transform = reactionObject.GetComponent<RectTransform>();
            Vector2 currentPosition = transform.anchoredPosition;
            currentPosition.y += 100f * i;
            transform.anchoredPosition = currentPosition;

            ElementalReactionUI reactionUI = reactionObject.GetComponent<ElementalReactionUI>();
            reactionUI.Init(reaction);
            StartCoroutine(AnimateReactionText(reactionUI));
        }
    }

    IEnumerator AnimateReactionText(ElementalReactionUI reactionUI)
    {
        TextMeshProUGUI text = reactionUI.Text;
        Color originalColor1 = text.color;

        Image image = reactionUI.GetComponent<Image>();
        Color originalColor2 = image.color;
        float timer = 0f;
        const float fadeDuration = 5f;
        const float moveSpeed = 0.10f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha1 = Mathf.Lerp(1.25f, 0f, timer / fadeDuration);
            text.color = new Color(originalColor1.r, originalColor1.g, originalColor1.b, alpha1);

            float alpha2 = Mathf.Lerp(0.5882353f, 0f, timer / fadeDuration);
            image.color = new Color(originalColor2.r, originalColor2.g, originalColor2.b, alpha2);

            // Move the text slightly in the direction of top right
            Vector3 moveDirection = Vector3.up + Vector3.right;
            image.rectTransform.position += moveDirection * Time.deltaTime * moveSpeed;

            yield return null;
        }

        Destroy(reactionUI.gameObject);
    }

    public GameObject PanelHit(int dmg, Element element)
    {
        _textHit.text = dmg.ToString();
        _textHit.color = ElementUtils.ElementColor(element);

        return CreateObjectPanel(_panelHit);
    }


    public GameObject PanelActionInfo(string action) => PanelActionInfo(action, action);

    public GameObject PanelActionInfo(string action, string iconName)
    {
        _textAction.text = action;
        _actionIconPopUp.sprite = Manager.UIIcons.GetIconByName(iconName)?.sprite;
        return CreateObjectPanel(_panelAction);
    }

    private GameObject CreateObjectPanel(GameObject _panel)
    {
        GameObject obj = Instantiate(_panel, _canvas.transform);
        obj.transform.localPosition += new Vector3(Random.Range(0, KOEFRANGEFORCREATINGCANVAS.x), Random.Range(0, KOEFRANGEFORCREATINGCANVAS.y));
        obj.name = _panel.name;
        return obj;
    }

    public IEnumerator PanelShow(GameObject panel, float timerBeforeDisable = 0) //refactoring => change type
    {
        _panelsQueue.Enqueue((panel, timerBeforeDisable));
        if (!queueStarts)
            StartCoroutine(PanelShowInQuenue());

        yield return new WaitForSeconds(timerBeforeDisable);
        //yield return StartCoroutine(PanelAnimation(panel: panel, speed: 1.4f, toNumberEverything: 1, toNumberPanel: panel.GetComponent<Image>().color.a, timeFinish: 2));
        //yield return new WaitForSeconds(timerBeforeDisable);
        //yield return StartCoroutine(PanelAnimation(panel: panel, speed: -0.5f, toNumberEverything: 0, toNumberPanel: 0, timeFinish: 2));
        //panel.SetActive(false);
    }

    public IEnumerator PanelShowInQuenue()
    {
        queueStarts = true;
        while(_panelsQueue.Count > 0)
        {
            var getQueue = _panelsQueue.Dequeue();
            yield return StartCoroutine(PanelAnimation(panel: getQueue.panel, speed: 1.4f, toNumberEverything: 1, toNumberPanel: getQueue.panel.GetComponent<Image>().color.a, timeFinish: 2));
            yield return new WaitForSeconds(getQueue.timer);
            StartCoroutine(PanelAnimation(panel: getQueue.panel, speed: -0.5f, toNumberEverything: 0, toNumberPanel: 0, timeFinish: 2));
        }
        queueStarts = false;
    }



    //Method than makes animathion appear or disappear panel with its children
    //If toNumberEverything = 0, it seems than panel is disappear
    //if disppear => speed < 0, if appear => speed > 0
    public IEnumerator PanelAnimation(GameObject panel, float speed, float toNumberEverything, float toNumberPanel, float timeFinish)
    {

        List<Image> colors = panel.GetComponentsInChildren<Image>().ToList(); //get all images of panel  //can be array?
        List<TextMeshProUGUI> texts = panel.GetComponentsInChildren<TextMeshProUGUI>().ToList(); //get all textes of panel

        Image panelColor = panel.GetComponent<Image>();
        float gValuePanelColor = panelColor.color.a; //save actual alpha of colour of panel

        if (toNumberEverything != 0) //if it is appear, set all objects on invisible
        {
            panelColor.color = new Color(panelColor.color.r, panelColor.color.g, panelColor.color.b, 0);
            colors.ForEach(n => n.color = new Color(n.color.r, n.color.g, n.color.b, 0));
            texts.ForEach(n => n.color = new Color(n.color.r, n.color.g, n.color.b, 0));
        }

        panel.SetActive(true);
        while ((panelColor.color.a < toNumberPanel && toNumberPanel != 0) || (panelColor.color.a > toNumberPanel && toNumberPanel == 0)
            || (colors.First().color.a < toNumberEverything && toNumberEverything != 0) || (colors.First().color.a > toNumberEverything && toNumberEverything == 0))
        { //cycle until the alpha values for the colours of the objects have the required values
            if ((panelColor.color.a < toNumberPanel && toNumberPanel != 0) || (panelColor.color.a > toNumberPanel && toNumberPanel == 0))
                panelColor.color = new Color(panelColor.color.r, panelColor.color.g, panelColor.color.b, panelColor.color.a + speed * Time.deltaTime);//add speed value to colour of panel

            if (colors.First().color.a != toNumberEverything)
            {
                colors.ForEach(n => n.color = new Color(n.color.r, n.color.g, n.color.b, panelColor.color.a + speed * Time.deltaTime)); //add speed value to colours of images
                texts.ForEach(n => n.color = new Color(n.color.r, n.color.g, n.color.b, panelColor.color.a + speed * Time.deltaTime));//add speed value to colours of texts
            }
            yield return null;
        }

        yield return new WaitForSeconds(timeFinish);

        panelColor.color = new Color(panelColor.color.r, panelColor.color.g, panelColor.color.b, gValuePanelColor); //set saved alpha parameter to color of panel
        if (speed < 0)
            Destroy(panel);
        else
        {
            colors.ForEach(n => n.color = new Color(n.color.r, n.color.g, n.color.b, 1)); //set all colours of images alpha value to 1
            texts.ForEach(n => n.color = new Color(n.color.r, n.color.g, n.color.b, 1));//set all colours of images alpha value to 1
        }
    }

    public void DisableAll()
    {
        foreach (Transform child in _canvas.transform)
        {
            if (child.gameObject.name != _panelHit.name && child.gameObject.name != _panelMiss.name)
                child.gameObject.SetActive(false);
        }
    }
}
