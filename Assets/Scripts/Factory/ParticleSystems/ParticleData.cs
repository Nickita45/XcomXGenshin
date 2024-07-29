using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ParticleSystemFactory
{
    public struct ParticleData //mb in future better is class with enheriting
    {
        public int Distance { get; private set; }
        public Vector3 Position { get; private set; }

        public ParticleData(Vector3 position, int distance = 0)
        {
            Distance = distance;
            Position = position;
        }
    }

}