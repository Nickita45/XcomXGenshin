using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIIcons : MonoBehaviour
{
    [Serializable]
    public class IconObject{
        public string name;
        public Sprite sprite;
    };
    public List<IconObject> iconObjects;

    public IconObject GetIconByName(string iconName)
    {
        return iconObjects.FirstOrDefault(icon => icon.name == iconName);
    } 
}
