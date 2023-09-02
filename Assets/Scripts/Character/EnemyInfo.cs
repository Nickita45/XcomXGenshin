using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInfo : MonoBehaviour //in future will be connect with CharacterInfo
{
    [SerializeField]
    private int _countHp = 5;

    private EnemyCanvasController _canvasController;

    public TerritroyReaded ActualTerritory { get; set; }

    private void Start()
    {
        _canvasController = GetComponent<EnemyCanvasController>();
        _canvasController.SetStartHealth(_countHp);


        ActualTerritory = GameManagerMap.Instance.Map[transform.localPosition];
        ActualTerritory.TerritoryInfo = TerritoryType.Character;

    }

    public void MakeHit(int hit)
    {
        _countHp -= hit;
        if (_countHp <= 0)
            KillEnemy();
        else
            _canvasController.SetStartHealth(_countHp);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            KillEnemy();
    }

    private void KillEnemy()
    {
        GameManagerMap.Instance.Map.Enemy.Remove(gameObject);
        _canvasController.DisableAll();
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<BoxCollider>().enabled = false;
        ActualTerritory.TerritoryInfo = TerritoryType.Air;
    }
}
