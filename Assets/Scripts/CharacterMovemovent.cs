using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterMovemovent : MonoBehaviour
{
    public static readonly Vector3 BIGVECTOR = new Vector3(int.MaxValue, int.MaxValue, int.MaxValue);

    [Header("Consts Test")]
    [SerializeField]
    private int _countMove = 5;
    [SerializeField]
    private float _speed = 1f;


    [Header("Script Objects")]
    [SerializeField]
    private GameObject _prefabPossibleTerritory;

    private CharacterInfo _selectedCharacter;

    //private HashSet<TerritroyReaded> _objectsCalculated;
    private Dictionary<TerritroyReaded, TerritroyReaded> _objectsCalculated; //orig, previous

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
                if (_objectsCalculated.Keys.Contains(detectTerritory) && detectTerritory.TerritoryInfo != TerritoryType.Shelter && detectTerritory.IndexDown.Where(n => GameManagerMap.Instance.Map[n].TerritoryInfo == TerritoryType.Ground).Count() == 1)
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

        foreach (var item in _objectsCalculated.Keys)
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
        calculatePoints(newTerritory);
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

            elapsedTime += Time.deltaTime * _speed; // Увеличиваем время в соответствии с прошедшим временем

            yield return null; // Ждем следующего кадра
        }

        // Убедиться, что объект точно достиг целевой позиции
        _selectedCharacter.gameObject.transform.localPosition = target;
    }

    public List<Vector3> calculatePoints(TerritroyReaded starter)
    {
        List<Vector3> points = new List<Vector3>();
        TerritroyReaded aktual = starter;
        //Debug.Log($"{starter.GetCordinats()} a {_selectedCharacter.ActualTerritory.GetCordinats()}: {(starter.GetCordinats() - _selectedCharacter.ActualTerritory.GetCordinats())}");
        Vector3 sub = CharacterMovemovent.BIGVECTOR;
        string debug = string.Empty;
        while(true)
        {
            var next = _objectsCalculated[aktual];
            if (next == null)
                break;
            
            var newSub = aktual.GetCordinats() - next.GetCordinats();
            if(sub != CharacterMovemovent.BIGVECTOR)
            {
                var ifSub = sub - newSub;
                if (!((ifSub.x == 0f && ifSub.y == 0f) || (ifSub.x == 0f && ifSub.z == 0f) || (ifSub.y == 0f && ifSub.z == 0f)))
                {
                    debug += aktual.Index + "\n";
                    //if()
                }
            }
            sub = newSub;
                
            aktual = next;

        }
        Debug.Log(debug);

       /* while(aktual != _selectedCharacter.ActualTerritory)
        {
            bool newPoint = false;
            var sub = starter.GetCordinats() - _selectedCharacter.ActualTerritory.GetCordinats();
            TerritroyReaded newAktual = aktual;
            if (sub.x < 0)
            {
                newAktual = GameManagerMap.Instance.Map[aktual.IndexBottom.First()];
            } else if(sub.x > 0)
            {
                newAktual = GameManagerMap.Instance.Map[aktual.IndexFront.First()];
            }

            if (!_objectsCalculated.Contains(newAktual))
                newPoint = true;


            if (sub.y < 0)
            {
                newAktual = GameManagerMap.Instance.Map[aktual.IndexLeft.First()];
            }
            else if(sub.y > 0)
            {
                newAktual = GameManagerMap.Instance.Map[aktual.IndexRight.First()];
            }

            if (!_objectsCalculated.Contains(newAktual))
            {
                if(newPoint == false)
                {

                    newPoint = true;
                }
                else
                {

                }
            }
        }
       */
        return points;
    }


    public Dictionary<TerritroyReaded, TerritroyReaded> CalculateAllPossible()
    {
        Dictionary<TerritroyReaded, TerritroyReaded> objectsCalculated = new Dictionary<TerritroyReaded, TerritroyReaded>();
        
        Stack<(TerritroyReaded orig, TerritroyReaded previus)> notCalculatedYet = new Stack<(TerritroyReaded orig, TerritroyReaded previus)>();
        Stack<(TerritroyReaded orig, TerritroyReaded previus)> nextCalculated = new Stack<(TerritroyReaded orig, TerritroyReaded previus)>();

        // actual = SelectedCharacter.ActualTerritory;
        nextCalculated.Push((_selectedCharacter.ActualTerritory, null));

        for (int i = 0; i <= _countMove;i++)
        {
            while (nextCalculated.Count > 0)
            {
                (TerritroyReaded orig, TerritroyReaded previus) actual = nextCalculated.Pop();
                if (!objectsCalculated.Keys.Contains(actual.orig))
                {
                    objectsCalculated.Add(actual.orig, actual.previus);
                }

                foreach (var item in actual.orig)
                {
                    var detectItem = GameManagerMap.Instance.Map[item];
                    if (detectItem.TerritoryInfo == TerritoryType.Shelter)
                        continue;
                    
                    notCalculatedYet.Push((detectItem, actual.orig));
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
