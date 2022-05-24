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
    [Tooltip("The layer on which to raycast (so it goes through other structures)")]
    [SerializeField] LayerMask layer;
    [SerializeField] bool scaleDownIfTooFar;

    bool hasCreatedCanvas; // Whether the structure UI canvas has been created yet
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
            RaycastHit hit;
            if (Physics.Raycast(cameraTransform.position, bounds.GlobalCentre, out hit, Single.PositiveInfinity, layer))
            {
                canvasTransform.position = hit.point + (cameraTransform.position - bounds.GlobalCentre).normalized;
                //Debug.DrawLine(cameraTransform.position, bounds.GlobalCentre, Color.red);
            }

            // Pointing the structure UI towards the user at all times
            float temp = Vector3.Distance(transform.position, hit.point);
            //print(temp);
            canvasTransform.localScale = Vector3.one *
                                         (Mathf.Pow(1.15f, temp - 20f) + 0.5f);

            if (scaleDownIfTooFar && canvasTransform.localScale.x > (temp / 10f))
            {
                canvasTransform.localScale = Vector3.one * (temp / 10f);
            }
            
            canvasTransform.LookAt(cameraTransform);
        }
    }

    /// <summary>
    /// Checks to see if the structure canvas has been created, and if not, it creates it
    /// </summary>
    void CheckCanvas()
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
