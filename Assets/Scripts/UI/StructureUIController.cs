using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StructureUIController : MonoBehaviour
{
    [Tooltip("Display name of the structure")]
    public new string name;
    [Tooltip("Description name of the structure")]
    public string description;
    [Tooltip("The layer on which to raycast (so it goes through other structures)")]
    [SerializeField] LayerMask layer;
    [Tooltip("Whether to scale the UI based on distance or not")]
    [SerializeField] bool fixedScale = true;
    [Tooltip("The unscaled Ui size")]
    [SerializeField] float scale = 8;
    [Tooltip("The amount that the structure UI should float off of the surface")]
    [SerializeField] float offsetAmount = 1;
    
    bool hasCreatedCanvas; // Whether the structure UI canvas has been created yet
    [HideInInspector] public Transform canvasTransform; // The UI canvas Transform
    Transform cameraTransform; // The camera Transform
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
            if (Physics.Raycast(cameraTransform.position, bounds.GlobalCentre, out RaycastHit hit, Single.PositiveInfinity, layer))
            {
                canvasTransform.position = hit.point + ((cameraTransform.position - bounds.GlobalCentre).normalized * offsetAmount);
            }

            if (fixedScale)
            {
                canvasTransform.localScale = Vector3.one * scale;
            }
            else
            {
                // Scaling the UI text based on its distance from the user
                float temp = Vector3.Distance(transform.position, hit.point);
                canvasTransform.localScale = Vector3.one *
                                             (Mathf.Pow(1.15f, temp - 20f) + 0.5f);
            
                if (canvasTransform.localScale.x > (temp / 10f))
                {
                    canvasTransform.localScale = Vector3.one * (temp / 10f);
                }
            }
            
            // Pointing the structure UI towards the user at all times
            canvasTransform.LookAt(cameraTransform);
        }
    }

    /// <summary>
    /// Checks to see if the structure canvas has been created, and if not, it creates it
    /// </summary>
    public void CheckCanvas()
    {
        if (!hasCreatedCanvas)
        {
            canvasTransform = Instantiate(Resources.Load<GameObject>("Structure Canvas"), transform).transform;

            // Getting the TMP text Components in the children
            TMP_Text[] temp = canvasTransform.GetComponentsInChildren<TMP_Text>();
            nameText = temp[0];
            descriptionText = temp[1];

            hasCreatedCanvas = true;
        }
    }

    /// <summary>
    /// Method to set the UI of the canvas at any time
    /// </summary>
    /// <param name="structureName">New name text</param>
    /// <param name="structureDescription">New description text</param>
    public void SetUI(string structureName, string structureDescription)
    {
        CheckCanvas(); // Checking to make sure a canvas has been created first
        
        // Setting the values
        nameText.text = structureName;
        descriptionText.text = structureDescription;
    }
}
