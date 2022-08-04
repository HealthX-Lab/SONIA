using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Valve.VR;

/// <summary>
/// MonoBehaviour to manage the selection and deselection of structures
/// within the mini brain and the structure information UI
/// </summary>
/// <organization>Health-X Lab</organization>
/// <project>Insideout (May-August 2022)</project>
/// <author>Owen Hellum</author>
public class StructureSelection : MonoBehaviour
{
    [Tooltip("The material to be used when a structure is selected, and the default one")]
    public Material defaultMaterial, selectedMaterial;
    [SerializeField, Tooltip(
         "Whether or not to treat the line attached to the controller as a lightsaber" +
         " (otherwise it acts as a laser pointer)"
    )]
    bool isLightsaber = true;
    [SerializeField, Tooltip("The length of the lightsaber")]
    float lightsaberLength = 0.3f;
    [SerializeField, Tooltip("The length of the laser pointer when not pointing towards anything")]
    float laserPointerLength = 0.2f;
    [SerializeField, Tooltip("The triggering action from the right hand controller")]
    SteamVR_Action_Boolean action;

    LineRenderer line; // The laser pointer
    bool hasReset; // Whether the laser pointer has already been reset after pointing away
    // The current objects (structure and menu) being pointed towards and the selected brain object
    GameObject hitObject, hitMenuObject, selectedObject;
    GameObject lastHitMenuObject, selectedMenuObject; // The last menu option pointed towards and selected (respectively)
    LineRenderer[] lastLineSections; // The colour-coded LineRenderers for the last selected connection
    GameObject lastOther; // Structure associated with the last selected connection
    GameObject lastLeft, lastOtherLeft; // The structures on the left side of the brain to have their outlines mirrored

    MiniBrain miniBrain; // The mini brain script
    BigBrain bigBrain; // The big brain script
    StructureInformation structureInformation; // The structure information script in the left hand
    TutorialLoader tutorial; // The tutorial script
    CompletionController completion; // The structure/connection completion script
    bool isInTutorial = true; // Whether or not to pause the selection, as the tutorial text is currently visible
    
    // Rider IDE told me to use this variable instead of using the string name directly, so here it is
    static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        // Creating the laser pointer
        line = gameObject.AddComponent<LineRenderer>();
        line.material = selectedMaterial;
        line.widthMultiplier = 0.005f;
        line.useWorldSpace = false;

        // Setting the lightsaber length
        if (isLightsaber)
        {
            line.SetPosition(1, Vector3.forward * lightsaberLength);   
        }

        ResetLaser();

        // Getting the required scripts
        miniBrain = FindObjectOfType<MiniBrain>();
        bigBrain = FindObjectOfType<BigBrain>();
        structureInformation = FindObjectOfType<StructureInformation>();
        tutorial = FindObjectOfType<TutorialLoader>();
        completion = FindObjectOfType<CompletionController>();

        if (tutorial == null)
        {
            isInTutorial = false;
        }
        
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
            if (
                !isInTutorial
                && hit.transform.IsChildOf(miniBrain.transform)
                && ((isLightsaber && hit.distance <= lightsaberLength) || !isLightsaber))
            {
                if (!isLightsaber)
                {
                    // Making the laser snap to that object
                    line.SetPosition(1, transform.InverseTransformPoint(hit.point));   
                }
                
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
            else if (
                hit.transform.IsChildOf(structureInformation.canvas.transform)
                || (tutorial != null && hit.transform.IsChildOf(tutorial.transform)))
            {
                // Making sure to select only the appropriate UI options at the appropriate times
                if ((isInTutorial && tutorial != null && hit.transform.IsChildOf(tutorial.transform)) || !isInTutorial)
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
                    hitMenuObject = lastHitMenuObject;
                
                    // Adding a new / enabling the outline
                    if (!lastHitMenuObject.GetComponent<Outline>())
                    {
                        Outline outline = lastHitMenuObject.AddComponent<Outline>();
                        outline.OutlineColor = selectedMaterial.color;
                        outline.OutlineWidth = 10;
                        outline.OutlineMode = Outline.Mode.OutlineVisible;
                    }
                    else
                    {
                        lastHitMenuObject.GetComponent<Outline>().enabled = true;
                    }   
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

            float resetLength = lightsaberLength;

            // Making sure to reset to the proper length
            if (!isLightsaber)
            {
                resetLength = laserPointerLength;
            }
            
            line.SetPosition(1, Vector3.forward * resetLength);
            
            hitObject = null;
            hitMenuObject = null;

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

        temp.widthMultiplier = mat.Equals(defaultMaterial) ? 0.001f : 0.005f;
    }

    /// <summary>
    /// Splits a particular LineRenderer on a GameObject into a set of colours
    /// </summary>
    /// <param name="obj">The GameObject to be searched within</param>
    /// <param name="index">The index of the LineRenderer to be set</param>
    /// <param name="cols">the colours to split the LineRenderer by</param>
    /// <param name="addArrow">Whether or not to add a directional arrow midway through the connection line</param>
    /// <param name="useLineWidth">
    /// Whether or not to set the width of the splits
    /// to be equal to the width of the original LineRenderer
    /// </param>
    LineRenderer[] SetLineRendererMaterial(GameObject obj, int index, Color[] cols, bool addArrow, bool useLineWidth)
    {
        return SetLineRendererMaterial(
            obj.transform.GetChild(index).GetComponent<LineRenderer>(),
            cols,
            addArrow,
            3,
            1,
            Vector3.zero,
            useLineWidth
        );
    }

    /// <summary>
    /// Splits a LineRenderer into a set of colours
    /// </summary>
    /// <param name="renderer">The LineRenderer in question to be split</param>
    /// <param name="cols">the colours to split the LineRenderer by</param>
    /// <param name="addArrow">Whether or not to add a directional arrow midway through the connection line</param>
    /// <param name="arrowSize">The scaling size of the arrow (if there is one)</param>
    /// <param name="positionOffset">The offset along the line that the arrow should be positioned by (if there is one)</param>
    /// <param name="rotationOffset">The rotation offset of the arrow (if there is one)</param>
    /// <param name="useLineWidth">
    /// Whether or not to set the width of the splits
    /// to be equal to the width of the original LineRenderer
    /// </param>
    public static LineRenderer[] SetLineRendererMaterial(
        LineRenderer renderer,
        Color[] cols,
        bool addArrow,
        float arrowSize,
        float positionOffset,
        Vector3 rotationOffset,
        bool useLineWidth)
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
            newLineObject.transform.localRotation = Quaternion.identity;
            newLineObject.transform.localPosition = start + (i * dir);
            
            LineRenderer newLine = newLineObject.AddComponent<LineRenderer>(); // Adding a LineRenderer to it
            
            // Setting the section's Subsystem colour
            newLine.material = renderer.material;
            newLine.material.SetColor(EmissionColor, cols[i]);
            newLine.widthMultiplier = useLineWidth ? renderer.widthMultiplier : 0.005f;
            newLine.useWorldSpace = false;
            
            // Setting the section's positions (it's less complex than it looks)
            newLine.SetPositions(
                new [] {
                    Vector3.zero,
                    dir
                }
            );

            lineSections[i] = newLine;

            if (addArrow)
            {
                // Adding an arrow object for each line segment
                GameObject arrow = new GameObject("Arrow");
                arrow.transform.SetParent(newLineObject.transform);
                arrow.transform.localScale = Vector3.one;
                arrow.transform.localPosition = dir / (2f * positionOffset);

                // Getting the arrow's direction and looking there
                Vector3 lookPosition = arrow.transform.position + dir;
                arrow.transform.LookAt(lookPosition);
                
                // Rotating the arrow by the offset (keeping it flat if the arrow is pointing straight up or down)
                if ((int)dir.x == 0)
                {
                    arrow.transform.localEulerAngles += new Vector3(rotationOffset.x, rotationOffset.y, 0);
                }
                else
                {
                    arrow.transform.localEulerAngles += rotationOffset;
                }

                // Adding an arrow LineRenderer to the above object
                LineRenderer arrowLine = arrow.AddComponent<LineRenderer>();
                arrowLine.material = renderer.material;
                arrowLine.material.SetColor(EmissionColor, cols[i]);
                arrowLine.widthMultiplier = useLineWidth ? renderer.widthMultiplier : 0.005f;
                arrowLine.useWorldSpace = false;
                
                arrowLine.positionCount = 3;
                arrowLine.SetPositions(
                    new [] {
                        arrowSize * new Vector3(-1, 0, -1),
                        Vector3.zero,
                        arrowSize * new Vector3(1, 0, -1)
                    }
                );   
            }
        }

        return lineSections; // Returning the newly added sections (for future destruction)
    }

    /// <summary>
    /// Quick method to get the appropriate, AtlasInfo-corresponding structure
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
    /// Method to be called to switch in and out of tutorial mode
    /// </summary>
    public void ToggleTutorial()
    {
        // Making sure that the tutorial canvas exists
        if (tutorial != null)
        {
            if (isInTutorial)
            {
                // If there are remaining tutorial popups in this 'chain' of popups
                if (tutorial.current is 0 or 2 || (tutorial.current >= 4 && tutorial.current <= 9))
                {
                    tutorial.Reset();
                }
                // Otherwise, the tutorial is toggled off
                else
                {
                    tutorial.gameObject.SetActive(false);
                    isInTutorial = false;   
                }
            }
            // The tutorial is toggled on
            else
            {
                tutorial.Reset();
                tutorial.gameObject.SetActive(true);
                isInTutorial = true;
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
            // If the first structure to be selected is selected, toggle on
            if (tutorial != null && tutorial.current == 1)
            {
                ToggleTutorial();
            }
            
            // Destroying the old left side outline
            if (lastLeft != null)
            {
                Destroy(lastLeft.GetComponent<Outline>());
            }
            
            // Destroying the old left side other outline
            if (lastOtherLeft != null && !lastOtherLeft.Equals(lastLeft))
            {
                Destroy(lastOtherLeft.GetComponent<Outline>());
            }
            
            // Hiding the last connection's structure's outline
            if (lastOther != null)
            {
                Destroy(lastOther.GetComponent<Outline>());
                
                // Turning the last selected object's LineRenderers back on
                SetLineRendererVisibility(GetCorrespondingGameObject(selectedObject), true);
                
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

            // Creating a left side outline to mirror the right
            if (miniBrain.ignoreLeft)
            {
                Outline leftOutline = miniBrain.info.LeftStructures[infoIndex].AddComponent<Outline>();
                leftOutline.OutlineMode = Outline.Mode.OutlineVisible;
                leftOutline.OutlineColor = outline.OutlineColor;
                leftOutline.OutlineWidth = outline.OutlineWidth;
            
                lastLeft = leftOutline.gameObject;
            }

            SetLineRendererVisibility(temp, true);
            
            int descriptionInfoIndex = infoIndex;

            if (miniBrain.ignoreLeft)
            {
                descriptionInfoIndex = (descriptionInfoIndex * 2) + 1;
            }

            List<GameObject> connectionsFrom = new List<GameObject>();

            // Getting all the structures that to the selected structure
            for (int k = 0; k < miniBrain.info.Structures.Length; k++)
            {
                if (miniBrain.info.ValidConnections[k].Contains(miniBrain.info.Structures[infoIndex]))
                {
                    connectionsFrom.Add(miniBrain.info.Structures[k]);
                }
            }

            // Applying the list to the UI
            structureInformation.SetUI(
                miniBrain.info.Structures[infoIndex],
                miniBrain.info.Descriptions[descriptionInfoIndex],
                miniBrain.info.ValidConnections[infoIndex].ToArray(),
                connectionsFrom.ToArray()
            );

            yield return new WaitForSeconds(bufferSeconds);
            
            completion.HighlightStructureInDiagram(miniBrain.info.Structures[infoIndex].name);

            bigBrain.UpdateStructure(temp, false, true, true, true); // Updating the big brain
        }
        // Checking if a menu object has been clicked on
        else if (hitMenuObject != null && lastHitMenuObject != null)
        {
            // If the tutorial mode is currently on, toggle off
            if (isInTutorial)
            {
                ToggleTutorial();
            }
            else
            {
                // If the first connection to be selected is selected, toggle on
                if (tutorial != null && tutorial.current == 10)
                {
                    ToggleTutorial();
                }
                
                // Destroying the old left side other outline
                if (lastOtherLeft != null && !lastOtherLeft.Equals(lastLeft))
                {
                    Destroy(lastOtherLeft.GetComponent<Outline>());
                }
                
                // Removing all the last created line sections
                if (lastLineSections != null)
                {
                    foreach (LineRenderer i in lastLineSections)
                    {
                        Destroy(i.gameObject);
                    }   
                }
                
                lastLineSections = null;

                // Hiding the last connection's structure's outline
                if (lastOther != null)
                {
                    Destroy(lastOther.GetComponent<Outline>());
                    
                    // Turning the last selected object's LineRenderers back on
                    SetLineRendererVisibility(lastOther, true);
                    
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
                selectedMenuObject.GetComponent<Outline>().OutlineWidth = 20;
                
                GameObject temp = GetCorrespondingGameObject(selectedObject);
                int selectedIndex = miniBrain.info.IndexOf(temp);

                // Getting the connected structure
                GameObject other = miniBrain.info.Find(
                    lastHitMenuObject.transform.parent.GetComponentInChildren<TMP_Text>().text
                );

                // Resetting the connection line visibility
                SetLineRendererVisibility(temp, true);
                HideOverlappingLineRenderers(other, temp);

                bigBrain.UpdateStructure(other, false, false, true, true);

                // The tag of the ancestor 'connected' UI GameObject (either "To" or "From")
                string parentTag = lastHitMenuObject.transform.parent.parent.parent.tag;

                int otherIndex = miniBrain.info.ValidConnections[selectedIndex].IndexOf(other);
                
                List<Color> colours = new List<Color>();

                if (miniBrain.info.Subsystems != null)
                {
                    // Getting the shared colours between the selected structure and the connected structure
                    foreach (SubsystemInfo k in miniBrain.info.FindSharedSubsystems(temp, other))
                    {
                        colours.Add(k.Colour);
                    }
                }
                // If there are no Subsystems it just highlights it with the default selection colour
                else
                {
                    colours.Add(selectedMaterial.color);
                    
                    ShowOverlappingLineRenderers(temp);
                    
                    SetLineRendererMaterial(
                        temp,
                        selectedMaterial,
                        otherIndex
                    ); 
                }
                
                // Changing the selection targets if the connected structure to be added is connected the other way around
                if (parentTag.Equals("From"))
                {
                    // Making sure to update the appropriate structures
                    HideOverlappingLineRenderers(temp, other);
                    bigBrain.UpdateStructure(temp, false, true, true, true);
                    
                    otherIndex = miniBrain.info.ValidConnections[miniBrain.info.IndexOf(other)].IndexOf(temp);
                    temp = other;
                }

                HighlightStructure(other, (colours.Count == 1) ? colours[0] : selectedMaterial.color);

                // Skipping over the node child if the structures are being replaced with nodes
                if (miniBrain.replaceWithNodes)
                {
                    otherIndex++;
                }
                
                // Making the target's line stand out
                lastLineSections = SetLineRendererMaterial(
                    temp,
                    otherIndex,
                    colours.ToArray(),
                    true,
                    false
                );

                bigBrain.UpdateStructure(other, false, true, true, true);

                yield return new WaitForSeconds(bufferSeconds);

                bigBrain.UpdateStructure(temp, false, true, true, true);

                // Making sure that the connection description exists
                if (miniBrain.info.Subsystems != null
                    && selectedIndex < miniBrain.info.SubsystemConnectionDescriptions.Length)
                {
                    structureInformation.connectionDescription.SetActive(true); // Showing the connection description

                    // Setting the text of the connection description
                    if (parentTag.Equals("To"))
                    {
                        structureInformation.SetConnectionDescription(
                            selectedIndex,
                            miniBrain.info.IndexOf(other)
                        );
                    }
                    else if (parentTag.Equals("From"))
                    {
                        structureInformation.SetConnectionDescription(
                            miniBrain.info.IndexOf(other),
                            selectedIndex
                        );
                    }
                }

                // Carrying over the last highlighted other object so it can be modified when something changes later
                if (parentTag.Equals("To"))
                {
                    lastOther = other;
                }
                else if (parentTag.Equals("From"))
                {
                    lastOther = temp;
                }

                structureInformation.ResetConnections();
                
                completion.HighlightConnectionInDiagram(
                    miniBrain.info.Structures[selectedIndex].name,
                    otherIndex,
                    Color.white
                );
            }
        }
    }
    
    /// <summary>
    /// Adds an outline to the given structure and to the corresponding structure on the left
    /// </summary>
    /// <param name="obj">The GameObject to be highlighted</param>
    /// <param name="col">The colour to be set as the outline</param>
    void HighlightStructure(GameObject obj, Color col)
    {
        // Adding an outline to the connection's structure
        Outline outline = obj.AddComponent<Outline>();
        outline.OutlineMode = Outline.Mode.OutlineVisible;
        outline.OutlineWidth *= 2f;

        outline.OutlineColor = col;
                
        // Creating a left side other outline to mirror the right
        if (miniBrain.ignoreLeft)
        {
            Outline leftOtherOutline = miniBrain.info.LeftStructures[miniBrain.info.IndexOf(obj)].AddComponent<Outline>();
            leftOtherOutline.OutlineMode = Outline.Mode.OutlineVisible;
            leftOtherOutline.OutlineColor = outline.OutlineColor;
            leftOtherOutline.OutlineWidth = outline.OutlineWidth;
            
            lastOtherLeft = leftOtherOutline.gameObject;   
        }
    }
}
