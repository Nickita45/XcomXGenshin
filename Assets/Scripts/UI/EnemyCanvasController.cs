using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyCanvasController : MonoBehaviour //change for Enemy and Character, maybe can be renamed
{
    [Header("Basic")]
    [SerializeField]
    private GameObject _panelMiss, _panelHit;

    [SerializeField]
    private TextMeshProUGUI textHit;

    [SerializeField]
    private GameObject _panelHealthBar, _hpPrefab;

    [SerializeField]
    private List<GameObject> _listDidntRotateObjects; //need to be HashSet
    private HashSet<GameObject> _objectCantBeRotated;


    protected Camera _actualCamera;

    protected Camera _fixedCamera, _freeCamera;

    protected GameObject _canvasToMove;

    public GameObject PanelMiss => _panelMiss;
    public GameObject PanelHit(int dmg)
    {
        textHit.text = dmg.ToString();
        return _panelHit;
    }

    public GameObject CanvasToMove => _canvasToMove;
    protected virtual void Start()
    {
        _fixedCamera = FindObjectOfType<FixedCameraController>().gameObject.GetComponent<Camera>();
        _freeCamera = FindObjectOfType<FreeCameraController>().gameObject.GetComponent<Camera>();
        _canvasToMove = transform.GetComponentInChildren<Canvas>().gameObject;
        GameManagerMap.Instance.StatusMain.OnStatusChange += OnStatusChange;

        _objectCantBeRotated = new HashSet<GameObject>(_listDidntRotateObjects);

        _actualCamera = _freeCamera;
    }

    private void Update()
    {
        if (_actualCamera == _fixedCamera)
            _canvasToMove.transform.LookAt(_actualCamera.transform);
        else if (_actualCamera == _freeCamera)
            _canvasToMove.transform.localRotation = _freeCamera.transform.localRotation;
    }

    protected virtual void OnStatusChange(HashSet<Permissions> permissions)
    {
        if (permissions.Count == 0)
        {
            GameManagerMap.Instance.StatusMain.OnStatusChange -= OnStatusChange;
            return;
        }

        if (permissions.Contains(Permissions.SelectEnemy) || permissions.Contains(Permissions.AnimationShooting))
        {
            _actualCamera = _fixedCamera;
            if (_canvasToMove != GameManagerMap.Instance.EnemyPanel.CanvasEnemyObject)
                _canvasToMove.gameObject.SetActive(false);
            else
                _canvasToMove.gameObject.SetActive(true);


            foreach (Transform child in _canvasToMove.transform)
            {
                if(!_objectCantBeRotated.Contains(child.gameObject))
                    child.localEulerAngles = new Vector3(0, 180, 0);
            }
        }
        else
        {
            _canvasToMove.gameObject.SetActive(true);
            
            _actualCamera = _freeCamera;

            foreach (Transform child in _canvasToMove.transform)
            {
                if (!_objectCantBeRotated.Contains(child.gameObject))
                    child.localEulerAngles = new Vector3(0, 0, 0);
            }
        }
    }

    private void OnDestroy()
    {
        GameManagerMap.Instance.StatusMain.OnStatusChange -= OnStatusChange;
    }

    public void DisableAll()
    {
        foreach (Transform child in _canvasToMove.transform)
        {
            if (child.gameObject != _panelHit && child.gameObject != _panelMiss)
                child.gameObject.SetActive(false);
        }
    }

    public IEnumerator PanelShow(GameObject panel, float timerBeforeDisable = 0)
    {
        yield return StartCoroutine(PanelAnimation(panel: panel, speed: 1.4f, toNumberEverything: 1, toNumberPanel: panel.GetComponent<Image>().color.a, timeFinish: 2));
        yield return new WaitForSeconds(timerBeforeDisable);
        yield return StartCoroutine(PanelAnimation(panel: panel, speed: -0.5f, toNumberEverything: 0, toNumberPanel: 0, timeFinish: 2));
        panel.SetActive(false);
    }

    public void SetStartHealth(int count)
    {
        int childCount = _panelHealthBar.transform.childCount;
        if (count > childCount)
        {
            for (int i = 0; i < count - childCount; i++)
                Instantiate(_hpPrefab, _panelHealthBar.transform);
        }
        else if (count < childCount)
        {
            for (int i = 0; i < childCount - count; i++)
                Destroy(_panelHealthBar.transform.GetChild(childCount - i - 1).gameObject);
        }
    }


    //Method than makes animathion appear or disappear panel with its children
    //If toNumberEverything = 0, it seems than panel is disappear
    //if disppear => speed < 0, if appear => speed > 0
    private IEnumerator PanelAnimation(GameObject panel, float speed, float toNumberEverything, float toNumberPanel, float timeFinish)
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
}
