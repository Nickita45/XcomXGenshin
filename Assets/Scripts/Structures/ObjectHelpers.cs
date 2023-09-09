using System;
using System.Collections;
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

    public static IEnumerator RotateTransform(Transform obj, Quaternion target, float duration)
    {
        float t = 0f;
        Quaternion origin = obj.rotation;

        while (t < duration)
        {
            obj.rotation = Quaternion.Lerp(origin, target, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        obj.rotation = target;
    }

    public static GameObject FindDescendantByName(Transform parent, string targetName)
    {
        if (parent.gameObject.name == targetName) return parent.gameObject;

        // Recursively search through the children of the current GameObject
        for (int i = 0; i < parent.childCount; i++)
        {
            GameObject foundChild = FindDescendantByName(parent.GetChild(i), targetName);
            if (foundChild != null) return foundChild;
        }

        // If not found in this subtree, return null
        return null;
    }
}
