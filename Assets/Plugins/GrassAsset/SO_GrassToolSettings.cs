using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Grass Tool Settings", menuName = "Utility/GrassToolSettings")]
public class SO_GrassToolSettings : ScriptableObject
{
    public enum VertexColorSetting { None, Red, Blue, Green };


    [Header("Terrain Layer Settings")]
    public float[] layerBlocking;
    public bool[] layerFading;

    [Header("Vertex Color Settings")]
    public VertexColorSetting VertexColorSettings;
    public VertexColorSetting VertexFade;

    // length/width

    public float sizeWidth = 1f;

    public float sizeLength = 1f;

    // length/width adjustments

    public float adjustWidth = 0f;

    public float adjustLength = 0f;

    public float adjustWidthMax = 2f;
    public float adjustHeightMax = 2f;

    // reproject settings

    public float reprojectOffset = 1f;

    // color settings

    public float rangeR, rangeG, rangeB;

    public Color AdjustedColor = Color.white;

    // brush settings

    public LayerMask paintBlockMask = 0;

    public LayerMask hitMask = 1;

    public LayerMask paintMask = 1;
    public float brushSize = 4f;

    public float brushFalloffSize = 0.8f;

    public float Flow;

    public float density = 1f;

    public float normalLimit = 1;

    public int grassAmountToGenerate = 100000;

    public float generationDensity = 1;


    public void CreateNewLayers()
    {
        Debug.Log("Setting up initial tool settings");
        layerBlocking = new float[8];
        for (int i = 0; i < layerBlocking.Length; i++)
        {
            layerBlocking[i] = 1;
        }
        layerFading = new bool[8];
        layerFading[0] = true;
    }
}
