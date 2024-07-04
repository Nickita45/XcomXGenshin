using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace ParticleSystemFactory
{
    public abstract class ParticleSystemBase
    {
        protected float _timerLifeNonLoopingParticles = 3f; //mb in future to interface
        protected GameObject _prefab;

        public ParticleSystemBase ()
        {
            _prefab = Resources.Load<GameObject>("Prefabs/Particles/" + PrefabName());
        }

        public abstract string PrefabName();
        public abstract GameObject Create(ParticleData data);
    }

    public class AlbedoFlowerParticle : ParticleSystemBase
    {
        public override string PrefabName() => "AlbedoFlower";

        public override GameObject Create(ParticleData data)
        {
            GameObject obj = Object.Instantiate(_prefab, data.Position, _prefab.transform.rotation, data.Parent);
            var velocityOverLifetime = obj.GetComponent<ParticleSystem>().velocityOverLifetime;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-data.Distance, data.Distance);
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-data.Distance, data.Distance);
            Object.Destroy(obj, _timerLifeNonLoopingParticles);
            return obj;
        }
    }

    public class SlimeJumpParticle : ParticleSystemBase
    {
        public override string PrefabName() => "SlimeJump";

        public override GameObject Create(ParticleData data)
        {
            GameObject obj = Object.Instantiate(_prefab, data.Position, _prefab.transform.rotation, data.Parent);
            var shape = obj.GetComponentInChildren<ParticleSystem>().shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(data.Distance + 2, data.Distance + 2, 0.5f);
            var emission = obj.GetComponentInChildren<ParticleSystem>().emission;
            ParticleSystem.Burst burst = emission.GetBurst(0);
            burst.count = 100 + 50 * data.Distance;
            emission.SetBurst(0, burst);
            Object.Destroy(obj, _timerLifeNonLoopingParticles);
            return obj;
        }
    }

    public class AlbedoUltimateParticle : ParticleSystemBase
    {
        public override string PrefabName() => "UltimateAlbedo";

        public override GameObject Create(ParticleData data)
        {
            GameObject obj = Object.Instantiate(_prefab, data.Position, _prefab.transform.rotation, data.Parent);
            Object.Destroy(obj, _timerLifeNonLoopingParticles);
            return obj;
        }
    }
}

/*
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
*/