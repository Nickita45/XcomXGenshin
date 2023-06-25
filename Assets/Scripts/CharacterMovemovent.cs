using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterMovemovent : MonoBehaviour
{
    private const int COUNTMOVE = 5;
    private const float SPEED = 1f;


    [SerializeField]
    private GameObject _prefabPossibleTerritory;

    private CharacterInfo _selectedCharacter;

    private HashSet<TerritroyReaded> _objectsCalculated;

    public void Update()
    {
        if (_selectedCharacter != null && _selectedCharacter.MoverActive())
        {
            SpawnMover();
        }


        if(Input.GetKey(KeyCode.K))
        {
            var test = CalculateAllPossible();
            Debug.Log(test.Count);
           
        }
    }

    private void SpawnMover()
    {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 cordinats = hit.point - GameManagerMap.Instance.MainParent.transform.position;
            cordinats = new Vector3(Mathf.Floor(cordinats.x), Mathf.Floor(cordinats.y), Mathf.Floor(cordinats.z));


            TerritroyReaded detectTerritory;
            if (GameManagerMap.Instance.Map.ContainsVertexByPox(cordinats, out detectTerritory) ||
                GameManagerMap.Instance.Map.ContainsVertexByPox(cordinats + new Vector3(0.5f, 0.5f, 0.5f), out detectTerritory))
            {
                if (_objectsCalculated.Contains(detectTerritory) && detectTerritory.TerritoryInfo != TerritoryType.Shelter && detectTerritory.IndexDown.Where(n => GameManagerMap.Instance.Map[n].TerritoryInfo == TerritoryType.Ground).Count() == 1)
                {
                    _selectedCharacter.SetCordintasToMover(detectTerritory.GetCordinats() + GameManagerMap.Instance.MainParent.transform.position - new Vector3(0, 0.5f, 0));

                    if (Input.GetMouseButton(0) && !_selectedCharacter.isAktualTerritory(detectTerritory))
                    {
                        StartCoroutine(CoroutineNewPositionCharacter(detectTerritory));
                        
                    }
                }
            }
           
        }
    }

    public void CharacterSelect(CharacterInfo character)
    {
        if(_selectedCharacter != null && _selectedCharacter != character)
        {
            _selectedCharacter.OnDeselected();
        }
        _selectedCharacter = character;
        _objectsCalculated = CalculateAllPossible();

        foreach (var item in _objectsCalculated)
        {
            var obj = Instantiate(_prefabPossibleTerritory, GameManagerMap.Instance.GenereteTerritoryMove.transform);
            obj.transform.localPosition = item.GetCordinats() - new Vector3(0, 0.5f, 0);
        }
    }

    public void CharacterDeselect()
    {

        _selectedCharacter = null;

        _objectsCalculated.Clear();
        foreach (Transform item in GameManagerMap.Instance.GenereteTerritoryMove.transform)
        {
            Destroy(item.gameObject);   
        }
    }


    private IEnumerator CoroutineNewPositionCharacter(TerritroyReaded newTerritory)
    {
        _selectedCharacter.MoverActive(false);
        yield return StartCoroutine(CoroutineMove(newTerritory.GetCordinats()));
        _selectedCharacter.ActualTerritory = newTerritory;

        var save = _selectedCharacter;
        _selectedCharacter.OnDeselected();
        save.OnSelected(save);
    }

    private IEnumerator CoroutineMove(Vector3 target)
    {
        Vector3 startPosition = _selectedCharacter.gameObject.transform.localPosition; // Начальная позиция объекта

        float elapsedTime = 0f; // Время, прошедшее с начала перемещения

        while (elapsedTime < 1f)
        {
            // Интерполируем позицию объекта между начальной и целевой позицией
            _selectedCharacter.gameObject.transform.localPosition = Vector3.Lerp(startPosition, target, elapsedTime);

            elapsedTime += Time.deltaTime * SPEED; // Увеличиваем время в соответствии с прошедшим временем

            yield return null; // Ждем следующего кадра
        }

        // Убедиться, что объект точно достиг целевой позиции
        _selectedCharacter.gameObject.transform.localPosition = target;
    }

    public HashSet<TerritroyReaded> CalculateAllPossible()
    {
        HashSet<TerritroyReaded> objectsCalculated = new HashSet<TerritroyReaded>();
        
        Stack<TerritroyReaded> notCalculatedYet = new Stack<TerritroyReaded>();
        Stack<TerritroyReaded> nextCalculated = new Stack<TerritroyReaded>();

        // actual = SelectedCharacter.ActualTerritory;
        nextCalculated.Push(_selectedCharacter.ActualTerritory);

        for (int i = 0; i <= COUNTMOVE;i++)
        {
            while (nextCalculated.Count > 0)
            {
                TerritroyReaded actual = nextCalculated.Pop();
                if (!objectsCalculated.Contains(actual))
                {
                    objectsCalculated.Add(actual);
                }

                foreach (var item in actual)
                {
                    var detectItem = GameManagerMap.Instance.Map[item];
                    if (detectItem.TerritoryInfo == TerritoryType.Shelter)
                        continue;
                    
                    notCalculatedYet.Push(detectItem);
                }
            }

            while(notCalculatedYet.Count > 0)
            {
                nextCalculated.Push(notCalculatedYet.Pop());
            }
        }
        return objectsCalculated;
    }
}
