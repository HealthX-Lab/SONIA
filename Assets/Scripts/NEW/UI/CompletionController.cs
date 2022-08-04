using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// MonoBehaviour to manage the calculation and updating of the completion UI
/// </summary>
/// <organization>Health-X Lab</organization>
/// <project>Insideout (May-August 2022)</project>
/// <author>Owen Hellum</author>
public class CompletionController : MonoBehaviour
{
    [Tooltip("The GridLayoutGroup GameObjects where the completion info will be listed")]
    public Transform structureLayout, subsystemLayout;
    [SerializeField, Tooltip("The specified order of indices that the structures should be displayed in in the completion UI")]
    int[] specificStructureOrder;

    MiniBrain miniBrain; // The mini brain script
    public StructureCompletion[] structureCompletion; // The % amount (0-1) that each structure has been completed to
    float[] subsystemCompletion; // The % amount (0-1) that each Subsystem has been completed to
    // The spawnable structure and Subsystem completion info objects
    GameObject structureCompletionInfoObject, subsystemCompletionInfoObject;
    // Current number of structures and Subsystem connections viewed (as well as total number of possible connections)
    int numberOfStructuresViewed, numberOfConnectionsViewed, numberOfConnections;
    // Whether or not all the structures have been viewed / all connections have been viewed
    [HideInInspector] public bool hasFinishedStructureSelection, hasFinishedConnectionSelection;
    bool hasHiddenStructureLayout; // Whether or not the structure information UI has been hidden yet
    // Whether to force the structures to have to all be viewed first, before the subsystems
    [HideInInspector] public bool structureSelectionFirst;
    GameObject lastHighlightedStructureInDiagram; // The structure box in the diagram UI that was last highlighted

    /// <summary>
    /// Struct with information about the completion amounts for a structure
    /// </summary>
    public struct StructureCompletion
    {
        /// <summary>
        /// The float value of the completion amount (0-1)
        /// </summary>
        public List<float> SubsystemCompletion { get; set; }
        
        /// <summary>
        /// Whether the structure has been viewed
        /// </summary>
        public bool ViewedStructure { get; set; }
        
        /// <summary>
        /// Which of the structure's connections have been viewed
        /// </summary>
        public bool[] ViewedConnections { get; set; }
        
        /// <summary>
        /// Quick method to calculate the total completion of the structure
        /// </summary>
        /// <param name="structuresFirst">
        /// Whether or not the completion is in stages
        /// (see structureSelectionFirst)
        /// </param>
        /// <returns>A calculated [0..1] completion percentage value</returns>
        public float Completion(bool structuresFirst)
        {
            float temp = 0;

            // Automatically completing the structure if it has been viewed (if the completion is in stages)
            if (structuresFirst)
            {
                if (ViewedStructure)
                {
                    temp = 1;
                }
            }
            else
            {
                // Adds all the subsystem percentages, if they exist
                if (SubsystemCompletion != null)
                {
                    temp += SubsystemCompletion.Sum();
                }

                // Adds a percentage for viewing the structure
                if (ViewedStructure)
                {
                    temp += 1f / (SubsystemCompletion.Count + 1);
                }   
            }

            return temp;
        }
    }

    /// <summary>
    /// External, manually-called Start method
    /// </summary>
    /// <param name="structureSelectionFirst">
    /// Whether to force the structures to have to all be viewed first,
    /// before the subsystems
    /// </param>
    public void CompletionStart(bool structureSelectionFirst)
    {
        structureCompletionInfoObject = Resources.Load<GameObject>("Structure");
        subsystemCompletionInfoObject = Resources.Load<GameObject>("Subsystem");
        
        this.structureSelectionFirst = structureSelectionFirst;

        // Moving the structure completion UI up if the completion is in stages
        if (structureSelectionFirst)
        {
            Transform layoutTransform = structureLayout.parent;
            
            layoutTransform.localPosition = new Vector3(
                layoutTransform.localPosition.x,
                subsystemLayout.parent.localPosition.y,
                layoutTransform.localPosition.z
            );
        }

        // Populating the structure UI order with default values if it's empty
        if (specificStructureOrder == null)
        {
            specificStructureOrder = new int[structureCompletion.Length];

            for (int i = 0; i < specificStructureOrder.Length; i++)
            {
                specificStructureOrder[i] = i;
            }
        }
    }

    /// <summary>
    /// Public method to generate both the structure and Subsystem completion info
    /// </summary>
    public void GenerateCompletionInfo()
    {
        // Makes sure that variables are set, and old children are removed
        CheckMiniBrain();

        if (!hasHiddenStructureLayout)
        {
            DestroyAllChildren(structureLayout);
            GenerateStructureCompletionInfo();

            // Making sure to move the subsystem UI after the structure selection has been completed
            if (structureSelectionFirst && hasFinishedStructureSelection)
            {
                subsystemLayout.parent.localPosition += new Vector3(0.2f, -1.5f, 0);
                hasHiddenStructureLayout = true;
            }
        }
        
        // Only generating the subsystem info once its stage has been reached
        if ((structureSelectionFirst && hasFinishedStructureSelection) || !structureSelectionFirst)
        {
            DestroyAllChildren(subsystemLayout);
            
            if (miniBrain.info.Subsystems != null)
            {
                GenerateSubsystemCompletionInfo();
            }
        }
        
        // Changing the completion UIs' title text if they are being done in stages
        // to reflect the remaining structures/connections to view
        if (structureSelectionFirst)
        {
            CalculateNumberOfConnectionsViewed();
            
            structureLayout.parent.gameObject.GetComponentInChildren<TMP_Text>().text =
                "Structures viewed (" + numberOfStructuresViewed + "/" + miniBrain.info.Structures.Length + "):";
            subsystemLayout.parent.gameObject.GetComponentInChildren<TMP_Text>().text =
                "Connections viewed (" + numberOfConnectionsViewed + "/" + numberOfConnections + "):";
            
            // If all the structures have been viewed, the connection completion stage starts
            if (!hasFinishedConnectionSelection
                && structureSelectionFirst
                && numberOfConnectionsViewed >= numberOfConnections)
            {
                hasFinishedConnectionSelection = true;
                
                FindObjectOfType<StructureSelection>().ToggleTutorial();
            }
        }
    }

    /// <summary>
    /// Checks to see if the mini brain and completion arrays have been set
    /// </summary>
    void CheckMiniBrain()
    {
        if (miniBrain == null)
        {
            miniBrain = FindObjectOfType<MiniBrain>();
            structureCompletion = new StructureCompletion[miniBrain.info.Structures.Length];
            numberOfConnections = 0;

            // Setting the lengths for the structure completion
            for (int i = 0; i < structureCompletion.Length; i++)
            {
                structureCompletion[i].ViewedConnections = new bool[miniBrain.info.ValidConnections[i].Count];
                
                // Updating the total number of connections in the atlas
                numberOfConnections += miniBrain.info.ValidConnections[i].Count;
            }

            // Setting the length for the Subsystem completion
            if (miniBrain.info.Subsystems != null)
            {
                subsystemCompletion = new float[miniBrain.info.Subsystems.Length];
            }
        }
    }

    /// <summary>
    /// Quick method to destroy all the children of a Transform
    /// </summary>
    /// <param name="parent">The Transform parent to be made child-less</param>
    void DestroyAllChildren(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// Method to generate/update the structure completion info
    /// </summary>
    void GenerateStructureCompletionInfo()
    {
        numberOfStructuresViewed = 0;
        
        // Adding a completion info object for each structure
        for (int i = 0; i < miniBrain.info.Structures.Length; i++)
        {
            // Only showing those which have some completion started
            if (structureCompletion[specificStructureOrder[i]].Completion(structureSelectionFirst) > 0)
            {
                GameObject temp = Instantiate(structureCompletionInfoObject, structureLayout);
                temp.name = miniBrain.info.Structures[specificStructureOrder[i]].name;

                TMP_Text tempText = temp.GetComponentInChildren<TMP_Text>();
                string txt = temp.name;

                // Repositioning the structure's name in the completion UI to be larger if it's being done in stages
                if (structureSelectionFirst)
                {
                    tempText.fontSize = 0.1f;
                    tempText.transform.localPosition = Vector3.down * 0.025f;
                }
                // Otherwise, adding a % to the name
                else
                {
                    txt += ": " + Mathf.RoundToInt(
                        structureCompletion[specificStructureOrder[i]].Completion(structureSelectionFirst) * 100
                    ) + "%";
                }

                tempText.text = txt;

                // Adding colour-coded Subsystem pips and/or colour bars for each structure
                // if the completion isn't being done in stages
                if (!structureSelectionFirst && miniBrain.info.Subsystems != null)
                {
                    Instantiate(Resources.Load<GameObject>("ColourBars"), temp.transform)
                        .GetComponent<ColourBars>().AddBars(miniBrain.info.Structures[i], Vector3.down * 0.15f);
                }

                numberOfStructuresViewed++;
            }
        }

        // If all the structures have been viewed, the connection completion stage starts
        if (structureSelectionFirst && numberOfStructuresViewed >= miniBrain.info.Structures.Length)
        {
            hasFinishedStructureSelection = true;
        }
        
        Invoke(nameof(WaitSetConnections), 0.1f);

        // Only checking and changing visibility if the stage hasn't changed yet
        if (!hasHiddenStructureLayout)
        {
            // Showing the structure completion UI only when there's at least one structure underway
            if (structureLayout.childCount == 0)
            {
                structureLayout.parent.gameObject.SetActive(false);
            }
            else
            {
                structureLayout.parent.gameObject.SetActive(true);
            }   
        }
    }

    /// <summary>
    /// Method to generate connections between structures in the completion UI
    /// (to be invoked after the structures have been added to the LayoutGroup and at least 1 frame has passed)
    /// </summary>
    void WaitSetConnections()
    {
        // Checking each structure in the UI with each other structure
        for (int i = 0; i < structureLayout.childCount; i++)
        {
            for (int j = 0; j < structureLayout.childCount; j++)
            {
                string iName = structureLayout.GetChild(i).name;
                string jName = structureLayout.GetChild(j).name;
                
                // Making sure that there is a connection from one to the other
                if (miniBrain.info.ValidConnections[miniBrain.info.IndexOf(iName)]
                    .Contains(miniBrain.info.Find(jName)))
                {
                    // Getting their shared Subsystems
                    SubsystemInfo[] subs = miniBrain.info.FindSharedSubsystems(iName, jName);

                    // Making sure that they do have Subsystems in common
                    if (subs.Length > 0)
                    {
                        Color col;
                        
                        // Getting the colour from the smallest Subsystem from among shared Subsystems
                        if (subs.Length > 1)
                        {
                            int smallest = 0;

                            for (int k = 0; k < subs.Length; k++)
                            {
                                if (subs[k].ValidStructures.Count < subs[smallest].ValidStructures.Count)
                                {
                                    smallest = k;
                                }
                            }
                            
                            col = subs[smallest].Colour;
                        }
                        // Otherwise, getting the only colour
                        else
                        {
                            col = subs[0].Colour;
                        }

                        // Adding a new LineRenderer object
                        Transform newLine = new GameObject("Connection to " + structureLayout.GetChild(j).name).transform;
                        newLine.SetParent(structureLayout.GetChild(i));
                        newLine.localPosition = Vector3.forward * 0.04f;
                        newLine.localRotation = Quaternion.identity;
                        newLine.localScale = Vector3.one;
                        
                        // Adding and setting the LineRenderer's attributes
                        LineRenderer renderer = newLine.gameObject.AddComponent<LineRenderer>();
                        renderer.material = miniBrain.connectionMaterial;
                        renderer.widthMultiplier = 0.01f;
                        renderer.useWorldSpace = false;
                        
                        renderer.SetPositions(
                            new [] {
                                Vector3.zero,
                                structureLayout.GetChild(i).InverseTransformPoint(structureLayout.GetChild(j).position)
                            }
                        );

                        // Setting the LineRenderer's colours to the shared colours (and transforming the arrow)
                        StructureSelection.SetLineRendererMaterial(
                            renderer, 
                            new [] { col },
                            true,
                            0.1f,
                            0.7f,
                            new Vector3(0, -20, 90),
                            true
                        );   
                    }   
                }
            }
        }
    }
    
    /// <summary>
    /// Method to generate/update the Subsystem completion info
    /// </summary>
    void GenerateSubsystemCompletionInfo()
    {
        // Adding a completion info object for each Subsystem
        for (int i = 0; i < miniBrain.info.Subsystems.Length; i++)
        {
            // Only showing those which have some completion started
            if (subsystemCompletion[i] > 0)
            {
                TMP_Text temp = Instantiate(subsystemCompletionInfoObject, subsystemLayout)
                    .GetComponentInChildren<TMP_Text>();

                // Setting the name so it can be retrieved later
                temp.transform.parent.name = miniBrain.info.Subsystems[i].Name;

                // TODO: add a toggle to view the Subsystem description?
                temp.text =
                    miniBrain.info.Subsystems[i].Name // Name
                    + ": " + Mathf.RoundToInt(subsystemCompletion[i] * 100) + "%"; // Completion amount
                    //+ "\n\n" + miniBrain.info.Subsystems[i].Description; // Subsystem description

                //temp.color = miniBrain.info.Subsystems[i].Colour;
                temp.color = FindObjectOfType<StructureSelection>().selectedMaterial.color;

                Color newColour = miniBrain.info.Subsystems[i].Colour;
                MeshRenderer tempRenderer = temp.transform.parent.GetComponentInChildren<MeshRenderer>();
                
                StandardShaderUtils.ChangeRenderMode(tempRenderer.material, StandardShaderUtils.BlendMode.Transparent);
                tempRenderer.material.color = new Color(
                    newColour.r,
                    newColour.g,
                    newColour.b,
                    0.5f
                );
                
                // Adding a new percentage completion bar for each Subsystem in the UI
                GameObject colourBar = Instantiate(Resources.Load<GameObject>("ColourBars"), temp.transform.parent);
                colourBar.transform.localScale *= 0.7f;
                
                // Setting the bar to be one colour, and equal to the Subsystem's total completion amount
                colourBar.GetComponent<ColourBars>().SetValues(
                    new []
                    {
                        miniBrain.info.Subsystems[i].Colour
                    }, 
                    new []
                    {
                        subsystemCompletion[i]
                    },
                    Vector3.down * 0.075f,
                    false
                );
            }
        }

        // Showing the Subsystem completion UI only when there's at least one Subsystem underway
        if (subsystemLayout.childCount == 0)
        {
            subsystemLayout.parent.gameObject.SetActive(false);
        }
        else
        {
            subsystemLayout.parent.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Updates that a structure has been viewed
    /// </summary>
    /// <param name="selectedIndex">The index of the selected structure in the AtlasInfo</param>
    public void UpdateStructureCompletion(int selectedIndex)
    {
        CheckMiniBrain();
        
        structureCompletion[selectedIndex].ViewedStructure = true;

        CalculateStructureCompletion(selectedIndex); // Calculating after updating
    }
    
    /// <summary>
    /// Updates the structure's connection that has been viewed
    /// </summary>
    /// <param name="selectedIndex">The index of the selected structure in the AtlasInfo</param>
    /// <param name="otherIndex">The index within the selected structure's connected structures that has been viewed</param>
    public void UpdateStructureCompletion(int selectedIndex, int otherIndex)
    {
        CheckMiniBrain();

        List<GameObject> temp = miniBrain.info.ValidConnections[selectedIndex];

        for (int i = 0; i < temp.Count; i++)
        {
            if (miniBrain.info.IndexOf(temp[i]) == otherIndex)
            {
                structureCompletion[selectedIndex].ViewedConnections[i] = true;

                CalculateStructureCompletion(selectedIndex);

                break;
            }
        }
    }

    /// <summary>
    /// Calculates a structure's total completion,
    /// based on whether it has been viewed,
    /// and which connections have been viewed
    /// </summary>
    /// <param name="selectedIndex">The index of the selected structure in the AtlasInfo</param>
    void CalculateStructureCompletion(int selectedIndex)
    {
        structureCompletion[selectedIndex].SubsystemCompletion = new List<float>(); // Resetting at first
        
        // The incremental value that each structure and connection view is 'worth' (for this structure)
        float completionValue = 1f / (structureCompletion[selectedIndex].ViewedConnections.Length + 1);

        // Checking first whether the structure has been viewed
        if (structureCompletion[selectedIndex].ViewedStructure)
        {
            bool[] temp = structureCompletion[selectedIndex].ViewedConnections;
            
            for (int i = 0; i < temp.Length; i++)
            {
                structureCompletion[selectedIndex].SubsystemCompletion.Add(0);
                
                // Then checking if any of the connections have been viewed
                if (temp[i])
                {
                    structureCompletion[selectedIndex].SubsystemCompletion[i] = completionValue;
                }
            }
        }

        // Updating the Subsystem completion (if there are Subsystems in this atlas)
        if (miniBrain.info.Subsystems != null)
        {
            CalculateSubsystemCompletion(); 
        }
    }

    /// <summary>
    /// Calculates each Subsystem's total completion, based on the average of its contained structures
    /// </summary>
    void CalculateSubsystemCompletion()
    {
        AtlasInfo tempInfo = miniBrain.info;
        SubsystemInfo[] tempSubsystems = tempInfo.Subsystems;

        for (int i = 0; i < tempSubsystems.Length; i++)
        {
            subsystemCompletion[i] = 0; // Resetting at first
            
            List<GameObject> tempValidStructures = tempSubsystems[i].ValidStructures;
            
            // Amount completed by each structure in regard to this particular Subsystem
            List<float> structureSubsystemCompletion = new List<float>();

            int current = 0; // The current index of the last added structure Subsystem completion amount
            
            // Calculating the Subsystem completion for each structure
            foreach (GameObject j in tempValidStructures)
            {
                // Getting the atlas index for each Subsystem's structure
                int index = tempInfo.IndexOf(j);
                
                StructureCompletion tempStructureCompletion = structureCompletion[index];

                // Getting the number of structures that are connected to this one and within this Subsystem
                // (aka possible visited connections for this structure in this Subsystem)
                int possible = tempValidStructures.Intersect(tempInfo.ValidConnections[index]).Count();

                if (possible > 0)
                {
                    structureSubsystemCompletion.Add(0);

                    int divisor = possible + 1;

                    // Ignoring the fact that the structure has been viewed if the completion is being done in stages
                    if (structureSelectionFirst)
                    {
                        divisor--;
                    }
                
                    // Subsystem 'worth' per structure = # of connected Subsystem structures + this structure
                    float completionValue = 1f / divisor;
                    
                    // Adding an amount if the structure has been viewed as part of the structure's completion
                    // if the completion isn't being done in stages
                    if (!structureSelectionFirst)
                    {
                        structureSubsystemCompletion[current] += completionValue;
                    }

                    // Checking all the possibly viewed connections for this structure
                    for (int k = 0; k < tempStructureCompletion.ViewedConnections.Length; k++)
                    {
                        // If the connection has been viewed and the viewed structure is in the current Subsystem
                        if (
                            tempStructureCompletion.ViewedConnections[k] &&
                            tempSubsystems[i].ValidStructures.Contains(tempInfo.ValidConnections[index][k]))
                        {
                            structureSubsystemCompletion[current] += completionValue;
                        }
                    }

                    current++;
                }
            }

            // Getting an average of all the structure Subsystem completions
            subsystemCompletion[i] = GetAverage(structureSubsystemCompletion.ToArray());
        }
    }

    /// <summary>
    /// Quick method to get a mean value from a float array
    /// </summary>
    /// <param name="arr">The array to be analysed</param>
    /// <returns>The average float value in the array</returns>
    float GetAverage(float[] arr)
    {
        float average = 0;

        foreach (float i in arr)
        {
            average += i;
        }

        return average / arr.Length;
    }

    /// <summary>
    /// Quick method to check through all the connections for all the completion info
    /// to determine how many connections have been viewed
    /// </summary>
    void CalculateNumberOfConnectionsViewed()
    {
        numberOfConnectionsViewed = 0; // Resetting at first
        
        foreach (StructureCompletion i in structureCompletion)
        {
            foreach (bool j in i.ViewedConnections)
            {
                if (j)
                {
                    numberOfConnectionsViewed++;
                }
            }
        }
    }

    /// <summary>
    /// Adds an outline to the structure with the given name in the diagram UI
    /// </summary>
    /// <param name="name">The name of the selected structure</param>
    public void HighlightStructureInDiagram(string name)
    {
        GameObject temp = null;

        // Looping through all the diagram UI boxes
        for (int i = 0; i < structureLayout.childCount; i++)
        {
            temp = structureLayout.GetChild(i).gameObject;

            // Stopping when the correct one is found
            if (temp.name.Equals(name))
            {
                break;
            }
        }

        // Destroying the old outline
        if (lastHighlightedStructureInDiagram != null)
        {
            Destroy(lastHighlightedStructureInDiagram.GetComponent<Outline>());
        }
        
        // Creating a new outline
        Outline tempOutline = temp.AddComponent<Outline>();
        tempOutline.OutlineColor = FindObjectOfType<StructureSelection>().selectedMaterial.color;
        tempOutline.OutlineWidth = 30f;
        tempOutline.OutlineMode = Outline.Mode.OutlineVisible;

        lastHighlightedStructureInDiagram = temp;
    }

    public void HighlightConnectionInDiagram(string name, string other, Color col)
    {
        GameObject temp = null;

        // Looping through all the diagram UI boxes
        for (int i = 0; i < structureLayout.childCount; i++)
        {
            temp = structureLayout.GetChild(i).gameObject;

            // Stopping when the correct one is found
            if (temp.name.Equals(name))
            {
                break;
            }
        }
        
        print(temp);

        foreach (LineRenderer i in temp.transform.GetChild(index + 2).gameObject.GetComponentsInChildren<LineRenderer>())
        {
            print(i);
            i.material.color = col;
        }
    }
}