using System;
using UnityEngine;
using Valve.VR;

public class StructureManager : MonoBehaviour
{
    [Header("Input variables")]
    [Tooltip("The controller trigger boolean")]
    [SerializeField] SteamVR_Action_Boolean trigger;
    [Tooltip("Which hand's input")]
    [SerializeField] SteamVR_Input_Sources handType;
    
    [Tooltip("The outline colour for the UI")]
    [SerializeField] Color outlineColour;
    
    ControllerLaser laser; // The right hand's laser script
    GameObject lastHitObject; // The last object hit with the laser script

    void Start()
    {
        laser = FindObjectOfType<ControllerLaser>();
        
        // Adding listeners for the controller input
        trigger.AddOnStateDownListener(OnTriggerDown, handType);
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
    /// Triggers when given action starts on the given controller
    /// </summary>
    /// <param name="fromAction">Action to be anticipated</param>
    /// <param name="fromSource">Controller/hand that performs the action</param>
    void OnTriggerDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        // Setting the current Pathway UI option's corresponding one
        if (laser.hitObject != null)
        {
            // TODO: switch to other structures when clicking UI
            
            // TODO: check when the Narrative has been completed
        }
    }
}