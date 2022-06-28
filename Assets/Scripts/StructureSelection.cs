using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Valve.VR;

public class StructureSelection : MonoBehaviour
{
    [SerializeField, Tooltip("The material to be used when a structure is selected, and the default one")]
    Material defaultMaterial, selectedMaterial;
    [SerializeField, Tooltip("The length of the laser pointer when not pointing towards anything")]
    float length = 0.2f;
    [SerializeField, Tooltip("The triggering action from the right hand controller")]
    SteamVR_Action_Boolean action;

    LineRenderer line; // The laser pointer
    bool hasReset; // Whether the laser pointer has already been reset after pointing away
    GameObject hitObject, selectedObject; // The current object being pointed towards and the clicked object
    GameObject lastHitMenuObject; // The last menu option pointed towards

    MiniBrain miniBrain; // The mini brain script
    BigBrain bigBrain; // The big brain script
    StructureInformation structureInformation; // The structure information script in the left hand

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
        structureInformation = FindObjectOfType<StructureInformation>();
        
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
                        outline.OutlineMode = Outline.Mode.OutlineVisible;
                    }
                }
            }
            // Checking if a menu object is being hit
            else if (hit.transform.IsChildOf(structureInformation.canvas.transform))
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
                    outline.OutlineMode = Outline.Mode.OutlineVisible;
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

        // Making sure it does exist in the other's valid connections
        if (targetIndex != -1)
        {
            // Making sure to skip the first child (the node) if the structures are replaced with nodes
            if (miniBrain.replaceWithNodes)
            {
                targetIndex++;
            }
            
            miniBrain.info.Structures[otherIndex].transform.GetChild(targetIndex).GetComponent<LineRenderer>().enabled = false; // Disabling it
        
            //SetLineRendererMaterial(target, selectedMaterial); // Making the target's line stand out

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
        for (int i = 0; i < obj.GetComponentsInChildren<LineRenderer>().Length; i++)
        {
            SetLineRendererMaterial(obj, mat, i);
        }
    } 

    /// <summary>
    /// Sets a particular LineRenderer on a GameObject to some Material
    /// </summary>
    /// <param name="obj">The GameObject to be searched within</param>
    /// <param name="mat">The material to be applied</param>
    /// <param name="index">The index of the LineRenderer to be set</param>
    void SetLineRendererMaterial(GameObject obj, Material mat, int index)
    {
        LineRenderer temp = obj.GetComponentsInChildren<LineRenderer>()[index];
        temp.material = mat;

        if (mat.Equals(defaultMaterial))
        {
            temp.widthMultiplier = 0.001f;
        }
        else
        {
            temp.widthMultiplier = 0.005f;
        }
    }

    /// <summary>
    /// Quick method to Get the appropriate, AtlasInfo-corresponding structure
    /// </summary>
    /// <param name="selected">The selected GameObject in the mini brain</param>
    /// <returns>Either the selected object itself or its parent (the real structure)</returns>
    GameObject GetCorrespondingGameObject(GameObject selected)
    {
        if (miniBrain.replaceWithNodes)
        {
            return selected.transform.parent.gameObject;
        }
        
        return selected;
    }

    GameObject waitStructure; // Very small temporary variable to hold a structure that's to be updated after some time
    
    /// <summary>
    /// Quick method to wait before updating a structure (to make sure there's time to delete GameObjects, etc.)
    /// </summary>
    void WaitUpdateStructure()
    {
        bigBrain.UpdateStructure(waitStructure, false, false); // Updating the big brain
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
            structureInformation.connectionDescription.SetActive(false); // Hiding the connection description
            
            // Removing the old selected object outline
            if (selectedObject != null)
            {
                Destroy(selectedObject.GetComponent<Outline>());
                
                GameObject lastTemp = GetCorrespondingGameObject(selectedObject);
                
                ShowOverlappingLineRenderers(lastTemp); // Showing the other overlapping connections

                waitStructure = lastTemp;
                Invoke(nameof(WaitUpdateStructure), 0.01f);
                
                foreach (GameObject i in miniBrain.info.ValidConnections[miniBrain.info.IndexOf(lastTemp)])
                {
                    bigBrain.UpdateStructure(i, false, false); // Updating the big brain
                }
            }
            
            selectedObject = hitObject;

            // Adding a unique outline to the selected object
            Outline outline = selectedObject.GetComponent<Outline>();
            outline.OutlineColor = selectedMaterial.color;
            outline.OutlineWidth *= 2f;

            GameObject temp = GetCorrespondingGameObject(selectedObject);
            
            int infoIndex = miniBrain.info.IndexOf(temp);
            
            foreach (GameObject j in miniBrain.info.ValidConnections[infoIndex])
            {
                // Hiding the other overlapping connections
                if (HideOverlappingLineRenderers(j, temp))
                {
                    bigBrain.UpdateStructure(j, false, true); // Updating the big brain
                }
            }
            
            int descriptionInfoIndex = infoIndex;

            if (miniBrain.ignoreLeft)
            {
                descriptionInfoIndex = (descriptionInfoIndex * 2) + 1;
            }

            // Applying the list to the UI
            structureInformation.SetUI(
                miniBrain.info.Structures[infoIndex],
                miniBrain.info.Descriptions[descriptionInfoIndex],
                miniBrain.info.ValidConnections[infoIndex].ToArray()
            );

            bigBrain.UpdateStructure(temp, false, true); // Updating the big brain
        }
        // Checking if a menu object has been clicked on
        else if (lastHitMenuObject != null)
        {
            GameObject temp = GetCorrespondingGameObject(selectedObject);
            int selectedIndex = miniBrain.info.IndexOf(temp);

            // Making sure that the connection description exists
            if (miniBrain.info.SubsystemConnectionDescriptions != null
                && selectedIndex < miniBrain.info.SubsystemConnectionDescriptions.Length)
            {
                structureInformation.connectionDescription.SetActive(true); // Showing the connection description

                int otherIndex = miniBrain.info.IndexOf(miniBrain.info.Find(
                    lastHitMenuObject.transform.parent.GetComponentInChildren<TMP_Text>().text
                ));
                
                int lineIndex = otherIndex; // TODO: this needs to be replaced with the index in the connected structures

                if (miniBrain.replaceWithNodes)
                {
                    lineIndex++;
                }
                
                print(lineIndex);
                
                SetLineRendererMaterial(temp, selectedMaterial, lineIndex); // Making the target's line stand out
            
                // Setting the text of the connection description
                structureInformation.SetConnectionDescription(
                    selectedIndex,
                    otherIndex
                ); 
            }
        }
    }
}
