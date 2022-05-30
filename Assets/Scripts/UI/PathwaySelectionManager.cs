using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class PathwaySelectionManager : MonoBehaviour
{
    [Header("Pathways & UI")]
    [Tooltip("All of the pathways currently in the system")]
    public Pathway[] pathways;

    [Header("Input variables")]
    [Tooltip("The controller trigger boolean")]
    [SerializeField] SteamVR_Action_Boolean trigger;
    [Tooltip("Which hand's input")]
    [SerializeField] SteamVR_Input_Sources handType;
    
    [Header("Structure outlines")]
    [Tooltip("The outline colour for the large structures and the UI")]
    [SerializeField] Color outlineColour;
    [Tooltip("The outline colour for the mini structures")]
    [SerializeField] Color miniOutlineColour;

    UIManager manager; // The overall manager for the minor UI managers such as this one
    
    [HideInInspector] public Pathway currentPathway; // The Pathway currently being accessed
    PathwayController pathwayController; // The controller script for the Pathway visualization

    ControllerLaser laser; // The right hand's laser script
    GameObject lastHitObject; // The last object hit with the laser script
    GameObject lastMiniOutlineStructure, lastOutlineStructure; // The last selected miniature and large structures, respectively

    //StructureZoom zoom; // The script to zoom the view around
    
    MiniatureBrainController miniBrain; // The script for the miniature brain

    void Start()
    {
        // Initializing sub-scripts
        manager = FindObjectOfType<UIManager>();
        laser = FindObjectOfType<ControllerLaser>();
        //zoom = FindObjectOfType<StructureZoom>();
        miniBrain = FindObjectOfType<MiniatureBrainController>();
        pathwayController = FindObjectOfType<PathwayController>();

        // Making sure that the Pathway dictionary in the Pathway controller script exists
        if (pathwayController.pathwayDict == null)
        {
            pathwayController.pathwayDict = new Dictionary<GameObject, Pathway>();
        }
        
        // Adding listeners for the controller input
        trigger.AddOnStateDownListener(OnTriggerDown, handType);
    }

    void FixedUpdate()
    {
        // Making sure that the laser is hitting one of the options
        if (laser.hitObject != null && laser.hitObject.CompareTag("Pathway Option"))
        {
            if (lastHitObject != laser.hitObject)
            {
                ResetHitting(); // Resetting the last options' outline
                
                // Enabling the outline if hitting the option
                laser.hitObject.GetComponent<Outline>().enabled = true;
                lastHitObject = laser.hitObject;
            }
        }
        // Resetting the last option if not hitting anything
        else
        {
            ResetHitting();
        }
    }
    
    /// <summary>
    /// Quick method to reset the last hit object
    /// </summary>
    void ResetHitting()
    {
        if (lastHitObject != null)
        {
            lastHitObject.GetComponent<Outline>().enabled = false;
            lastHitObject = null;
        }
    }

    /// <summary>
    /// Triggers when given action starts on the given controller
    /// </summary>
    /// <param name="fromAction">Action to be anticipated</param>
    /// <param name="fromSource">Controller/hand that performs the action</param>
    void OnTriggerDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        // Setting the current Pathway UI option's corresponding one
        if (laser.hitObject != null)
        {
            SetCurrentPathway(pathwayController.pathwayDict[lastHitObject]);
        }
    }

    /// <summary>
    /// Method to generate the pathway selection UI after the Pathways have all been created
    /// </summary>
    public void GeneratePathwayOptions()
    {
        // Creating the option canvas
        GameObject canvas = Instantiate(Resources.Load<GameObject>("Pathway Canvas"), transform);
                
        GameObject option = Resources.Load<GameObject>("Pathway Option");
        
        for (int i = 0; i < pathways.Length; i++)
        {
            // Creating and initializing the new options attributes
            GameObject newOption = Instantiate(option, canvas.GetComponentInChildren<GridLayoutGroup>().transform);
            
            TMP_Text[] temp = newOption.GetComponentsInChildren<TMP_Text>();
            temp[0].text = pathways[i].narrative.Name;
            temp[1].text = pathways[i].narrative.Description;
            
            newOption.GetComponent<Outline>().enabled = false;
            
            pathwayController.pathwayDict.Add(newOption, pathways[i]);
        }
    }

    /// <summary>
    /// Highlights the current Pathway, both visually and in code
    /// </summary>
    /// <param name="path">The Pathway to be set visible</param>
    void SetCurrentPathway(Pathway path)
    {
        currentPathway = path;
        GameObject tempStructure = currentPathway.narrative.Current.Object;
        //zoom.target = tempStructure; // Setting the view zoom target
        
        // Making sure the last large outline is hidden
        if (lastOutlineStructure != null)
        {
            lastOutlineStructure.GetComponent<Outline>().enabled = false;
        }

        Outline tempOutline = tempStructure.GetComponent<Outline>();

        // Adding/enabling the large outline for the current node
        if (tempOutline != null)
        {
            tempOutline.enabled = true;
        }
        else
        {
            Outline newOutline = tempStructure.AddComponent<Outline>();
            newOutline.OutlineWidth = 10;
            newOutline.OutlineColor = outlineColour;
        }

        lastOutlineStructure = tempStructure;

        // Making sure the last mini outline is hidden
        if (lastMiniOutlineStructure != null)
        {
            lastMiniOutlineStructure.GetComponent<Outline>().enabled = false;
        }

        GameObject[] temp = GameObject.FindGameObjectsWithTag("Structure Node");

        foreach (GameObject i in temp)
        {
            // Finding the corresponding mini structure to the current node
            if (i.transform.IsChildOf(miniBrain.transform) && i.name.Equals(tempStructure.name) && i.layer.Equals(tempStructure.layer))
            {
                Outline tempMiniOutline = i.GetComponent<Outline>();

                // Adding/enabling the large outline for the current node
                if (tempMiniOutline != null)
                {
                    tempMiniOutline.enabled = true;
                }
                else
                {
                    Outline newMiniOutline = i.AddComponent<Outline>();
                    newMiniOutline.OutlineWidth = 5;
                    newMiniOutline.OutlineColor = miniOutlineColour;
                }

                lastMiniOutlineStructure = i;

                break;
            }
        }
        
        //currentPathway.narrative.GoToNext(0); // Automatically goes to the next structure
        manager.TogglePathwayManager();
        
        pathwayController.SetCurrentPathway(currentPathway);
    }
}
