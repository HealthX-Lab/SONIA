using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EdgeDescriptionController : MonoBehaviour
{
    Transform cameraTransform;
    
    void FixedUpdate()
    {
        transform.LookAt(cameraTransform);
    }

    public void SetAlignment()
    {
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

    public void SetUI(string edgeDescription)
    {
        GetComponentInChildren<TMP_Text>().text = edgeDescription;
    }
}
