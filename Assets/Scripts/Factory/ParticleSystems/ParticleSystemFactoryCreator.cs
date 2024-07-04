using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParticleSystemFactory
{
    public enum ParticleType
    {
        AlbedoFlower,
        SlimeJump,
        AlbedoUltimate
    }

    public static class ParticleSystemFactoryCreator
    {
        private static Dictionary<ParticleType, ParticleSystemBase> _particles = new Dictionary<ParticleType, ParticleSystemBase>()
        {
            { ParticleType.AlbedoFlower, new AlbedoFlowerParticle()}
        };

        public static GameObject CreateParticle(ParticleType type, ParticleData data) => _particles[type].Create(data);
    }
}