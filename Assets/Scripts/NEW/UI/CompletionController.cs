﻿using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CompletionController : MonoBehaviour
{
    [SerializeField, Tooltip("The GridLayoutGroup GameObjects where the completion info will be listed")]
    Transform structureLayout, subsystemLayout;

    MiniBrain miniBrain; // The mini brain script
    public StructureCompletion[] structureCompletion; // The % amount (0-1) that each structure has been completed to
    float[] subsystemCompletion; // The % amount (0-1) that each Subsystem has been completed to
    // The spawnable structure and Subsystem completion info objects
    GameObject structureCompletionInfoObject, subsystemCompletionInfoObject;

    /// <summary>
    /// A small struct with information about the completion amounts for a structure
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
        /// <returns></returns>
        public float Completion()
        {
            float temp = 0;

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
            
            return temp;
        }
    }

    void Start()
    {
        structureCompletionInfoObject = Resources.Load<GameObject>("Structure");
        subsystemCompletionInfoObject = Resources.Load<GameObject>("Subsystem");
    }

    /// <summary>
    /// Public method to generate both the structure and Subsystem completion info
    /// </summary>
    public void GenerateCompletionInfo()
    {
        // Makes sure that variables are set, and old children are removed
        CheckMiniBrain();
        DestroyAllChildren(structureLayout);
        DestroyAllChildren(subsystemLayout);
        
        GenerateStructureCompletionInfo();

        if (miniBrain.info.Subsystems != null)
        {
            GenerateSubsystemCompletionInfo();
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

            // Setting the lengths for the structure completion
            for (int i = 0; i < structureCompletion.Length; i++)
            {
                structureCompletion[i].ViewedConnections = new bool[miniBrain.info.ValidConnections[i].Count];
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
        // Adding a completion info object for each structure
        for (int i = 0; i < miniBrain.info.Structures.Length; i++)
        {
            // Only showing those which have some completion started
            if (structureCompletion[i].Completion() > 0)
            {
                GameObject temp = Instantiate(structureCompletionInfoObject, structureLayout);
                temp.name = miniBrain.info.Structures[i].name;

                // TODO: may want to remove the % part of the text here
                temp.GetComponentInChildren<TMP_Text>().text =
                    temp.name + ": " + // Name
                    Mathf.RoundToInt(structureCompletion[i].Completion() * 100) + "%"; // Completion amount

                // Adding colour-coded Subsystem pips and/or colour bars for each structure
                if (miniBrain.info.Subsystems != null)
                {
                    /*
                    Instantiate(Resources.Load<GameObject>("ColourPips"), temp.transform)
                        .GetComponent<ColourPips>().AddPips(miniBrain.info.Structures[i], Vector3.right * 0.3f);
                    */
                    
                    Instantiate(Resources.Load<GameObject>("ColourBars"), temp.transform)
                        .GetComponent<ColourBars>().AddBars(miniBrain.info.Structures[i], Vector3.down * 0.15f);
                }
            }
        }
        
        //Invoke(nameof(WaitSetConnections), 0.1f); // TODO: this needs to be displayed better

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

    /// <summary>
    /// Method to generate connections between structures in the completion UI
    /// (to be invoked after the structures have been added to the LayoutGroup and at least 1 frame has passed)
    /// </summary>
    void WaitSetConnections()
    {
        // Checking each structure in teh UI with each other structure
        for (int i = 0; i < structureLayout.childCount; i++)
        {
            for (int j = i+1; j < structureLayout.childCount; j++)
            {
                // Getting their shared Subsystem colours
                Color[] cols = miniBrain.info.FindSharedColours(structureLayout.GetChild(i).name, structureLayout.GetChild(j).name);

                if (cols.Length > 0)
                {
                    // Adding a new LineRenderer object
                    Transform newLine = new GameObject("Connection to " + structureLayout.GetChild(j).name).transform;
                    newLine.SetParent(structureLayout.GetChild(i).transform);
                    newLine.transform.localPosition = Vector3.zero;
                    
                    // Adding and setting the LineRenderer's attributes
                    LineRenderer renderer = newLine.AddComponent<LineRenderer>();
                    renderer.material = miniBrain.connectionMaterial;
                    renderer.widthMultiplier = 0.005f;
                    renderer.SetPositions(
                        new [] {
                            structureLayout.GetChild(i).position,
                            structureLayout.GetChild(j).position
                        }
                    );

                    // Setting the LineRenderer's colours to the shared colours
                    StructureSelection.SetLineRendererMaterial(renderer, cols, false);
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

                // TODO: add a toggle to view the Subsystem description
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
            
            // Amount completed by each structure for this particular Subsystem
            float[] structureSubsystemCompletion = new float[tempValidStructures.Count];
            
            // Calculating the Subsystem completion for each structure
            for (int j = 0; j < tempValidStructures.Count; j++)
            {
                // Getting the atlas index for each Subsystem's structure
                int index = tempInfo.IndexOf(tempValidStructures[j]);
                
                StructureCompletion tempStructureCompletion = structureCompletion[index];
                
                // Making sure that the structure has started its completion
                if (tempStructureCompletion.ViewedStructure)
                {
                    // Getting the structures that are connected to this one and within this Subsystem
                    // (aka possible visited connections for this structure in this Subsystem)
                    var possible = tempValidStructures.Intersect(tempInfo.ValidConnections[index]);
                    
                    // Subsystem 'worth' per structure = # of connected Subsystem structures + this structure
                    float completionValue = 1f / (possible.Count() + 1);
                    structureSubsystemCompletion[j] += completionValue;
                    
                    // Checking all the possibly viewed connections for this structure
                    for (int k = 0; k < tempStructureCompletion.ViewedConnections.Length; k++)
                    {
                        // If the connection has been viewed and the viewed structure is in the Subsystem
                        if (
                            tempStructureCompletion.ViewedConnections[k] &&
                            tempSubsystems[i].ValidStructures.Contains(tempInfo.ValidConnections[index][k]))
                        {
                            structureSubsystemCompletion[j] += completionValue;
                        }
                    }
                }
            }

            // Getting an average of all the structure Subsystem completions
            subsystemCompletion[i] = GetAverage(structureSubsystemCompletion);
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
}