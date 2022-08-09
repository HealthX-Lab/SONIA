using System;
using UnityEngine;
using Valve.VR;

/// <summary>
/// MonoBehaviour to temporarily grab and reposition various UI (or other) objects in the scene
/// </summary>
/// <organization>Health-X Lab</organization>
/// <project>Insideout (May-August 2022)</project>
/// <author>Owen Hellum</author>
public class GrabUI : MonoBehaviour
{
    [SerializeField, Tooltip("The triggering action from the left hand controller")]
    SteamVR_Action_Boolean action;

    Transform originalParent; // The parent of the grabTransform
    // The current object being collided with and the current object being grabbed, respectively
    RectTransform hitTransform, grabTransform;
    
    void Start()
    {
        // Adding a listener for the triggering action
        action.AddOnStateDownListener(OnActionDown, SteamVR_Input_Sources.LeftHand);
        action.AddOnStateUpListener(OnActionUp, SteamVR_Input_Sources.LeftHand);
    }

    void OnTriggerEnter(Collider other)
    {
        hitTransform = other.gameObject.GetComponent<RectTransform>(); // Assigning the colliding object
    }

    void OnTriggerExit(Collider other)
    {
        hitTransform = null; // Nullifying the colliding object
    }

    /// <summary>
    /// Triggers when given action starts on the given controller
    /// </summary>
    /// <param name="fromAction">Action to be anticipated</param>
    /// <param name="fromSource">Controller/hand that performs the action</param>
    void OnActionDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        // Making sure an object is being collided with
        if (hitTransform != null)
        {
            grabTransform = hitTransform;

            // Setting the object as a parent of the controller
            originalParent = grabTransform.parent;
            grabTransform.SetParent(transform); // TODO: need to do proper error handling
        }
    }
    
    /// <summary>
    /// Triggers when given action stops on the given controller
    /// </summary>
    /// <param name="fromAction">Action to be anticipated</param>
    /// <param name="fromSource">Controller/hand that performs the action</param>
    void OnActionUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        grabTransform.SetParent(originalParent); // Returning the object to it's original parent

        grabTransform = null;
        originalParent = null;
    }
}