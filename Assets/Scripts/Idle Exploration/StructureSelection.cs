using System;
using UnityEditor;
using UnityEngine;
using Valve.VR;

public class StructureSelection : MonoBehaviour
{
    [Tooltip("The material to be used when a structure is selected")]
    [SerializeField] Material selectedMaterial;
    [Tooltip("The length of the laser pointer when not pointing towards anything")]
    [SerializeField] float length = 0.2f;
    [Tooltip("The triggering action from the right hand controller")]
    [SerializeField] SteamVR_Action_Boolean action;

    LineRenderer line; // The laser pointer
    bool hasReset; // Whether the laser pointer has already been reset after pointing away
    GameObject hitObject, selectedObject; // The current object being pointed towards and the clicked object

    MiniBrain miniBrain; // The mini brain script
    BigBrain bigBrain; // The big brain script
    StructureInformation leftHand; // The structure information script in the left hand

    void Start()
    {
        // Creating the laser pointer
        line = gameObject.AddComponent<LineRenderer>();
        line.material = selectedMaterial;
        line.widthMultiplier = 0.005f;

        line.useWorldSpace = false;
        ResetLaser();

        // Getting the required scripts
        miniBrain = FindObjectOfType<MiniBrain>();
        bigBrain = FindObjectOfType<BigBrain>();
        leftHand = FindObjectOfType<StructureInformation>();
        
        action.AddOnStateDownListener(OnActionDown, SteamVR_Input_Sources.RightHand); // Adding a listener for the triggering action
    }

    void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, transform.forward, out var hit, Single.PositiveInfinity))
        {
            line.SetPosition(1, transform.InverseTransformPoint(hit.point)); // Making the laser snap to that object
            hasReset = false;
            
            // Making sure the object being pointed at is a new one
            if (hitObject == null || (hitObject != null && !hit.transform.gameObject.Equals(hitObject)))
            {
                // If it is, remove the old outline
                if (hitObject != null && !hitObject.Equals(selectedObject))
                {
                    Destroy(hitObject.GetComponent<Outline>());
                }
                
                hitObject = hit.transform.gameObject;
            
                // If it isn't the selected one, add a new outline
                if (!hitObject.Equals(selectedObject))
                {
                    Outline outline = hitObject.AddComponent<Outline>();
                    outline.OutlineColor = selectedMaterial.color;
                    outline.OutlineWidth = 2;
                }
            }
        }
        else
        {
            ResetLaser(); // Resetting when pointing away
        }
    }
    
    
    /// <summary>
    /// Quick method to reset the laser to its null position
    /// </summary>
    void ResetLaser()
    {
        if (!hasReset)
        {
            if (hitObject != null && !hitObject.Equals(selectedObject))
            {
                Destroy(hitObject.GetComponent<Outline>());
            }
            
            line.SetPosition(1, Vector3.forward * length);
            hitObject = null;

            hasReset = true;   
        }
    }

    /// <summary>
    /// Triggers when given action starts on the given controller
    /// </summary>
    /// <param name="fromAction">Action to be anticipated</param>
    /// <param name="fromSource">Controller/hand that performs the action</param>
    void OnActionDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if (hitObject != null && !hitObject.Equals(selectedObject))
        {
            // Removing the old selected object outline
            if (selectedObject != null)
            {
                Destroy(selectedObject.GetComponent<Outline>());
                
                bigBrain.UpdateStructure(selectedObject, false); // Updating the big brain
            }
            
            selectedObject = hitObject;
            
            bigBrain.UpdateStructure(selectedObject, false); // Updating the big brain

            // Adding a unique outline to the selected object
            Outline outline = selectedObject.GetComponent<Outline>();
            outline.OutlineColor = selectedMaterial.color;
            outline.OutlineWidth *= 2f;

            int infoIndex = ArrayUtility.IndexOf(miniBrain.info.Structures, selectedObject);
            string connections = "";

            // Creating a list of other connected structures
            foreach (GameObject i in miniBrain.info.ValidConnections[infoIndex])
            {
                connections += i.name + ", ";
            }

            leftHand.SetUI(miniBrain.info.Structures[infoIndex].name, connections); // Applying the list to the UI
        }
    }
}
