using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CompletionController : MonoBehaviour
{
    [Tooltip("The GridLayoutGroup GameObjects where the completion info will be listed")]
    [SerializeField] Transform structureLayout, subsystemLayout;

    MiniBrain miniBrain; // The mini brain script
    StructureCompletion[] structureCompletion; // The % amount (0-1) that each structure has been completed to
    float[] subsystemCompletion; // The % amount (0-1) that each Subsystem has been completed to
    GameObject structureCompletionInfoObject, subsystemCompletionInfoObject; // The spawnable structure and Subsystem completion info objects

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
            if (structureCompletion[i].Completion > 0)
            {
                GameObject temp = Instantiate(structureCompletionInfoObject, structureLayout);
                
                temp.GetComponentInChildren<TMP_Text>().text =
                    miniBrain.info.Structures[i].name + ": " +                         // Name
                    Mathf.RoundToInt(structureCompletion[i].Completion * 100) + "%"; // Completion amount

                // Adding colour-coded Subsystem pips for each structure
                if (miniBrain.info.Subsystems != null)
                {
                    Instantiate(Resources.Load<GameObject>("ColourPips"), temp.transform).GetComponent<ColourPips>()
                        .AddPips(miniBrain.info.Structures[i], Vector3.right * 0.3f);
                }
            }
        }
        
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
                TMP_Text temp = Instantiate(subsystemCompletionInfoObject, subsystemLayout).GetComponentInChildren<TMP_Text>();
                
                temp.text =
                    miniBrain.info.Subsystems[i].Name + ": " + // Name
                    Mathf.RoundToInt(subsystemCompletion[i] * 100) + "%\n\n" + // Completion amount
                    miniBrain.info.Subsystems[i].Description;  // Subsystem description

                temp.color = miniBrain.info.Subsystems[i].Colour;
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
            float[] structureSubsystemCompletion = new float[tempValidStructures.Count]; // Amount completed by each structure for this particular Subsystem
            
            // Calculating the Subsystem completion for each structure
            for (int j = 0; j < tempValidStructures.Count; j++)
            {
                int index = tempInfo.IndexOf(tempValidStructures[j]); // Getting the atlas index for each Subsystem's structure
                
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

            subsystemCompletion[i] = GetAverage(structureSubsystemCompletion); // Getting an average of all the structure Subsystem completions
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