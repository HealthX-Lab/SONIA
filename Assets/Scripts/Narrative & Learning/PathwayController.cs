using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class PathwayController : MonoBehaviour
{
    [Header("Pathways & UI")]
    [Tooltip("All of the pathways currently in the system")]
    [SerializeField] Pathway[] pathways;
    [Tooltip("The left hand's Transform component")]
    [SerializeField] Transform leftHand;
    
    [Header("Input variables")]
    [Tooltip("The controller trigger boolean")]
    [SerializeField] SteamVR_Action_Boolean trigger;
    [Tooltip("Which hand's input")]
    [SerializeField] SteamVR_Input_Sources handType;

    [Header("Structure materials")]
    [Tooltip("The default material for each structure")]
    [SerializeField] Material defaultMaterial;
    [Tooltip("The material when the structure is occluding")]
    [SerializeField] Material occlusionMaterial;
    [Tooltip("The outline colour for the large structures")]
    [SerializeField] Color outlineColour;
    [Tooltip("The outline colour for the mini structures")]
    [SerializeField] Color miniOutlineColour;

    Narrative[] narratives; // The array of Narratives in the system
    Narrative currentNarrative; // The currently-active Narrative

    Dictionary<GameObject, Pathway> pathwayDict; // The dictionary to quickly access the Pathways for each respective UI option
    ControllerLaser laser; // The right hand's laser script
    GameObject lastHitObject; // The last object hit with the laser script
    StructureZoom zoom; // The script to zoom the view around

    MiniatureBrainController miniBrain; // The script for the miniature brain
    GameObject lastMiniOutlineStructure, lastOutlineStructure; // The last selected miniature and large structures, respectively
        
    void Start()
    {
        // Creating the option canvas
        GameObject canvas = Instantiate(Resources.Load<GameObject>("Pathway Canvas"), leftHand);
        canvas.transform.Rotate(Vector3.right, 90, Space.Self);
        
        narratives = new Narrative[pathways.Length];
        
        // Initializing the option variables
        GameObject option = Resources.Load<GameObject>("Pathway Option");
        pathwayDict = new Dictionary<GameObject, Pathway>();
        
        for (int i = 0; i < pathways.Length; i++)
        {
            // TODO: this Narrative stuff will need to be replaced with reading from a file

            NarrativeNode previous = null;
            NarrativeNode first = null;

            // Creating new Narratives from the supplied Pathways
            for (int j = 0; j < pathways[i].nodes.Length; j++)
            {
                NarrativeNode temp = new NarrativeNode(pathways[i].nodes[j].name, "[description goes here]", pathways[i].nodes[j]);
                
                // TODO: need to make this non-linear
                // Chaining them all together
                if (previous != null)
                {
                    previous.SetNext(new []{temp}, new []{"[description goes here]"});
                }
                else
                {
                    first = temp;
                }

                previous = temp;
            }
            
            narratives[i] = new Narrative(pathways[i].name, first);
            
            // Creating and initializing the new options attributes
            GameObject newOption = Instantiate(option, canvas.GetComponentInChildren<GridLayoutGroup>().transform);
            newOption.GetComponentInChildren<TMP_Text>().text = pathways[i].name;
            newOption.GetComponent<Outline>().enabled = false;
            
            pathwayDict.Add(newOption, pathways[i]);
        }
        
        Invoke(nameof(HideAllPathways), 0.1f);

        laser = FindObjectOfType<ControllerLaser>();
        zoom = FindObjectOfType<StructureZoom>();
        miniBrain = FindObjectOfType<MiniatureBrainController>();
        
        // Adding listeners for the controller input
        trigger.AddOnStateDownListener(OnTriggerDown, handType);
    }

    void FixedUpdate()
    {
        // Making sure that the laser is hitting one of the options
        if (laser.hitObject != null && pathwayDict.Keys.Contains(laser.hitObject))
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
    /// Sets the pathway (and its connected structures) active and inactive
    /// </summary>
    /// <param name="path">The Pathway to set</param>
    /// <param name="activeValue">Whether the pathway is being set on or off</param>
    void SetPathway(Pathway path, bool activeValue)
    {
        path.gameObject.SetActive(activeValue); // Setting the pathway visibility

        Material tempMaterial = defaultMaterial;

        if (!activeValue)
        {
            tempMaterial = occlusionMaterial;
        }

        foreach (GameObject i in path.nodes)
        {
            MeshRenderer[] tempRenderers = i.GetComponentsInChildren<MeshRenderer>();

            // Setting the Pathways' structures to the appropriate material and setting their colliders
            foreach (MeshRenderer j in tempRenderers)
            {
                j.material = tempMaterial;
                j.GetComponent<MeshCollider>().enabled = activeValue;
            }
            
            StructureUIController tempStructureUI = i.GetComponent<StructureUIController>();
            
            tempStructureUI.canvasTransform.gameObject.SetActive(activeValue); // Setting the structures' names too

            if (activeValue)
            {
                tempStructureUI.SetUI(currentNarrative.Current.Name, currentNarrative.Current.Description);
            }
            
            // TODO: set edge UI text from Narrative as well
        }
    }

    /// <summary>
    /// Sets all the Pathways to false
    /// </summary>
    void HideAllPathways()
    {
        foreach (Pathway i in pathways)
        {
            SetPathway(i, false);
        }
    }
    
    /// <summary>
    /// Sets all the pathways to false, then shows the given one
    /// </summary>
    /// <param name="path">The Pathway to set visible</param>
    void SetCurrentPathway(Pathway path)
    {
        // Getting the current narrative based on teh given Pathway
        foreach (Narrative i in narratives)
        {
            if (i.Name.Equals(path.name))
            {
                currentNarrative = i;
            }
        }
        
        // Only making the appropriate pathway visible
        HideAllPathways();
        SetPathway(path, true);
        
        zoom.target = currentNarrative.Current.Object; // Setting the view zoom target
        
        // Making sure the last large outline is hidden
        if (lastOutlineStructure != null)
        {
            lastOutlineStructure.GetComponent<Outline>().enabled = false;
        }

        Outline tempOutline = currentNarrative.Current.Object.GetComponent<Outline>();

        // Adding/enabling the large outline for the current node
        if (tempOutline != null)
        {
            tempOutline.enabled = true;
        }
        else
        {
            Outline newOutline = currentNarrative.Current.Object.AddComponent<Outline>();
            newOutline.OutlineWidth = 10;
            newOutline.OutlineColor = outlineColour;
        }

        lastOutlineStructure = currentNarrative.Current.Object;

        // Making sure the last mini outline is hidden
        if (lastMiniOutlineStructure != null)
        {
            lastMiniOutlineStructure.GetComponent<Outline>().enabled = false;
        }

        GameObject[] temp = GameObject.FindGameObjectsWithTag("Structure Node");

        foreach (GameObject i in temp)
        {
            // Finding the corresponding mini structure to the current node
            if (i.transform.IsChildOf(miniBrain.transform) && i.name.Equals(currentNarrative.Current.Object.name) && i.layer.Equals(currentNarrative.Current.Object.layer))
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
                    newMiniOutline.OutlineColor = outlineColour;
                }

                lastMiniOutlineStructure = i;

                break;
            }
        }
        
        // TODO: need to make this a manual button in the UI
        currentNarrative.GoToNext(0); // Automatically goes to teh next structure
    }
    
    /// <summary>
    /// Triggers when given action starts on the given controller
    /// </summary>
    /// <param name="fromAction">Action to be anticipated</param>
    /// <param name="fromSource">Controller/hand that performs the action</param>
    void OnTriggerDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if (laser.hitObject != null)
        {
            SetCurrentPathway(pathwayDict[lastHitObject]); // Setting the current Pathway UI option's corresponding one
        }
    }
}
