using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIIcons : MonoBehaviour
{
    [SerializeField]
    private Sprite[] _sprites;
    public Dictionary<string, Sprite> dictSprites = new();
    void Start()
    {
        foreach(Sprite sprite in _sprites)
        {
            //Add parser in future
            dictSprites.Add(sprite.name,sprite);
        }
    }
}
