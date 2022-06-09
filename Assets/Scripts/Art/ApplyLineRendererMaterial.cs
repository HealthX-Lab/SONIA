using System;
using UnityEngine;

public class ApplyLineRendererMaterial : MonoBehaviour
{
    [Tooltip("The material to be applied")]
    public Material material;
    
    void Start()
    {
        Apply();
    }

    /// <summary>
    /// Checks through all LineRenderers in children of the object and applies the given Material
    /// </summary>
    public void Apply()
    {
        LineRenderer[] temp = GetComponentsInChildren<LineRenderer>();

        // Goes through all the LineRenderers in the object's children, and sets their materials
        foreach (LineRenderer i in temp)
        {
            i.material = material;
        }
    }
}