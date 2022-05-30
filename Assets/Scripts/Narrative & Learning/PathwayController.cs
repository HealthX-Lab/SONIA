using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathwayController : MonoBehaviour
{
    [Tooltip("The default material for each structure")]
    [SerializeField] Material defaultMaterial;
    [Tooltip("The material when the structure is occluding")]
    [SerializeField] Material occlusionMaterial;
    
    public Dictionary<GameObject, Pathway> pathwayDict; // The dictionary to quickly access the Pathways for each respective UI option
    
    PathwaySelectionManager manager; // The UI manager script that houses and manipulates the pathways
    bool hasGeneratedPathwayOptions; // Whether the Pathway selection options have been generated yet in the UI
    
    void Start()
    {
        if (pathwayDict == null)
        {
            pathwayDict = new Dictionary<GameObject, Pathway>();
        }

        manager = FindObjectOfType<PathwaySelectionManager>();

        Invoke(nameof(HideAllPathways), 0.1f); // Hiding all pathways at the beginning
    }

    /// <summary>
    /// Whether the Pathway dictionary contains some option as a key
    /// </summary>
    /// <param name="option">The UI option being hit and checked</param>
    /// <returns>Whether the option is in the dictionary</returns>
    public bool ContainsKey(GameObject option)
    {
        return pathwayDict.Keys.Contains(option);
    }

    /// <summary>
    /// Sets the pathway (and its connected structures) active and inactive
    /// </summary>
    /// <param name="path">The Pathway to be set</param>
    /// <param name="activeValue">Whether the pathway is being set on or off</param>
    void SetPathway(Pathway path, bool activeValue)
    {
        path.gameObject.SetActive(activeValue); // Setting the pathway visibility

        Material tempMaterial = defaultMaterial;

        if (!activeValue)
        {
            tempMaterial = occlusionMaterial;
        }

        for (int i = 0; i < path.nodes.Length; i++)
        {
            MeshRenderer[] tempRenderers = path.nodes[i].GetComponentsInChildren<MeshRenderer>();

            // Setting the Pathways' structures to the appropriate material and setting their colliders
            foreach (MeshRenderer j in tempRenderers)
            {
                j.material = tempMaterial;
                j.GetComponent<MeshCollider>().enabled = activeValue;
            }
            
            StructureUIController tempStructureUI = path.nodes[i].GetComponent<StructureUIController>();
            
            tempStructureUI.canvasTransform.gameObject.SetActive(activeValue); // Setting the structures' names too

            NarrativeNode tempNode = path.narrativeNodes[i];
            tempStructureUI.SetUI(tempNode.Name, tempNode.Description);
        }
    }

    /// <summary>
    /// Sets all the Pathways to false
    /// </summary>
    void HideAllPathways()
    {
        foreach (Pathway i in manager.pathways)
        {
            SetPathway(i, false);
        }

        // Making sure to generate the Pathway selection UI options after the Pathways have been generated
        if (!hasGeneratedPathwayOptions)
        {
            manager.GeneratePathwayOptions();
            hasGeneratedPathwayOptions = true;
        }
    }
    
    /// <summary>
    /// Sets all the pathways to false, then shows the given one
    /// </summary>
    /// <param name="path">The Pathway to be set visible</param>
    public void SetCurrentPathway(Pathway path)
    {
        // Only making the appropriate pathway visible
        HideAllPathways();
        SetPathway(path, true);
    }
}