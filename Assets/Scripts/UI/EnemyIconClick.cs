using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EnemyIconClick : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private GameObject _enemy;
    private Image _image;

    public void OnPointerClick(PointerEventData data)
    {
        GameManagerMap.Instance.CameraController.MoveTo(_enemy.transform.position);
    }

    public void OnPointerEnter(PointerEventData data)
    {
        _image.color = _image.color.WithAlpha(0.5f);
    }

    public void OnPointerExit(PointerEventData data)
    {
        _image.color = _image.color.WithAlpha(1.0f);
    }

    public void SetEnemy(GameObject enemy)
    {
        _enemy = enemy;
    }

    // Start is called before the first frame update
    void Start()
    {
        _image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
