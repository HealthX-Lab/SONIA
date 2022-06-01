using System;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Tooltip("The management script for pathway selection")]
    [SerializeField] PathwaySelectionManager pathwayManager;
    [Tooltip("The management script for structure previewing")]
    [SerializeField] StructureManager structureManager;

    void Start()
    {
        GoToPathwayUI(); // Pathway by default
    }

    /// <summary>
    /// Shows the pathway manager UI
    /// </summary>
    public void GoToPathwayUI()
    {
        SetPathwayManager(true);
        SetStructureManager(false);
    }

    /// <summary>
    /// Shows the structure manager UI
    /// </summary>
    public void GoToStructureUI()
    {
        SetPathwayManager(false);
        SetStructureManager(true);
        
        NarrativeNode temp = pathwayManager.currentPathway.narrative.Current; // Getting the current NarrativeNode
        
        // Initializing some variables for later
        string next1 = "";
        string next2 = "";
        string edge1 = "";
        string edge2 = "";
        GameObject structure1 = null;
        GameObject structure2 = null;
        structureManager.isLeftPrevious = false;

        // If the current node has next nodes
        if (temp.Next.Count > 0)
        {
            // Setting the right values if they exist
            next1 = temp.Next[0].Name;
            edge1 = temp.EdgeDescriptions[0];
            structure1 = temp.Next[0].Object;
            
            if (temp.Next.Count > 1)
            {
                // Setting the left values if they exist
                next2 = temp.Next[1].Name;
                edge2 = temp.EdgeDescriptions[1];
                structure2 = temp.Next[1].Object;
            }
        }
        
        // If there are no next nodes or only one
        if (temp.Next.Count < 2)
        {
            if (temp.Previous != null)
            {
                // Setting the previous values on the left side if they do exist
                next2 = temp.Previous.Name;
                edge2 = temp.PreviousEdgeDescription;
                structure2 = temp.Previous.Object;

                structureManager.isLeftPrevious = true;
            }
        }

        // Applying all of the UI
        structureManager.SetUI(
            new []{ temp.Name, next1, next2 },
            temp.Description, 
            new []{ edge1, edge2 },
            new []{ temp.Object, structure1, structure2 }
        );
        
        pathwayManager.SetCurrentStructure(); // Setting the structure visualization to the current NarrativeNode
    }

    /// <summary>
    /// Public method to quickly go to the next NarrativeNode in the structure UI (right or left)
    /// </summary>
    /// <param name="index">0 = right, 1 = left</param>
    public void GoToNext(int index)
    {
        pathwayManager.currentPathway.narrative.GoToNext(index); // Setting the current Narrative appropriately
        GoToStructureUI();
    }
    
    /// <summary>
    /// Public method to quickly go to the previous NarrativeNode in the structure UI
    /// </summary>
    public void GoToPrevious()
    {
        pathwayManager.currentPathway.narrative.GoToPrevious(); // Setting the current Narrative appropriately
        GoToStructureUI();
    }

    /// <summary>
    /// Method to show/hide the pathway selection management script
    /// </summary>
    /// <param name="active">New visibility value</param>
    void SetPathwayManager(bool active) { pathwayManager.gameObject.SetActive(active); }

    /// <summary>
    /// Method to show/hide the structure previewing management script
    /// </summary>
    /// <param name="active"></param>
    void SetStructureManager(bool active) { structureManager.gameObject.SetActive(active); }
}