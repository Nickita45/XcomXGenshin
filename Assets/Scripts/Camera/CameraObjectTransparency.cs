using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CameraObjectTransparency : MonoBehaviour
{

    private List<RenderData> _renderers = new();

    // Cached data about the renderer
    private class RenderData
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

    void Update()
    {
        // Hit all colliders from a position slightly behind of the camera
        RaycastHit[] hits = Physics.RaycastAll(transform.position - transform.forward * 3f, transform.forward, 4.5F);

        List<RenderData> newRenderers = new();

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            TerritoryInfo info = hit.transform.GetComponent<TerritoryInfo>();
            List<Renderer> rends = hit.transform.GetComponentsInChildren<Renderer>().ToList(); //change this because some objects has TerritoryInfo in children
            rends.AddRange(hit.transform.GetComponentsInParent<Renderer>().ToList()); // or in parents

            if (hit.transform.GetComponent<Renderer>())
                rends.Add(hit.transform.GetComponent<Renderer>()); //add himself

            if (rends.Count > 0)
            {
                foreach (var rend in rends) //serch all of them
                {
                    // Save the renderer either if there is not territory info or info type is Shelter
                    if (info)
                    {
                        if (info.Type == TerritoryType.Shelter)
                        {
                            newRenderers.Add(
                                new(rend,
                                rend.materials.Select(material => material.shader).ToList(),
                                rend.materials.Select(material => material.color).ToList())
                            );
                        }
                    }
                    else
                    {
                        newRenderers.Add(
                                new(rend,
                                rend.materials.Select(material => material.shader).ToList(),
                                rend.materials.Select(material => material.color).ToList())
                            );
                    }
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
                    tempColor.a = 0.3F;
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
}
