using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StructureUIController : MonoBehaviour
{
    [Tooltip("Display name of the structure")]
    [SerializeField] string name;
    [Tooltip("Description name of the structure")]
    [SerializeField] string description;

    GameObject canvas; // The canvas to be instantiated
    bool hasCreatedCanvas;
    Transform cameraTransform, canvasTransform; // The camera and canvas Transforms, respectively
    BoundsInfo bounds; // The mesh bounds info of the structure
    TMP_Text nameText, descriptionText; // The name and description TMP text

    void Start()
    {
        cameraTransform = Camera.main.transform;
        bounds = new BoundsInfo(gameObject);
        
        // Setting the UI if one of the fields is not empty
        if (!name.Equals("") || !description.Equals(""))
        {
            SetUI(name, description);
        }
    }
    
    void FixedUpdate()
    {
        if (hasCreatedCanvas)
        {
            // Pointing the structure UI towards the user at all times
            canvasTransform.position = (cameraTransform.position - transform.position).normalized * (bounds.Magnitude * 0.4f);
            canvasTransform.localPosition = new Vector3(canvasTransform.position.x, 0, canvasTransform.position.z);
            canvas.transform.LookAt(cameraTransform);
        }
    }

    /// <summary>
    /// Checks to see if the structure canvas has been created, and if not, it creates it
    /// </summary>
    void CheckCanvas()
    {
        if (!hasCreatedCanvas)
        {
            canvas = Instantiate(Resources.Load<GameObject>("Structure Canvas"), transform);
            canvasTransform = canvas.transform;

            // Getting the TMP text Components in the children
            TMP_Text[] temp = canvas.GetComponentsInChildren<TMP_Text>();
            nameText = temp[0];
            descriptionText = temp[1];

            hasCreatedCanvas = true;
        }
    }

    /// <summary>
    /// Method to set the UI of the canvas at any time
    /// </summary>
    /// <param name="n">New name text</param>
    /// <param name="d">New description text</param>
    public void SetUI(string n, string d)
    {
        CheckCanvas(); // Checking to make sure a canvas has been created first
        
        // Setting the values
        nameText.text = n;
        descriptionText.text = d;
    }
}
