using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParticleSystemFactory
{
    public class AlbedoFlowerParticle : ParticleSystemBase
    {
        public AlbedoFlowerParticle(Transform parent) : base(parent) {}

        public override string PrefabName() => "AlbedoFlower";

        public override void Create(ParticleData data)
        {

            ParticleSystem obj = _pool.Get();
            obj.transform.localPosition = data.Position;
            
            var velocityOverLifetime = obj.velocityOverLifetime;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-data.Distance, data.Distance);
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-data.Distance, data.Distance);

            EndOfCreating(obj);
        }

        public override int DefaultCapacity() => 2;
        public override int MaxCapacity() => 10;
    }

    public class SlimeJumpParticle : ParticleSystemBase
    {
        public SlimeJumpParticle(Transform parent) : base(parent) { }

        public override string PrefabName() => "SlimeJump";

        public override void Create(ParticleData data)
        {
            ParticleSystem obj = _pool.Get();
            obj.transform.localPosition = data.Position;

            var shape = obj.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(data.Distance + 2, data.Distance + 2, 0.5f);
            var emission = obj.emission;
            ParticleSystem.Burst burst = emission.GetBurst(0);
            burst.count = 100 + 50 * data.Distance;
            emission.SetBurst(0, burst);

            EndOfCreating(obj);
        }

        public override int DefaultCapacity() => 1;

        public override int MaxCapacity() => 5;
    }

    public class AlbedoUltimateParticle : ParticleSystemBaseOnlySummon
    {
        public AlbedoUltimateParticle(Transform parent) : base(parent) {}

        public override string PrefabName() => "UltimateAlbedo";

        public override int DefaultCapacity() => 1;

        public override int MaxCapacity() => 5;
    }

    public class ShootFlashParticle : ParticleSystemBaseOnlySummon
    {
        public ShootFlashParticle(Transform parent) : base(parent) { }

        public override string PrefabName() => "MuzzleFlash01";

        public override int DefaultCapacity() => 16;

        public override int MaxCapacity() => 32;
    }

    public class HitEffectParticle : ParticleSystemBaseOnlySummon
    {
        public HitEffectParticle(Transform parent) : base(parent) { }

        public override string PrefabName() => "HitEffect";

        public override int DefaultCapacity() => 16;

        public override int MaxCapacity() => 32;
    }
}
