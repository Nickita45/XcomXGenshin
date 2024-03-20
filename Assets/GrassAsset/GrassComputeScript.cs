// @Minionsart version
// credits  to  forkercat https://gist.github.com/junhaowww/fb6c030c17fe1e109a34f1c92571943f
// and  NedMakesGames https://gist.github.com/NedMakesGames/3e67fabe49e2e3363a657ef8a6a09838
// for the base setup for compute shaders
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class GrassComputeScript : MonoBehaviour
{
    // very slow, but will update always
    public bool autoUpdate;

    // main camera
    private Camera m_MainCamera;

    // grass settings to send to the compute shader
    public SO_GrassSettings currentPresets;

    // interactors
    ShaderInteractor[] interactors;

    // base data lists
    [SerializeField, HideInInspector]
    List<GrassData> grassData = new List<GrassData>();

    // list of all visible grass ids, rest are culled
    List<int> grassVisibleIDList = new List<int>();

    // A state variable to help keep track of whether compute buffers have been set up
    private bool m_Initialized;
    // A compute buffer to hold vertex data of the source mesh
    private ComputeBuffer m_SourceVertBuffer;
    // A compute buffer to hold vertex data of the generated mesh
    private ComputeBuffer m_DrawBuffer;
    // A compute buffer to hold indirect draw arguments
    private ComputeBuffer m_ArgsBuffer;
    // Instantiate the shaders so data belong to their unique compute buffers
    private ComputeShader m_InstantiatedComputeShader;
    // buffer that contains the ids of all visible instances
    private ComputeBuffer m_VisibleIDBuffer;
    [SerializeField] Material m_InstantiatedMaterial;
    // The id of the kernel in the grass compute shader
    private int m_IdGrassKernel;
    // The x dispatch size for the grass compute shader
    private int m_DispatchSize;
    // compute shader thread group size
    uint threadGroupSize;

    // The size of one entry in the various compute buffers, size comes from the float3/float2 entrees in the shader
    private const int SOURCE_VERT_STRIDE = sizeof(float) * (3 + 3 + 2 + 3);
    private const int DRAW_STRIDE = sizeof(float) * (3 + 3 + ((3 + 2) * 3));

    // bounds of the total grass 
    Bounds bounds;


    private uint[] argsBufferReset = new uint[5]
   {
        0,  // Number of vertices to render (Calculated in the compute shader with "InterlockedAdd(_IndirectArgsBuffer[0].numVertices);")
        1,  // Number of instances to render (should only be 1 instance since it should produce a single mesh)
        0,  // Index of the first vertex to render
        0,  // Index of the first instance to render
        0   // Not used
   };

    // culling tree data ----------------------------------------------------------------------
    CullingTreeNode cullingTree;
    List<Bounds> BoundsListVis = new List<Bounds>();
    List<CullingTreeNode> leaves = new List<CullingTreeNode>();
    Plane[] cameraFrustumPlanes = new Plane[6];
    float cameraOriginalFarPlane;

    // list of -1 to overwrite the grassvisible buffer with
    List<int> empty = new List<int>();

    // speeding up the editor a bit
    Vector3 m_cachedCamPos;
    Quaternion m_cachedCamRot;
    bool m_fastMode;
    int shaderID;

    // max buffer size can depend on platform and your draw stride, you may have to change it
    int maxBufferSize = 2500000;

    ///-------------------------------------------------------------------------------------

    public List<GrassData> SetGrassPaintedDataList
    {
        get { return grassData; }
        set { grassData = value; }
    }

#if UNITY_EDITOR
    SceneView view;

    void OnDestroy()
    {
        // When the window is destroyed, remove the delegate
        // so that it will no longer do any drawing.
        SceneView.duringSceneGui -= this.OnScene;
    }

    void OnScene(SceneView scene)
    {
        view = scene;
        if (!Application.isPlaying)
        {
            if (view.camera != null)
            {
                m_MainCamera = view.camera;
            }
        }
        else
        {
            m_MainCamera = Camera.main;
        }
    }

    private void OnValidate()
    {
        // Set up components
        if (!Application.isPlaying)
        {
            if (view != null)
            {
                m_MainCamera = view.camera;
            }
        }
        else
        {
            m_MainCamera = Camera.main;
        }
    }
#endif



    private void OnEnable()
    {
        // If initialized, call on disable to clean things up
        if (m_Initialized)
        {
            OnDisable();
        }

        MainSetup(true);
    }

    void MainSetup(bool full)
    {
#if UNITY_EDITOR

        SceneView.duringSceneGui += this.OnScene;
        if (!Application.isPlaying)
        {
            if (view != null)
            {
                m_MainCamera = view.camera;
            }

        }
#endif
        if (Application.isPlaying)
        {
            m_MainCamera = Camera.main;
        }

        // Don't do anything if resources are not found,
        // or no vertex is put on the mesh.
        if (grassData.Count == 0)
        {
            return;
        }

        if (currentPresets.shaderToUse == null || currentPresets.materialToUse == null)
        {
            Debug.LogWarning("Missing Compute Shader/Material in grass Settings", this);
            return;
        }

        // empty array to replace the visible grass with
        PopulateEmptyList(grassData.Count);
        m_Initialized = true;

        // Instantiate the shaders so they can point to their own buffers
        m_InstantiatedComputeShader = Instantiate(currentPresets.shaderToUse);
        m_InstantiatedMaterial = Instantiate(currentPresets.materialToUse);

        int numSourceVertices = grassData.Count;

        // amount of segmets
        int maxBladesPerVertex = Mathf.Max(1, currentPresets.allowedBladesPerVertex);
        int maxSegmentsPerBlade = Mathf.Max(1, currentPresets.allowedSegmentsPerBlade);
        // -1 is because the top part of the grass only has 1 triangle
        int maxBladeTriangles = maxBladesPerVertex * ((maxSegmentsPerBlade - 1) * 2 + 1);

        // Create compute buffers
        // The stride is the size, in bytes, each object in the buffer takes up
        m_SourceVertBuffer = new ComputeBuffer(numSourceVertices, SOURCE_VERT_STRIDE, ComputeBufferType.Structured, ComputeBufferMode.Immutable);
        m_SourceVertBuffer.SetData(grassData);


        m_DrawBuffer = new ComputeBuffer(maxBufferSize, DRAW_STRIDE, ComputeBufferType.Append);

        m_ArgsBuffer = new ComputeBuffer(1, argsBufferReset.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

        m_VisibleIDBuffer = new ComputeBuffer(grassData.Count, sizeof(int), ComputeBufferType.Structured); //uint only, per visible grass

        // Cache the kernel IDs we will be dispatching
        m_IdGrassKernel = m_InstantiatedComputeShader.FindKernel("Main");

        // Set buffer data
        m_InstantiatedComputeShader.SetBuffer(m_IdGrassKernel, "_SourceVertices",
            m_SourceVertBuffer);
        m_InstantiatedComputeShader.SetBuffer(m_IdGrassKernel, "_DrawTriangles", m_DrawBuffer);
        m_InstantiatedComputeShader.SetBuffer(m_IdGrassKernel, "_IndirectArgsBuffer", m_ArgsBuffer);
        m_InstantiatedComputeShader.SetBuffer(m_IdGrassKernel, "_VisibleIDBuffer", m_VisibleIDBuffer);
        m_InstantiatedMaterial.SetBuffer("_DrawTriangles", m_DrawBuffer);
        // Set vertex data
        m_InstantiatedComputeShader.SetInt("_NumSourceVertices", numSourceVertices);
        // cache shader property to int id for interactivity;
        shaderID = Shader.PropertyToID("_PositionsMoving");

        // Calculate the number of threads to use. Get the thread size from the kernel
        // Then, divide the number of triangles by that size
        m_InstantiatedComputeShader.GetKernelThreadGroupSizes(m_IdGrassKernel,
            out threadGroupSize, out _, out _);
        //set once only
        m_DispatchSize = Mathf.CeilToInt(grassData.Count / threadGroupSize);
        SetGrassDataBase(full);

        if (full)
        {

            UpdateBounds();

        }
        SetupQuadTree(full);
    }

    void UpdateBounds()
    {
        // Get the bounds of all the grass points and then expand
        bounds = new Bounds(grassData[0].position, Vector3.one);

        for (int i = 0; i < grassData.Count; i++)
        {
            Vector3 target = grassData[i].position;

            bounds.Encapsulate(target);
        }
    }

    void SetupQuadTree(bool full)
    {
        if (full)
        {

            cullingTree = new CullingTreeNode(bounds, currentPresets.cullingTreeDepth);

            cullingTree.RetrieveAllLeaves(leaves);
            //add the id of each grass point into the right cullingtree
            for (int i = 0; i < grassData.Count; i++)
            {
                cullingTree.FindLeaf(grassData[i].position, i);
            }
            cullingTree.ClearEmpty();
        }
        else
        {
            // just make everything visible while editing grass
            GrassFastList(grassData.Count);
            m_VisibleIDBuffer.SetData(grassVisibleIDList);
        }
    }

    void GrassFastList(int count)
    {
        grassVisibleIDList = Enumerable.Range(0, count).ToArray().ToList();
    }

    void PopulateEmptyList(int count)
    {
        empty = new List<int>(count);
        empty.InsertRange(0, Enumerable.Repeat(-1, count));
    }

    void GetFrustumData()
    {
        if (m_MainCamera == null)
        {
            return;
        }
        // if the camera didnt move, we dont need to change the culling;
        if (m_cachedCamRot == m_MainCamera.transform.rotation && m_cachedCamPos == m_MainCamera.transform.position && Application.isPlaying)
        {
            return;
        }
        // get frustum data from the main camera
        cameraOriginalFarPlane = m_MainCamera.farClipPlane;
        m_MainCamera.farClipPlane = currentPresets.maxDrawDistance;//allow drawDistance control    
        GeometryUtility.CalculateFrustumPlanes(m_MainCamera, cameraFrustumPlanes);
        m_MainCamera.farClipPlane = cameraOriginalFarPlane;//revert far plane edit

        if (!m_fastMode)
        {
            BoundsListVis.Clear();
            m_VisibleIDBuffer.SetData(empty);
            grassVisibleIDList.Clear();
            cullingTree.RetrieveLeaves(cameraFrustumPlanes, BoundsListVis, grassVisibleIDList);
            m_VisibleIDBuffer.SetData(grassVisibleIDList);
        }

        // cache camera position to skip culling when not moved
        m_cachedCamPos = m_MainCamera.transform.position;
        m_cachedCamRot = m_MainCamera.transform.rotation;
    }

    private void OnDisable()
    {
        // Dispose of buffers and copied shaders here
        if (m_Initialized)
        {
            // If the application is not in play mode, we have to call DestroyImmediate
            if (Application.isPlaying)
            {
                Destroy(m_InstantiatedComputeShader);
                Destroy(m_InstantiatedMaterial);
            }
            else
            {
                DestroyImmediate(m_InstantiatedComputeShader);
                DestroyImmediate(m_InstantiatedMaterial);
            }
            // Release each buffer
            m_SourceVertBuffer?.Release();
            m_DrawBuffer?.Release();
            m_ArgsBuffer?.Release();
            m_VisibleIDBuffer?.Release();
        }
        m_Initialized = false;
    }

    // LateUpdate is called after all Update calls
    private void Update()
    {
        // If in edit mode, we need to update the shaders each Update to make sure settings changes are applied
        // Don't worry, in edit mode, Update isn't called each frame
        if (!Application.isPlaying && autoUpdate && !m_fastMode)
        {
            OnDisable();
            OnEnable();
        }

        // If not initialized, do nothing (creating zero-length buffer will crash)
        if (!m_Initialized)
        {
            // Initialization is not done, please check if there are null components
            // or just because there is not vertex being painted.
            return;
        }
        // get the data from the camera for culling
        GetFrustumData();
        // Update the shader with frame specific data
        SetGrassDataUpdate();
        // Clear the draw and indirect args buffers of last frame's data
        m_DrawBuffer.SetCounterValue(0);
        m_ArgsBuffer.SetData(argsBufferReset);
        m_DispatchSize = Mathf.CeilToInt(grassVisibleIDList.Count / threadGroupSize);
        if (grassVisibleIDList.Count > 0)
        {
            // make sure the compute shader is dispatched even when theres very little grass
            m_DispatchSize += 1;
        }
        if (m_DispatchSize > 0)
        {
            // Dispatch the grass shader. It will run on the GPU
            m_InstantiatedComputeShader.Dispatch(m_IdGrassKernel, m_DispatchSize, 1, 1);
            // DrawProceduralIndirect queues a draw call up for our generated mesh
            Graphics.DrawProceduralIndirect(m_InstantiatedMaterial, bounds, MeshTopology.Triangles,
            m_ArgsBuffer, 0, null, null, currentPresets.castShadow, true, gameObject.layer);
        }
    }

    private void SetGrassDataBase(bool full)
    {
        // Send things to compute shader that dont need to be set every frame
        m_InstantiatedComputeShader.SetFloat("_Time", Time.time);
        m_InstantiatedComputeShader.SetFloat("_GrassRandomHeightMin", currentPresets.grassRandomHeightMin);
        m_InstantiatedComputeShader.SetFloat("_GrassRandomHeightMax", currentPresets.grassRandomHeightMax);
        m_InstantiatedComputeShader.SetFloat("_WindSpeed", currentPresets.windSpeed);
        m_InstantiatedComputeShader.SetFloat("_WindStrength", currentPresets.windStrength);


        if (full)
        {
            m_InstantiatedComputeShader.SetFloat("_MinFadeDist", currentPresets.minFadeDistance);
            m_InstantiatedComputeShader.SetFloat("_MaxFadeDist", currentPresets.maxDrawDistance);
            interactors = (ShaderInteractor[])FindObjectsOfType(typeof(ShaderInteractor));
        }
        else
        {
            // if theres a lot of grass, just cull earlier so we can still see what we're paiting, otherwise it will be invisible
            if (grassData.Count > 200000)
            {
                m_InstantiatedComputeShader.SetFloat("_MinFadeDist", 40f);
                m_InstantiatedComputeShader.SetFloat("_MaxFadeDist", 50f);
            }
            else
            {
                m_InstantiatedComputeShader.SetFloat("_MinFadeDist", currentPresets.minFadeDistance);
                m_InstantiatedComputeShader.SetFloat("_MaxFadeDist", currentPresets.maxDrawDistance);
            }

        }
        m_InstantiatedComputeShader.SetFloat("_InteractorStrength", currentPresets.affectStrength);
        m_InstantiatedComputeShader.SetFloat("_BladeRadius", currentPresets.bladeRadius);
        m_InstantiatedComputeShader.SetFloat("_BladeForward", currentPresets.bladeForwardAmount);
        m_InstantiatedComputeShader.SetFloat("_BladeCurve", Mathf.Max(0, currentPresets.bladeCurveAmount));
        m_InstantiatedComputeShader.SetFloat("_BottomWidth", currentPresets.bottomWidth);



        m_InstantiatedComputeShader.SetInt("_MaxBladesPerVertex", currentPresets.allowedBladesPerVertex);
        m_InstantiatedComputeShader.SetInt("_MaxSegmentsPerBlade", currentPresets.allowedSegmentsPerBlade);

        m_InstantiatedComputeShader.SetFloat("_MinHeight", currentPresets.MinHeight);
        m_InstantiatedComputeShader.SetFloat("_MinWidth", currentPresets.MinWidth);

        m_InstantiatedComputeShader.SetFloat("_MaxHeight", currentPresets.MaxHeight);
        m_InstantiatedComputeShader.SetFloat("_MaxWidth", currentPresets.MaxWidth);
        m_InstantiatedMaterial.SetColor("_TopTint", currentPresets.topTint);
        m_InstantiatedMaterial.SetColor("_BottomTint", currentPresets.bottomTint);
    }

    public void Reset()
    {
        m_fastMode = false;
        OnDisable();
        MainSetup(true);
    }

    public void ResetFaster()
    {
        m_fastMode = true;
        OnDisable();
        MainSetup(false);
    }
    private void SetGrassDataUpdate()
    {
        // variables sent to the shader every frame
        m_InstantiatedComputeShader.SetFloat("_Time", Time.time);
        m_InstantiatedComputeShader.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
        if (interactors.Length > 0)
        {
            Vector4[] positions = new Vector4[interactors.Length];

            for (int i = 0; i < interactors.Length; i++)
            {
                positions[i] = new Vector4(interactors[i].transform.position.x, interactors[i].transform.position.y, interactors[i].transform.position.z,
                interactors[i].radius);

            }
            m_InstantiatedComputeShader.SetVectorArray(shaderID, positions);
            m_InstantiatedComputeShader.SetFloat("_InteractorsLength", interactors.Length);
        }
        if (m_MainCamera != null)
        {
            m_InstantiatedComputeShader.SetVector("_CameraPositionWS", m_MainCamera.transform.position);
        }
#if UNITY_EDITOR
        // if we dont have a main camera (it gets added during gameplay), use the scene camera
        else if (view != null)
        {
            m_InstantiatedComputeShader.SetVector("_CameraPositionWS", view.camera.transform.position);
        }
#endif
    }


    // draw the bounds gizmos
    void OnDrawGizmos()
    {
        if (currentPresets)
        {
            if (currentPresets.drawBounds)
            {
                Gizmos.color = new Color(0, 1, 0, 0.3f);
                for (int i = 0; i < BoundsListVis.Count; i++)
                {
                    Gizmos.DrawWireCube(BoundsListVis[i].center, BoundsListVis[i].size);
                }
                Gizmos.color = new Color(1, 0, 0, 0.3f);
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }

    }
}

[System.Serializable]
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind
       .Sequential)]
public struct GrassData
{
    public Vector3 position;
    public Vector3 normal;
    public Vector2 length;
    public Vector3 color;
}
