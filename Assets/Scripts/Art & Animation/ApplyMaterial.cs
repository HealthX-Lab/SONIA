using System;
using Unity.VisualScripting;
using UnityEngine;

public class ApplyMaterial : MonoBehaviour
{
    [Tooltip("The material to be applied to all MeshRenderers within the objet")]
    [SerializeField] Material material;
    [Tooltip("The amount of seconds to wait before actually changing the material (to wait for other scripts to load)")]
    [SerializeField] float waitSeconds = 1;

    void Start()
    {
        Invoke(nameof(Apply), waitSeconds);
    }

    /// <summary>
    /// Checks through all MeshRenders in children of the object and applies the given Material
    /// </summary>
    void Apply()
    {
        MeshRenderer[] renderers = gameObject.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer i in renderers)
        {
            i.material = material;
        }
    }
}