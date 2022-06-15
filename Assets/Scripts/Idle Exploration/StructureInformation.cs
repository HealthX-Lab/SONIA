using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StructureInformation : MonoBehaviour
{
    Transform cam; // The camera's transform
    GameObject canvas; // The information canvas
    
    void Start()
    {
        cam = Camera.main.transform;
        
        // Creating a new canvas
        canvas = Instantiate(Resources.Load<GameObject>("Structure Canvas"));
        canvas.SetActive(false);
    }

    void FixedUpdate()
    {
        Transform canvasTransform = canvas.transform;

        // Positioning and rotating the canvas
        canvasTransform.position = transform.position + (transform.forward * 0.2f) + (Vector3.up * 0.2f);
        canvasTransform.LookAt(cam);
        
        canvasTransform.localRotation = Quaternion.Euler(new Vector3(
            -10,
            canvasTransform.localRotation.eulerAngles.y,
            canvasTransform.localRotation.eulerAngles.z
        ));
    }

    /// <summary>
    /// Method to quickly set teh name and description of the selected structure
    /// </summary>
    /// <param name="name">The name of the selected structure</param>
    /// <param name="description">The description of the selected structure</param>
    public void SetUI(string name, string description)
    {
        canvas.SetActive(true);
        
        TMP_Text[] text = canvas.GetComponentsInChildren<TMP_Text>();

        text[0].text = name;
        text[1].text = description;
    }
}
