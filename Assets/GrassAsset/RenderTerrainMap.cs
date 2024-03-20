using UnityEngine;

[ExecuteInEditMode]
public class RenderTerrainMap : MonoBehaviour
{
    public Camera camToDrawWith;
    // layer to render
    [SerializeField]
    LayerMask layer;

    // objects to render
    [SerializeField]
    Renderer[] renderers;
    // unity terrain to render
    [SerializeField]
    Terrain[] terrains;
    // map resolution
    public int resolution = 512;

    // padding the total size
    public float adjustScaling = 2.5f;
    [SerializeField]
    bool RealTimeDiffuse;
    RenderTexture tempTex;

    public float repeatRate = 5f;
    private Bounds bounds;

    void GetBounds()
    {

        if (renderers.Length > 0)
        {
            foreach (Renderer renderer in renderers)
            {
                if (bounds.size.magnitude < 0.1f)
                {
                    bounds = new Bounds(renderer.transform.position, Vector3.zero);
                }
                bounds.Encapsulate(renderer.bounds);
            }
        }

        if (terrains.Length > 0)
        {
            foreach (Terrain terrain in terrains)
            {
                if (bounds.size.magnitude < 0.1f)
                {
                    bounds = new Bounds(terrain.transform.position, Vector3.zero);
                }
                Vector3 terrainCenter = terrain.GetPosition() + terrain.terrainData.bounds.center;
                Bounds worldBounds = new Bounds(terrainCenter, terrain.terrainData.bounds.size);
                bounds.Encapsulate(worldBounds);
            }
        }
    }

    void OnEnable()
    {
        // reset bounds
        bounds = new Bounds(transform.position, Vector3.zero);
        tempTex = new RenderTexture(resolution, resolution, 24);
        GetBounds();
        SetUpCam();
        DrawDiffuseMap();
    }


    void Start()
    {
        GetBounds();
        SetUpCam();
        DrawDiffuseMap();
        if (RealTimeDiffuse)
        {
            InvokeRepeating("UpdateTex", 1f, repeatRate);
        }

    }


    void UpdateTex()
    {
        camToDrawWith.enabled = true;
        camToDrawWith.targetTexture = tempTex;
        Shader.SetGlobalTexture("_TerrainDiffuse", tempTex);
    }
    public void DrawDiffuseMap()
    {
        DrawToMap("_TerrainDiffuse");
    }

    void DrawToMap(string target)
    {
        camToDrawWith.enabled = true;
        camToDrawWith.targetTexture = tempTex;
        camToDrawWith.depthTextureMode = DepthTextureMode.Depth;

        Shader.SetGlobalFloat("_OrthographicCamSizeTerrain", camToDrawWith.orthographicSize);
        Shader.SetGlobalVector("_OrthographicCamPosTerrain", camToDrawWith.transform.position);
        camToDrawWith.Render();
        Shader.SetGlobalTexture(target, tempTex);

        camToDrawWith.enabled = false;
    }

    void SetUpCam()
    {
        if (camToDrawWith == null)
        {
            camToDrawWith = GetComponentInChildren<Camera>();
        }
        float size = bounds.size.magnitude;
        camToDrawWith.cullingMask = layer;
        camToDrawWith.orthographicSize = size / adjustScaling;
        camToDrawWith.transform.parent = null;
        camToDrawWith.transform.position = bounds.center + new Vector3(0, bounds.extents.y + 5f, 0);
        camToDrawWith.transform.parent = gameObject.transform;
    }

}