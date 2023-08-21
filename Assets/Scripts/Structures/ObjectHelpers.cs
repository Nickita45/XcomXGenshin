using System;
using UnityEngine;

public static class ObjectHelpers
{
    public static void DestroyAllChildren(GameObject parent)
    {
        int childCount = parent.transform.childCount;

        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = parent.transform.GetChild(i);
            UnityEngine.Object.Destroy(child.gameObject);
        }
    }

    public static void DestroyAllChildrenImmediate(GameObject parent)
    {
        int childCount = parent.transform.childCount;

        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = parent.transform.GetChild(i);
            UnityEngine.Object.DestroyImmediate(child.gameObject);
        }
    }
}
