using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraObjectTransparency : MonoBehaviour
{
    // Cached data about the renderer
    public class RenderData
    {
        public RenderData(Renderer renderer, List<Shader> shaders, List<Color> colors)
        {
            Renderer = renderer;
            Shaders = shaders;
            Colors = colors;
        }

        public Renderer Renderer;
        public List<Shader> Shaders;
        public List<Color> Colors;
    }

    private List<RenderData> _renderers = new();

    public void HideObjectsInLine(Vector3 origin, Vector3 target)
    {
        Vector3 diff = target - origin;
        RaycastHit[] hits = Physics.RaycastAll(origin - diff.normalized * 0.5f, diff.normalized, diff.magnitude + 1f);
        //Debug.DrawRay(origin, diff, Color.blue, 15f);

        SetNewRenderersFromHits(hits);
    }

    public void SetNewRenderersFromHits(RaycastHit[] hits)
    {
        List<RenderData> newRenderers = new();

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            // If the component or its parent can be hidden
            if (hit.transform.GetComponent<CanBeHiddenByCamera>() != null ||
                hit.transform.parent?.GetComponent<CanBeHiddenByCamera>() != null)
            {
                // Get list of renderers on the game object, all of its children and its parent
                List<Renderer> rends = hit.transform.GetComponentsInChildren<Renderer>().ToList();
                Renderer parentRenderer = hit.transform.GetComponentInParent<Renderer>();
                if (parentRenderer) rends.Add(parentRenderer);

                foreach (var rend in rends)
                {
                    newRenderers.Add(
                        new(rend,
                            rend.materials.Select(material => material.shader).ToList(),
                            rend.materials.Select(material => material.color).ToList())
                    );
                }
            }
        }

        for (int i = 0; i < newRenderers.Count; i++)
        {
            int index = _renderers.FindIndex(r => r.Renderer == newRenderers[i].Renderer);
            if (index == -1)
            {
                // Replace shaders and colors for new renderers
                foreach (Material material in newRenderers[i].Renderer.materials)
                {
                    material.shader = Shader.Find("Transparent/Diffuse");
                    Color tempColor = material.color;
                    tempColor.a = 0.4F;
                    material.color = tempColor;
                }
            }
            else
            {
                // For old renderers, cache the previous values of shaders and colors
                newRenderers[i].Shaders = _renderers[index].Shaders;
                newRenderers[i].Colors = _renderers[index].Colors;
            }
        }

        for (int i = 0; i < _renderers.Count; i++)
        {
            if (newRenderers.FindIndex(r => r.Renderer == _renderers[i].Renderer) == -1)
            {
                for (int j = 0; j < _renderers[i].Shaders.Count; j++)
                {
                    // Return old shaders and colors to objects
                    _renderers[i].Renderer.materials[j].shader = _renderers[i].Shaders[j];
                    _renderers[i].Renderer.materials[j].color = _renderers[i].Colors[j];
                }
            }
        }

        _renderers = newRenderers;
    }

    void Update()
    {
        FreeCamera freeCamera = Manager.CameraManager.FreeCamera;
        if (freeCamera.IsMainCamera())
        {
            // Hit all colliders from a position slightly behind of the camera
            RaycastHit[] hits = Physics.RaycastAll(freeCamera.transform.position - freeCamera.transform.forward * 3f, freeCamera.transform.forward, 4.5f);

            Debug.Log(hits.Length);
            SetNewRenderersFromHits(hits);
        }
    }
}
