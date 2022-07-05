using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Valve.VR;

public class StructureSelection : MonoBehaviour
{
    [Tooltip("The material to be used when a structure is selected, and the default one")]
    public Material defaultMaterial, selectedMaterial;
    [SerializeField, Tooltip("The length of the laser pointer when not pointing towards anything")]
    float length = 0.2f;
    [SerializeField, Tooltip("The triggering action from the right hand controller")]
    SteamVR_Action_Boolean action;

    LineRenderer line; // The laser pointer
    bool hasReset; // Whether the laser pointer has already been reset after pointing away
    GameObject hitObject, selectedObject; // The current object being pointed towards and the clicked object
    GameObject lastHitMenuObject, selectedMenuObject; // The last menu option pointed towards
    LineRenderer[] lastLineSections; // The colour-coded LineRenderers for the last selected connection

    MiniBrain miniBrain; // The mini brain script
    BigBrain bigBrain; // The big brain script
    StructureInformation structureInformation; // The structure information script in the left hand

    bool waitShowName; // Temporary variable to hold whether or not to show the big structure's name after some big time
    GameObject waitStructure; // Temporary variable to hold a structure that's to be updated after some time
    
    // Rider IDE told me to use this variable instead of using the string name directly, so here it is
    static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

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
        
        // Adding a listener for the triggering action
        action.AddOnStateDownListener(OnActionDown, SteamVR_Input_Sources.RightHand);
    }

    void FixedUpdate()
    {
        if (Physics.Raycast(
                transform.position, 
                transform.forward,
                out var hit,
                Single.PositiveInfinity
            ))
        {
            // Checking if a structure is being hit
            if (hit.transform.IsChildOf(miniBrain.transform))
            {
                // Making the laser snap to that object
                line.SetPosition(1, transform.InverseTransformPoint(hit.point));
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
                // Making the laser snap to that object
                line.SetPosition(1, transform.InverseTransformPoint(hit.point));
                hasReset = false;

                // Disabling the old outline
                if (lastHitMenuObject != null && !lastHitMenuObject.Equals(selectedMenuObject))
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
            
            if (lastHitMenuObject != null && !lastHitMenuObject.Equals(selectedMenuObject))
            {
                lastHitMenuObject.GetComponent<Outline>().enabled = false;
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
    void HideOverlappingLineRenderers(GameObject other, GameObject target)
    {
        int otherIndex = miniBrain.info.IndexOf(other); // Index of the other GameObject in the whole structure array
        
        // Index of the target GameObject in the other GameObject's valid connections
        int targetIndex = miniBrain.info.ValidConnections[otherIndex].IndexOf(target);

        // Making sure it does exist in the other's valid connections
        if (targetIndex != -1)
        {
            // Making sure to skip the first child (the node) if the structures are replaced with nodes
            if (miniBrain.replaceWithNodes)
            {
                targetIndex++;
            }
            
            // Disabling it
            miniBrain.info.Structures[otherIndex].transform.GetChild(targetIndex)
                .GetComponent<LineRenderer>().enabled = false;
        }
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
            SetLineRendererVisibility(i, true);
        }
    }

    /// <summary>
    /// Quick method to set the 'enabled' property of all LineRenderers in the children of a GameObject
    /// </summary>
    /// <param name="obj">The GameObject to be checked</param>
    /// <param name="visibility">the visibility state to set the LineRenderers to</param>
    void SetLineRendererVisibility(GameObject obj, bool visibility)
    {
        foreach (LineRenderer i in obj.GetComponentsInChildren<LineRenderer>())
        {
            i.enabled = visibility;
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
    /// Splits a particular LineRenderer on a GameObject into a set of colours
    /// </summary>
    /// <param name="obj">The GameObject to be searched within</param>
    /// <param name="cols">the colours to split the LineRenderer by</param>
    /// <param name="index">The index of the LineRenderer to be set</param>
    /// <param name="isLocal">Whether or not the LineRenderer(s) use local positioning</param>
    LineRenderer[] SetLineRendererMaterial(GameObject obj, Color[] cols, int index, bool isLocal)
    {
        return SetLineRendererMaterial(obj.transform.GetChild(index).GetComponent<LineRenderer>(), cols, isLocal);
    }

    /// <summary>
    /// Splits a LineRenderer into a set of colours
    /// </summary>
    /// <param name="cols">the colours to split the LineRenderer by</param>
    /// <param name="isLocal">Whether or not the LineRenderer(s) use local positioning</param>
    public static LineRenderer[] SetLineRendererMaterial(LineRenderer renderer, Color[] cols, bool isLocal)
    {
        LineRenderer[] lineSections = new LineRenderer[cols.Length];
        
        renderer.enabled = false;
        
        // The initial local position and direction
        Vector3 start = renderer.GetPosition(0);
        Vector3 dir = (renderer.GetPosition(1) - start) / cols.Length;
        
        for (int i = 0; i < cols.Length; i++)
        {
            // Creating a new line section/segment object as a child of the child LineRenderer
            GameObject newLineObject = new GameObject("Line Section");
            newLineObject.transform.SetParent(renderer.transform);
            newLineObject.transform.localPosition = Vector3.zero;
            newLineObject.transform.localScale = Vector3.one;
            
            LineRenderer newLine = newLineObject.AddComponent<LineRenderer>(); // Adding a LineRenderer to it
            
            // Setting the section's Subsystem colour
            newLine.material = renderer.material;
            newLine.material.SetColor(EmissionColor, cols[i]);
            newLine.widthMultiplier = 0.005f;
            
            if (isLocal)
            {
                newLine.useWorldSpace = false;
            }
            
            // Setting the section's positions (it's less complex than it looks)
            newLine.SetPositions(
                new [] {
                    start + (i * dir),
                    start + ((i+1) * dir)
                }
            );

            lineSections[i] = newLine;
        }

        return lineSections; // Returning the newly added sections (for future destruction)
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

    /// <summary>
    /// Quick method to wait before updating a structure (to make sure there's time to delete GameObjects, etc.)
    /// </summary>
    void WaitUpdateStructure()
    {
        UpdateStructure(waitStructure, true, waitShowName);
    }

    /// <summary>
    /// Quick method to update a structure in the big brain
    /// </summary>
    /// <param name="obj">The mini brain structure to be copied into the big brain</param>
    /// <param name="checkAndAddOutline">
    /// Whether or not to add an outline to the structure
    /// (if the structures in the mini brain are replaced by nodes)
    /// </param>
    /// <param name="showName">Whether or not to show the structure's name on the structure when selected</param>
    void UpdateStructure(GameObject obj, bool checkAndAddOutline, bool showName)
    {
        bigBrain.UpdateStructure(obj, false, checkAndAddOutline, showName);
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

                waitShowName = false;
                waitStructure = lastTemp;
                Invoke(nameof(WaitUpdateStructure), 0.01f);
                
                foreach (GameObject i in miniBrain.info.ValidConnections[miniBrain.info.IndexOf(lastTemp)])
                {
                    bigBrain.UpdateStructure(i, false, false, false); // Updating the big brain
                }
            }
            
            selectedObject = hitObject;

            // Adding a unique outline to the selected object
            Outline outline = selectedObject.GetComponent<Outline>();
            outline.OutlineColor = selectedMaterial.color;
            outline.OutlineWidth *= 2f;

            GameObject temp = GetCorrespondingGameObject(selectedObject);
            int infoIndex = miniBrain.info.IndexOf(temp);

            SetLineRendererVisibility(temp, true);
            
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

            bigBrain.UpdateStructure(temp, false, true, true); // Updating the big brain
        }
        // Checking if a menu object has been clicked on
        else if (lastHitMenuObject != null)
        {
            // Disabling the old outline
            if (selectedMenuObject != null)
            {
                selectedMenuObject.GetComponent<Outline>().enabled = false;
            }
            
            // Confirming the menu selection
            selectedMenuObject = lastHitMenuObject;
            selectedMenuObject.GetComponent<Outline>().OutlineWidth = 16;
            
            GameObject temp = GetCorrespondingGameObject(selectedObject);
            int selectedIndex = miniBrain.info.IndexOf(temp);
            
            // Removing all the last created line sections
            if (lastLineSections != null)
            {
                foreach (LineRenderer j in lastLineSections)
                {
                    Destroy(j.gameObject);
                }   
            }
                
            structureInformation.connectionDescription.SetActive(true); // Showing the connection description

            GameObject other = miniBrain.info.Find(
                lastHitMenuObject.transform.parent.GetComponentInChildren<TMP_Text>().text
            );

            // Resetting the connection line visibility
            SetLineRendererVisibility(temp, true);
            HideOverlappingLineRenderers(other, temp);

            UpdateStructure(other, false, true);

            int otherIndex = miniBrain.info.ValidConnections[selectedIndex].IndexOf(other);

            if (miniBrain.info.Subsystems != null)
            {
                List<Color> colours = new List<Color>();
                
                // getting the shared colours between the selected structure and the connected structure
                foreach (SubsystemInfo i in miniBrain.info.FindSharedSubsystems(temp, other))
                {
                    colours.Add(i.Colour);
                }
                
                if (miniBrain.replaceWithNodes)
                {
                    otherIndex++;
                }

                // Making the target's line stand out
                lastLineSections = SetLineRendererMaterial(
                    temp,
                    colours.ToArray(),
                    otherIndex,
                    true
                );   
            }
            // If there are no Subsystem's it just highlights it with the default selection colour
            else
            {
                ShowOverlappingLineRenderers(temp);
                
                SetLineRendererMaterial(
                    temp,
                    selectedMaterial,
                    otherIndex
                ); 
            }

            waitShowName = true;
            waitStructure = temp;
            Invoke(nameof(WaitUpdateStructure), 0.01f);

            // Making sure that the connection description exists
            if (miniBrain.info.SubsystemConnectionDescriptions != null
                && selectedIndex < miniBrain.info.SubsystemConnectionDescriptions.Length)
                // TODO: make sure that the connection description does exist
            {
                // Setting the text of the connection description
                structureInformation.SetConnectionDescription(
                    selectedIndex,
                    miniBrain.info.IndexOf(other)
                ); 
            }
        }
    }
}
