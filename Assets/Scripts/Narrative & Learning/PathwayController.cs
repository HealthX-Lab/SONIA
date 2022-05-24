using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PathwayController : MonoBehaviour
{
    [Tooltip("All of the pathways currently in the system")]
    [SerializeField] GameObject[] pathways;
    [Tooltip("The left hand's Transform component")]
    [SerializeField] Transform leftHand;

    GameObject[] options; // The generated options for viewing the pathways
    ControllerLaser laser; // The right hand's laser script
    GameObject lastHitObject; // The last object hit with the laser script

    void Start()
    {
        // Creating the option canvas
        GameObject canvas = Instantiate(Resources.Load<GameObject>("Pathway Canvas"), leftHand);
        canvas.transform.Rotate(Vector3.right, 90, Space.Self);
        
        // Initializing the option variables
        GameObject option = Resources.Load<GameObject>("Pathway Option");
        options = new GameObject[pathways.Length];
        
        for (int i = 0; i < pathways.Length; i++)
        {
            // Creating and initializing the new options attributes
            GameObject newOption = Instantiate(option, canvas.GetComponentInChildren<GridLayoutGroup>().transform);
            newOption.GetComponentInChildren<TMP_Text>().text = pathways[i].name;
            newOption.GetComponent<Outline>().enabled = false;

            options[i] = newOption;
        }

        laser = FindObjectOfType<ControllerLaser>();
    }

    void FixedUpdate()
    {
        // Making sure that the laser's hitting one of the options
        if (laser.hitObject != null && options.Contains(laser.hitObject))
        {
            if (lastHitObject != laser.hitObject)
            {
                ResetOutline(); // Resetting the last options' outline
                
                // Enabling the outline if hitting the option
                laser.hitObject.GetComponent<Outline>().enabled = true;
                lastHitObject = laser.hitObject;
            }
        }
        // Resetting the last options' outline if not hitting anything
        else
        {
            ResetOutline();
        }
    }

    /// <summary>
    /// Quick method to reset the last hit object's outline
    /// </summary>
    void ResetOutline()
    {
        if (lastHitObject != null)
        {
            lastHitObject.GetComponent<Outline>().enabled = false;       
        }
    }
}
