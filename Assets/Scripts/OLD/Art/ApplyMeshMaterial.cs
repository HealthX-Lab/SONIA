using System;
using Unity.VisualScripting;
using UnityEngine;

public class ApplyMeshMaterial : MonoBehaviour
{
    [Tooltip("The material to be applied to all MeshRenderers within the objet")]
    public Material material;
    [Tooltip("The amount of seconds to wait before actually changing the material (to wait for other scripts to load)")]
    [SerializeField] float waitSeconds;

    void Start()
    {
        if (waitSeconds > 0)
        {
            Invoke(nameof(Apply), waitSeconds);
        }
        else
        {
            Apply();
        }
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