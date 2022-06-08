using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Valve.VR;

public class StructureManager : MonoBehaviour
{
    [Header("Pathways & UI")]
    [Tooltip("The outline colour for the UI")]
    [SerializeField] Color outlineColour;
    [Tooltip("The parent objects of the central, right, and left structure UI")]
    [SerializeField] GameObject central, right, left;
    [Tooltip("The buttons for selecting right and left options to change structures")]
    [SerializeField] GameObject rightOption, leftOption;
    
    [Header("Structures")]
    [Tooltip("The scale to which all of the structure previews should be resized")]
    [SerializeField] float structureScale = 0.01f;
    [Tooltip("The materials applied to the structure previews (occlusion for the right and left)")]
    [SerializeField] Material defaultStructureMaterial, occlusionStructureMaterial;
    [Tooltip("The objects where the structure previews should be spawned in")]
    [SerializeField] GameObject centralStructure, rightStructure, leftStructure;
    
    [Tooltip("The controller trigger boolean")]
    [SerializeField] SteamVR_Action_Boolean trigger;
    
    UIManager manager; // The overall manager for the minor UI managers such as this one

    ControllerLaser laser; // The right hand's laser script
    GameObject lastHitObject; // The last object hit with the laser script

    [HideInInspector] public bool isLeftPrevious; // Whether the structure being previewed on the left is from the current structure's previous NarrativeNode

    void Start()
    {
        // Initializing sub-scripts
        manager = FindObjectOfType<UIManager>();
        laser = FindObjectOfType<ControllerLaser>();
        
        // Adding listeners for the controller input
        trigger.AddOnStateDownListener(OnTriggerDown, SteamVR_Input_Sources.RightHand);
    }

    void FixedUpdate()
    {
        // Making sure that the laser is hitting one of the options
        if (laser.hitObject != null && laser.hitObject.CompareTag("Structure Option"))
        {
            if (lastHitObject != laser.hitObject)
            {
                ResetHitting(); // Resetting the last options' outline
                
                Outline tempOutline = laser.hitObject.GetComponent<Outline>();
                
                if (tempOutline != null)
                {
                    // Enabling the outline if hitting the option
                    tempOutline.enabled = true;
                    lastHitObject = laser.hitObject;
                }
                else
                {
                    Outline newOutline = laser.hitObject.AddComponent<Outline>();
                    newOutline.OutlineWidth = 10;
                    newOutline.OutlineColor = outlineColour;
                }
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
    /// Method to set the UI of the canvas at any time
    /// </summary>
    /// <param name="names">Names of the current and next structures in the Narrative</param>
    /// <param name="description">Description of the current structure</param>
    /// <param name="edges">Edge descriptions for the next structures</param>
    /// <param name="structures">Copied structures to be previewed for the current and next structures</param>
    public void SetUI(string[] names, string description, string[] edges, GameObject[] structures)
    {
        // Getting and setting the current name and description
        TMP_Text[] centralTexts = central.GetComponentsInChildren<TMP_Text>();
        centralTexts[0].text = names[0];
        centralTexts[1].text = description;

        CreateStructurePreview(structures[0], centralStructure.transform, defaultStructureMaterial); // Generating the current structure preview
        
        // Hiding the right and left options (temporarily)
        right.SetActive(false);
        rightOption.SetActive(false);
        left.SetActive(false);
        leftOption.SetActive(false);
        
        // If there's a right option
        if (!names[1].Equals(""))
        {
            // Making it visible
            right.SetActive(true);
            rightOption.SetActive(true);
            
            // Setting its text
            rightOption.GetComponentInChildren<TMP_Text>().text = edges[0];
            right.GetComponentInChildren<TMP_Text>().text = names[1];

            CreateStructurePreview(structures[1], rightStructure.transform, occlusionStructureMaterial); // Generating its structure preview
        }
        
        // If there's a left option
        if (!names[2].Equals(""))
        {
            // Making it visible
            left.SetActive(true);
            leftOption.SetActive(true);
            
            // Setting its text
            leftOption.GetComponentInChildren<TMP_Text>().text = edges[1];
            left.GetComponentInChildren<TMP_Text>().text = names[2];

            CreateStructurePreview(structures[2], leftStructure.transform, occlusionStructureMaterial); // Generating its structure preview
        }
    }

    /// <summary>
    /// A method to copy a structure to the structure UI for previewing
    /// </summary>
    /// <param name="structure">The structure to be copied</param>
    /// <param name="target">The new parent/target</param>
    /// <param name="structureMaterial">The material to be applied to the structure</param>
    void CreateStructurePreview(GameObject structure, Transform target, Material structureMaterial)
    {
        // Removing any old ones
        if (target.childCount > 0)
        {
            Destroy(target.GetChild(0).gameObject);
        }
        
        // Creating the new structure and orienting it
        GameObject tempStructure = Instantiate(structure, target);
        tempStructure.transform.localScale = Vector3.one * structureScale;
        tempStructure.transform.rotation = structure.transform.rotation;

        // Positioning the structure so that its bounds are in the middle
        BoundsInfo tempStructureBounds = new BoundsInfo(tempStructure);
        tempStructure.transform.position += tempStructure.transform.position - tempStructureBounds.GlobalCentre;
        
        Destroy(tempStructure.GetComponent<Outline>()); // Removing its outline

        foreach (MeshRenderer i in tempStructure.GetComponentsInChildren<MeshRenderer>())
        {
            i.materials = new[] { structureMaterial }; // Setting its new material
            
            Destroy(i.GetComponent<MeshCollider>()); // Removing its collider (so that the big brain structures' UI will work appropriately)
        }
        
        // Removing its old big brain UI
        StructureUIController tempStructureUI = tempStructure.GetComponent<StructureUIController>();
        Destroy(tempStructureUI.canvasTransform.gameObject);
        Destroy(tempStructureUI);

        tempStructure.AddComponent<Rotator>(); // Adding a slow rotation script for a final touch
    }

    /// <summary>
    /// Triggers when given action starts on the given controller
    /// </summary>
    /// <param name="fromAction">Action to be anticipated</param>
    /// <param name="fromSource">Controller/hand that performs the action</param>
    void OnTriggerDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        // Setting the current Pathway UI option's corresponding one
        if (laser.hitObject != null && laser.hitObject.CompareTag("Structure Option"))
        {
            if (laser.hitObject.Equals(rightOption))
            {
                manager.GoToNext(0);
            }
            else if (laser.hitObject.Equals(leftOption))
            {
                if (isLeftPrevious)
                {
                    manager.GoToPrevious();
                }
                else
                {
                    manager.GoToNext(1);
                }
            }
            
            // TODO: check when the Narrative has been completed
        }
    }
}