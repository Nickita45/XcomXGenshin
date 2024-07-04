using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ParticleSystemFactory
{
    public struct ParticleData
    {
        public int Distance { get; private set; }
        public Vector3 Position { get; private set; }
        public Transform Parent { get; private set; }

        public ParticleData(Vector3 position, Transform parent,int distance = 0)
        {
            Distance = distance;
            Position = position;
            Parent = parent;
        }
    }

}