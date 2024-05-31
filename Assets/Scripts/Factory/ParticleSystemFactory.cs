using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemFactory : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField]
    private float _timerLifeNonLoopingParticles = 3f;


    [Header("Particles")]
    [SerializeField]
    private GameObject _albedoFlower;
    [SerializeField]
    private GameObject _slimeJump;
    [SerializeField]
    private GameObject _albedoUltimate;

    public void CreateAlbedoFlower(int distance, Vector3 position)
    {
        GameObject obj = Instantiate(_albedoFlower, position, _albedoFlower.transform.rotation, Manager.MainParent.transform);
        var velocityOverLifetime = obj.GetComponent<ParticleSystem>().velocityOverLifetime;
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-distance, distance);
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-distance, distance);
        Destroy(obj, _timerLifeNonLoopingParticles);
    }

    public void CreateSlimeJump(int distance, Vector3 position)
    {
        GameObject obj = Instantiate(_slimeJump, position, _slimeJump.transform.rotation, Manager.MainParent.transform);
        var shape = obj.GetComponentInChildren<ParticleSystem>().shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(distance + 2, distance + 2, 0.5f);
        var emission = obj.GetComponentInChildren<ParticleSystem>().emission;
        ParticleSystem.Burst burst = emission.GetBurst(0);
        burst.count = 100 + 50 * distance;
        emission.SetBurst(0, burst);
        Destroy(obj, _timerLifeNonLoopingParticles);
    }

    public void CreateAlbedoUltimate(Vector3 position) //in future add distance
    {
        GameObject obj = Instantiate(_albedoUltimate, position, _slimeJump.transform.rotation, Manager.MainParent.transform);
        Destroy(obj, _timerLifeNonLoopingParticles);
    }

}
