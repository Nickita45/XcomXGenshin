using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace ParticleSystemFactory
{
    public abstract class ParticleSystemBase
    {
        private Transform _parent;
        private bool collectionCheck = true;
        
        protected float _timerLifeNonLoopingParticles = 3f; //mb in future to interface
        protected GameObject _prefab;

        protected IObjectPool<ParticleSystem> _pool;

        public ParticleSystemBase(Transform parent)
        {
            _prefab = Resources.Load<GameObject>("Prefabs/Particles/" + PrefabName());
            _parent = parent;

            _pool = new ObjectPool<ParticleSystem>(CreateParticleSystem,
                     OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject,
                     collectionCheck: collectionCheck, defaultCapacity: DefaultCapacity(), maxSize: MaxCapacity());
        }

        #region Object Pool
        private ParticleSystem CreateParticleSystem()
        {
            GameObject obj = Object.Instantiate(_prefab, _parent);
            ParticleSystem particleSystem = obj.GetComponent<ParticleSystem>();

            return particleSystem;
        }
        private void OnGetFromPool(ParticleSystem pooledObject)
        {
            pooledObject.gameObject.SetActive(true);
        }
        private void OnReleaseToPool(ParticleSystem pooledObject)
        {
            pooledObject.gameObject.SetActive(false);
        }
        private void OnDestroyPooledObject(ParticleSystem pooledObject)
        {
            Object.Destroy(pooledObject.gameObject);
        }
        #endregion

        protected void EndOfCreating(ParticleSystem particleSystem)
        {
            particleSystem.Play();
            Manager.Instance.StartCoroutine(ReturnToPoolAfterLifetime(particleSystem, _timerLifeNonLoopingParticles)); //maybe remake to Task
        }

        protected virtual IEnumerator ReturnToPoolAfterLifetime(ParticleSystem particleSystem, float lifetime)
        {
            yield return new WaitForSeconds(lifetime);
            particleSystem.Stop();
            _pool.Release(particleSystem);
        }

        public abstract string PrefabName();
        public abstract int DefaultCapacity();
        public abstract int MaxCapacity();
        public abstract void Create(ParticleData data);
    }

   
}