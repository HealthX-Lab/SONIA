using System;
using System.Collections;
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
    GameObject lastHitMenuObject, selectedMenuObject; // The last menu option pointed towards and selected (respectively)
    LineRenderer[] lastLineSections; // The colour-coded LineRenderers for the last selected connection
    GameObject lastOther; // Structure associated with the last selected connection

    MiniBrain miniBrain; // The mini brain script
    BigBrain bigBrain; // The big brain script
    StructureInformation structureInformation; // The structure information script in the left hand
    
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
    LineRenderer[] SetLineRendererMaterial(GameObject obj, Color[] cols, int index)
    {
        return SetLineRendererMaterial(obj.transform.GetChild(index).GetComponent<LineRenderer>(), cols);
    }

    /// <summary>
    /// Splits a LineRenderer into a set of colours
    /// </summary>
    /// <param name="renderer">The LineRenderer in question to be split</param>
    /// <param name="cols">the colours to split the LineRenderer by</param>
    public static LineRenderer[] SetLineRendererMaterial(LineRenderer renderer, Color[] cols)
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
            newLineObject.transform.localScale = Vector3.one;
            newLineObject.transform.localPosition = start + (i * dir);
            
            LineRenderer newLine = newLineObject.AddComponent<LineRenderer>(); // Adding a LineRenderer to it
            
            // Setting the section's Subsystem colour
            newLine.material = renderer.material;
            newLine.material.SetColor(EmissionColor, cols[i]);
            newLine.widthMultiplier = 0.005f;
            newLine.useWorldSpace = false;
            
            // Setting the section's positions (it's less complex than it looks)
            newLine.SetPositions(
                new [] {
                    Vector3.zero,
                    dir
                }
            );

            lineSections[i] = newLine;

            // Adding an arrow object for each line segment
            GameObject arrow = new GameObject("Arrow");
            arrow.transform.SetParent(newLineObject.transform);
            arrow.transform.localScale = Vector3.one;
            arrow.transform.localPosition = dir / 2f;
            arrow.transform.LookAt(arrow.transform.position + dir);

            // Adding an arrow LineRenderer to the above object
            LineRenderer arrowLine = arrow.AddComponent<LineRenderer>();
            arrowLine.material = renderer.material;
            arrowLine.material.SetColor(EmissionColor, cols[i]);
            arrowLine.widthMultiplier = 0.005f;
            arrowLine.useWorldSpace = false;

            float arrowSize = 3; // Scale factor for the arrow

            arrowLine.positionCount = 3;
            arrowLine.SetPositions(
                new [] {
                    arrowSize * new Vector3(-1, 0, -1),
                    Vector3.zero,
                    arrowSize * new Vector3(1, 0, -1)
                }
            );
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
    /// Triggers when given action starts on the given controller
    /// </summary>
    /// <param name="fromAction">Action to be anticipated</param>
    /// <param name="fromSource">Controller/hand that performs the action</param>
    void OnActionDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        StartCoroutine(OnActionDownCoroutine());
    }

    /// <summary>
    /// Coroutine to manage controller selections (with pauses to ensure that structure/components are deleted properly)
    /// </summary>
    IEnumerator OnActionDownCoroutine()
    {
        float bufferSeconds = 0.01f;

        // Checking if a structure has been clicked on
        if (hitObject != null && !hitObject.Equals(selectedObject))
        {
            // Hiding the last connection's structure's outline
            if (lastOther != null)
            {
                Destroy(lastOther.GetComponent<Outline>());
                yield return new WaitForSeconds(bufferSeconds);
                bigBrain.UpdateStructure(lastOther, false, false, false, false);
            }
            
            structureInformation.connectionDescription.SetActive(false); // Hiding the connection description
            
            // Removing the old selected object outline
            if (selectedObject != null)
            {
                // Removing all the last created line sections
                if (lastLineSections != null)
                {
                    foreach (LineRenderer i in lastLineSections)
                    {
                        Destroy(i.gameObject);
                    }   
                }

                lastLineSections = null;
                
                Destroy(selectedObject.GetComponent<Outline>());
                
                GameObject lastTemp = GetCorrespondingGameObject(selectedObject);
                
                ShowOverlappingLineRenderers(lastTemp); // Showing the other overlapping connections

                yield return new WaitForSeconds(bufferSeconds);
                bigBrain.UpdateStructure(lastTemp, false, false, false, false);

                foreach (GameObject j in miniBrain.info.ValidConnections[miniBrain.info.IndexOf(lastTemp)])
                {
                    // Updating the big brain
                    bigBrain.UpdateStructure(j, false, false, false, false);
                }
            }
            
            selectedObject = hitObject;

            // Adding a unique outline to the selected object
            Outline outline = selectedObject.GetComponent<Outline>();
            outline.OutlineMode = Outline.Mode.OutlineVisible;
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

            bigBrain.UpdateStructure(temp, false, true, true, true); // Updating the big brain
        }
        // Checking if a menu object has been clicked on
        else if (lastHitMenuObject != null)
        {
            // Hiding the last connection's structure's outline
            if (lastOther != null)
            {
                Destroy(lastOther.GetComponent<Outline>());
                yield return new WaitForSeconds(bufferSeconds);
                bigBrain.UpdateStructure(lastOther, false, false, false, false);
            }
            
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
            
            lastLineSections = null;
                
            structureInformation.connectionDescription.SetActive(true); // Showing the connection description

            // Getting the connected structure
            GameObject other = miniBrain.info.Find(
                lastHitMenuObject.transform.parent.GetComponentInChildren<TMP_Text>().text
            );

            // Resetting the connection line visibility
            SetLineRendererVisibility(temp, true);
            HideOverlappingLineRenderers(other, temp);

            bigBrain.UpdateStructure(other, false, false, true, true);

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
                    otherIndex
                );
                
                // Adding an outline to the connection's structure
                Outline outline = other.AddComponent<Outline>();
                outline.OutlineMode = Outline.Mode.OutlineVisible;
                outline.OutlineWidth *= 2f;

                if (colours.Count == 1)
                {
                    outline.OutlineColor = colours[0];
                }
                else
                {
                    outline.OutlineColor = selectedMaterial.color;
                }
                
                bigBrain.UpdateStructure(other, false, true, true, true);
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

            yield return new WaitForSeconds(bufferSeconds);
            bigBrain.UpdateStructure(temp, false, true, true, true);

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

            lastOther = other;
        }
    }
}
