using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitiesFactory
{
    public struct AbilitiesCreateData
    {
        public Element Element { get; private set; }
        public Unit Creator { get; private set; }

        public AbilitiesCreateData(Element element, Unit creator)
        {
            Element = element;
            Creator = creator;
        }
    }

}