using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Valve.VR;

public class StructureSelection : MonoBehaviour
{
    [Tooltip("The material to be used when a structure is selected, and the default one")]
    [SerializeField] Material defaultMaterial, selectedMaterial;
    [Tooltip("The length of the laser pointer when not pointing towards anything")]
    [SerializeField] float length = 0.2f;
    [Tooltip("The triggering action from the right hand controller")]
    [SerializeField] SteamVR_Action_Boolean action;

    LineRenderer line; // The laser pointer
    bool hasReset; // Whether the laser pointer has already been reset after pointing away
    GameObject hitObject, selectedObject; // The current object being pointed towards and the clicked object
    GameObject lastHitMenuObject; // The last menu option pointed towards

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
            // Checking if a structure is being hit
            if (hit.transform.IsChildOf(miniBrain.transform))
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
            // Checking if a menu object is being hit
            else if (hit.transform.IsChildOf(leftHand.canvas.transform))
            {
                line.SetPosition(1, transform.InverseTransformPoint(hit.point)); // Making the laser snap to that object
                hasReset = false;

                // Disabling the old outline
                if (lastHitMenuObject != null && lastHitMenuObject.GetComponent<Outline>())
                {
                    lastHitMenuObject.GetComponent<Outline>().enabled = false;
                }

                lastHitMenuObject = hit.transform.gameObject;
                
                // Adding a new / enabling the outline
                if (!lastHitMenuObject.GetComponent<Outline>())
                {
                    Outline outline = lastHitMenuObject.AddComponent<Outline>();
                    outline.OutlineColor = selectedMaterial.color;
                    outline.OutlineWidth = 8;
                }
                else
                {
                    lastHitMenuObject.GetComponent<Outline>().enabled = true;
                }
            }
            else
            {
                ResetLaser();
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
    /// Hides any connections that the other GameObject has that go to the current/target GameObject
    /// </summary>
    /// <param name="other"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    bool HideOverlappingLineRenderers(GameObject other, GameObject target)
    {
        int otherIndex = miniBrain.info.IndexOf(other); // Index of the other GameObject in the whole structure array
        int targetIndex = miniBrain.info.ValidConnections[otherIndex].IndexOf(target); // Index of the target GameObject in the other GameObject's valid connections

        // Making sure it does exist in teh other's valid connections
        if (targetIndex != -1)
        {
            miniBrain.info.Structures[otherIndex].transform.GetChild(targetIndex).GetComponent<LineRenderer>().enabled = false; // Disabling it
        
            SetLineRendererMaterial(target, selectedMaterial); // Making the target's line stand out

            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Resets the LineRenderer material and shows of the structure's connections
    /// </summary>
    /// <param name="target">The GameObject that's connections will be shown</param>
    void ShowOverlappingLineRenderers(GameObject target)
    {
        SetLineRendererMaterial(target, defaultMaterial); // Resetting the target's LineRenderers to default
        
        // Showing all the connections again
        foreach (GameObject i in miniBrain.info.ValidConnections[miniBrain.info.IndexOf(target)])
        {
            foreach (LineRenderer j in i.GetComponentsInChildren<LineRenderer>())
            {
                j.enabled = true;
            }
        }
    }

    /// <summary>
    /// Sets all of the LineRenderers on a GameObject to some Material
    /// </summary>
    /// <param name="obj">The GameObject to be searched within</param>
    /// <param name="mat">The material to be applied</param>
    void SetLineRendererMaterial(GameObject obj, Material mat)
    {
        foreach (LineRenderer i in obj.GetComponentsInChildren<LineRenderer>())
        {
            i.material = mat;

            // Making it thick if the Material is the selected one
            if (mat.Equals(selectedMaterial))
            {
                i.widthMultiplier = 0.005f;
            }
            // Otherwise returning it to default thickness
            else if (mat.Equals(defaultMaterial))
            {
                i.widthMultiplier = 0.001f;
            }
        }
    }

    /// <summary>
    /// Triggers when given action starts on the given controller
    /// </summary>
    /// <param name="fromAction">Action to be anticipated</param>
    /// <param name="fromSource">Controller/hand that performs the action</param>
    void OnActionDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        // Checking if a structure has been clicked on
        if (hitObject != null && !hitObject.Equals(selectedObject))
        {
            leftHand.connectionDescription.SetActive(false); // Hiding the connection description
            
            int infoIndex;
            
            // Removing the old selected object outline
            if (selectedObject != null)
            {
                Destroy(selectedObject.GetComponent<Outline>());
                
                ShowOverlappingLineRenderers(selectedObject); // Showing the other overlapping connections
                
                bigBrain.UpdateStructure(selectedObject, false); // Updating the big brain
                
                infoIndex = miniBrain.info.IndexOf(selectedObject);
                
                foreach (GameObject i in miniBrain.info.ValidConnections[infoIndex])
                {
                    bigBrain.UpdateStructure(i, false); // Updating the big brain
                }
            }
            
            selectedObject = hitObject;

            // Adding a unique outline to the selected object
            Outline outline = selectedObject.GetComponent<Outline>();
            outline.OutlineColor = selectedMaterial.color;
            outline.OutlineWidth *= 2f;
            
            infoIndex = miniBrain.info.IndexOf(selectedObject);
            
            foreach (GameObject i in miniBrain.info.ValidConnections[infoIndex])
            {
                // Hiding the other overlapping connections
                if (HideOverlappingLineRenderers(i, selectedObject))
                {
                    bigBrain.UpdateStructure(i, false); // Updating the big brain
                }
            }

            // Applying the list to the UI
            leftHand.SetUI(
                miniBrain.info.Structures[infoIndex].name,
                miniBrain.info.Descriptions[infoIndex],
                miniBrain.info.ValidConnections[infoIndex].ToArray()
            );
            
            bigBrain.UpdateStructure(selectedObject, false); // Updating the big brain
        }
        // Checking if a menu object has been clicked on
        else if (lastHitMenuObject != null)
        {
            leftHand.connectionDescription.SetActive(true); // Showing the connection description
            
            // Setting the text of teh connection description
            leftHand.SetConnectionDescription(miniBrain.info.ConnectionDescriptions[
                miniBrain.info.IndexOf(selectedObject),
                miniBrain.info.IndexOf(miniBrain.info.Find(lastHitMenuObject.transform.parent.GetComponentInChildren<TMP_Text>().text))]
            );
        }
    }
}
