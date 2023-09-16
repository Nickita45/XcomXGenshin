using System;
using UnityEngine;

public static class CameraHelpers
{
    // Find the best position and rotation for the camera when choosing the enemy target.
    public static (Vector3, Quaternion) CalculateEnemyView(GameObject character, GameObject enemy)
    {
        Vector3 position = character.transform.position + (character.transform.position - enemy.transform.position).normalized * 3 + Vector3.up * 1.5f;
        Quaternion rotation = Quaternion.LookRotation(enemy.transform.position - position);
        return (position, rotation);
    }

    public static Vector3 CalculateCameraLookAt(GameObject obj, Camera camera)
    {
        return obj.transform.position - camera.transform.forward * 3 * Mathf.Sqrt(2);
    }
}
