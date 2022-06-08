using System;
using UnityEngine;

public class SetLineRendererMaterial : MonoBehaviour
{
    [Tooltip("The material to be applied")]
    public Material material;
    
    void Start()
    {
        LineRenderer[] temp = GetComponentsInChildren<LineRenderer>();

        // Goes through all the LineRenderers in the object's children, and sets their materials
        foreach (LineRenderer i in temp)
        {
            i.material = material;
        }
    }
}