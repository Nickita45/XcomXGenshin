using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraObjectTransparency : MonoBehaviour
{
    [SerializeField]
    private Material _transperent;

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

    private List<(Outline outline,Material[][] materials)> _renderers = new();

    public void HideObjectsInLine(Vector3 origin, Vector3 target)
    {
        Vector3 diff = target - origin;
        RaycastHit[] hits = Physics.RaycastAll(origin - diff.normalized * 0.5f, diff.normalized, diff.magnitude + 1f);
        //Debug.DrawRay(origin, diff, Color.blue, 15f);

        SetNewRenderersFromHits(hits);
    }

    public void SetNewRenderersFromHits(RaycastHit[] hits)
    {
        List<(Outline outline, Material[][] materials)> newRenderers = new();

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            // If the component or its parent can be hidden
            if (hit.transform.GetComponentsInParent<TerritoryInfo>().Any(n => n.CanBeHiddenByCamera()))
            {
                // Get list of renderers on the game object, all of its children and its parent
                //List<Outline> rends = hit.transform.GetComponentsInChildren<Renderer>().ToList();
                //Renderer parentRenderer = hit.transform.GetComponentInParent<Renderer>();
                //if (parentRenderer) rends.Add(parentRenderer);
                var parent = hit.transform.GetComponentsInParent<TerritoryInfo>().First();
                var outline = parent.gameObject.GetComponent<Outline>();
                Material[][] materials = parent.gameObject.GetComponentsInChildren<Renderer>().Select(n => n.sharedMaterials).ToArray();
                if (!outline || outline == null)
                {
                    Color color;
                    ColorUtility.TryParseHtmlString(ConfigurationManager.GlobalDataJson.outlineObjectsOnMapColor, out color);
                    outline = ObjectUtils.AddOutline(parent.gameObject,
                        color,
                        ConfigurationManager.GlobalDataJson.outlineObjectsOnMapWidth);
                    outline.enabled = false;
                }
                newRenderers.Add((outline, materials));
                //foreach (var rend in rends)
               // {
               //     newRenderers.Add(
                        //new(rend,
                        //    rend.materials.Select(material => material.shader).ToList(),
                         //   rend.materials.Select(material => material.color).ToList())
                //    );
                //}
            }
        }

        for (int i = 0; i < newRenderers.Count; i++)
        {
            int index = _renderers.FindIndex(r => r.outline == newRenderers[i].outline);
            if (index == -1)
            {
                newRenderers[i].outline.enabled = true;
                foreach (var renderer in newRenderers[i].outline.gameObject.GetComponentsInChildren<Renderer>())
                {
                    var materials = renderer.materials; 
                    for (int j = 0; j < materials.Length-2; j++)
                    {
                        materials[j] = _transperent;
                    }
                    renderer.materials = materials; 
                }

                // Replace shaders and colors for new renderers
                /*foreach (Material material in newRenderers[i].Renderer.materials)
                {
                    material.shader = Shader.Find("Universal Render Pipeline/Simple Lit");
                    material.SetFloat("_Surface", 1);
                    Color tempColor = material.color;
                    tempColor.a = 0.4F;
                    material.color = tempColor;
                }*/
            }
            else
            {

                // For old renderers, cache the previous values of shaders and colors
                newRenderers[i] =(_renderers[index].outline, _renderers[index].materials);

                //newRenderers[i].Shaders = _renderers[index].Shaders;
                //newRenderers[i].Colors = _renderers[index].Colors;
            }
        }

        for (int i = 0; i < _renderers.Count; i++)
        {
            if (newRenderers.FindIndex(r => r.outline == _renderers[i].outline) == -1)
            {
                _renderers[i].outline.enabled = false;
                var renderers = _renderers[i].outline.gameObject.GetComponentsInChildren<Renderer>();

                for (int m = 0; m < renderers.Length; m++)
                {
                    var materials = renderers[m].materials; 
                    for (int j = 0; j < materials.Length - 2; j++) 
                    {
                        materials[j] = _renderers[i].materials[m][j];

                    }
                    var listMat = materials.ToList();
                    if(listMat.Count >= 3)
                        listMat.RemoveRange(materials.Length - 2, 2);


                    renderers[m].materials = listMat.ToArray();
                }
                /*for (int j = 0; j < _renderers[i].Shaders.Count; j++)
                {
                    // Return old shaders and colors to objects
                    _renderers[i].Renderer.materials[j].shader = _renderers[i].Shaders[j];
                    _renderers[i].Renderer.materials[j].color = _renderers[i].Colors[j];
                }*/
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

            SetNewRenderersFromHits(hits);  
        }
    }
}
