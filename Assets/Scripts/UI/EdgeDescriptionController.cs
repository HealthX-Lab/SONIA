using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EdgeDescriptionController : MonoBehaviour
{
    [Tooltip("Description of the Pathway edge")]
    public string description;
    
    [HideInInspector] public GameObject source; // The starting GameObject of the edge
    
    Transform cameraTransform; // The main Camera's Transform
    
    void FixedUpdate()
    {
        transform.LookAt(cameraTransform);
    }

    public void SetAlignment(GameObject src)
    {
        source = src;
        
        cameraTransform = Camera.main.transform;
                
        // Scaling the UI text based on its distance from the user
        float temp = Vector3.Distance(transform.position, cameraTransform.position);
        transform.localScale = Vector3.one *
                                     (Mathf.Pow(1.15f, temp - 20f) + 0.5f);

        if (transform.localScale.x > (temp / 10f))
        {
            transform.localScale = Vector3.one * (temp / 10f);
        }
        
        Vector3 target = GetComponentInParent<particleAttractorLinear>().target;
        float distance = Vector3.Distance(transform.position, target);

        transform.position += (target - transform.position).normalized * (distance * 0.1f);
        transform.position += Vector3.up * transform.localScale.x / 2f;
    }

    /// <summary>
    /// Set's this edge's text description
    /// </summary>
    /// <param name="edgeDescription">The new description to be set</param>
    public void SetUI(string edgeDescription)
    {
        GetComponentInChildren<TMP_Text>().text = edgeDescription;
    }
}
