using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CompletionController : MonoBehaviour
{
    [Tooltip("The GridLayoutGroup GameObjects where the completion info will be listed")]
    [SerializeField] Transform structureLayout, subsystemLayout;

    MiniBrain miniBrain; // The mini brain script
    StructureCompletion[] structureCompletion; // The % amount (0-1) that each structure has been completed to
    float[] subsystemCompletion; // The % amount (0-1) that each subsystem has been completed to
    GameObject structureCompletionInfoObject, subsystemCompletionInfoObject; // The spawnable structure and subsystem completion info objects

    /// <summary>
    /// A small struct with information about the completion amounts for a structure
    /// </summary>
    struct StructureCompletion
    {
        /// <summary>
        /// The float value of the completion amount (0-1)
        /// </summary>
        public float Completion { get; set; }
        
        /// <summary>
        /// Whether the structure has been viewed
        /// </summary>
        public bool ViewedStructure { get; set; }
        
        /// <summary>
        /// Which of the structure's connections have been viewed
        /// </summary>
        public bool[] ViewedConnections { get; set; }
    }

    void Start()
    {
        structureCompletionInfoObject = Resources.Load<GameObject>("Structure");
        subsystemCompletionInfoObject = Resources.Load<GameObject>("Subsystem");
    }

    /// <summary>
    /// Public method to generate both the structure and subsystem completion info
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

            // Setting the length for the subsystem completion
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
            if (structureCompletion[i].Completion > 0)
            {
                Instantiate(structureCompletionInfoObject, structureLayout).GetComponentInChildren<TMP_Text>().text =
                    miniBrain.info.Structures[i].name + ": " + // Name
                    Mathf.RoundToInt(structureCompletion[i].Completion * 100) + "%";      // Completion amount
            }
        }
    }
    
    /// <summary>
    /// Method to generate/update the subsystem completion info
    /// </summary>
    void GenerateSubsystemCompletionInfo()
    {
        // Adding a completion info object for each subsystem
        for (int i = 0; i < miniBrain.info.Subsystems.Length; i++)
        {
            // Only showing those which have some completion started
            if (subsystemCompletion[i] > 0)
            {
                TMP_Text temp = Instantiate(subsystemCompletionInfoObject, subsystemLayout).GetComponentInChildren<TMP_Text>();
                
                temp.text =
                    miniBrain.info.Subsystems[i].Name + ": " + // Name
                    Mathf.RoundToInt(subsystemCompletion[i] * 100) + "%\n\n" + // Completion amount
                    miniBrain.info.Subsystems[i].Description;  // Subsystem description

                temp.color = miniBrain.info.Subsystems[i].Colour;
            }
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
    /// <param name="otherIndex">The index within teh selected structure's connected structures that has been viewed</param>
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
    /// Calculates a structure's total completion, based on whether it has been viewed, and which connections have been viewed
    /// </summary>
    /// <param name="selectedIndex">The index of the selected structure in the AtlasInfo</param>
    void CalculateStructureCompletion(int selectedIndex)
    {
        structureCompletion[selectedIndex].Completion = 0; // Resetting at first
        
        float completionValue = 1f / (structureCompletion[selectedIndex].ViewedConnections.Length + 1); // The incremental value that each structure and connection view is 'worth' (for this structure)

        // Checking first whether the structure has been viewed
        if (structureCompletion[selectedIndex].ViewedStructure)
        {
            structureCompletion[selectedIndex].Completion += completionValue;
            
            foreach (bool i in structureCompletion[selectedIndex].ViewedConnections)
            {
                // Then checking if any of the connections have been viewed
                if (i)
                {
                    structureCompletion[selectedIndex].Completion += completionValue;
                }
            }
        }

        // Updating the subsystem completion (if there are subsystems in this atlas)
        if (miniBrain.info.Subsystems != null)
        {
            CalculateSubsystemCompletion(); 
        }
    }

    /// <summary>
    /// Calculates each subsystem's total completion, based on the average of its contained structures
    /// </summary>
    void CalculateSubsystemCompletion()
    {
        AtlasInfo tempInfo = miniBrain.info;
        SubsystemInfo[] tempSubsystems = tempInfo.Subsystems;
        
        for (int i = 0; i < tempSubsystems.Length; i++)
        {
            subsystemCompletion[i] = 0; // Resetting at first
            
            List<GameObject> tempValidStructures = tempSubsystems[i].ValidStructures;
            
            float average = 0; // Creating a temporary average variable
            
            // Adding the structures' completion values together
            foreach (GameObject j in tempValidStructures)
            {
                average += structureCompletion[tempInfo.IndexOf(j)].Completion;
            }

            subsystemCompletion[i] = average / tempValidStructures.Count; // Dividing to get the average
        }
    }
}