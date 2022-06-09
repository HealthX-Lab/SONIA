using System;
using Unity.VisualScripting;
using UnityEngine;

public class ApplyMeshMaterial : MonoBehaviour
{
    [Tooltip("The material to be applied to all MeshRenderers within the objet")]
    [HideInInspector] public Material material;
    [Tooltip("The amount of seconds to wait before actually changing the material (to wait for other scripts to load)")]
    [SerializeField] float waitSeconds = 1;
    [Tooltip("Whether the script is being added to a VR controller")]
    [SerializeField] bool isController;

    void Start()
    {
        if (isController)
        {
            Invoke(nameof(Apply), waitSeconds);
        }
        // TODO: should probably add this for continuity
        /*
        else
        {
            Apply();
        }
        */
    }

    /// <summary>
    /// Checks through all MeshRenders in children of the object and applies the given Material
    /// </summary>
    public void Apply()
    {
        MeshRenderer[] renderers = gameObject.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer i in renderers)
        {
            i.material = material;
        }
    }
}