using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MiniatureBrainController : MonoBehaviour
{
    [Tooltip("The miniature brain object to be created")]
    [SerializeField] GameObject brain;
    [Tooltip("Any other objects to be created in the mini brain")]
    [SerializeField] GameObject[] additionalObjects;
    [Tooltip("The active and non-active materials for the mini brain preview")]
    [SerializeField] Material defaultMaterial, hiddenMaterial;

    bool hasHiddenConnectivity;
    Pathway[] pathways;

    void Start()
    {
        CreateMiniObject(brain);
        ConfigurePathways();

        // Looping through, creating any of the extra objects
        foreach (GameObject i in additionalObjects)
        {
            CreateMiniObject(i);
        }
    }

    /// <summary>
    /// Quick method to create a miniature object and place it inside the mini brain
    /// </summary>
    /// <param name="obj">The GameObject to be instantiated</param>
    void CreateMiniObject(GameObject obj)
    {
        Instantiate(obj, transform);
    }
    
    /// <summary>
    /// Resets the mini brain Pathway visualizations to default
    /// </summary>
    void ConfigurePathways()
    {
        if (pathways == null)
        {
            pathways = GetComponentsInChildren<Pathway>();
        }

        foreach (Pathway i in pathways)
        {
            // Getting the mini structure is on the right
            if (i.transform.parent.CompareTag("Right"))
            {
                LineRenderer[] temp = i.gameObject.GetComponentsInChildren<LineRenderer>();

                // Removing any old lines
                foreach (LineRenderer j in temp)
                {
                    Destroy(j);
                }
                
                i.attractParticlesAnyway = false;
                i.nonNarrativePathwayWidth = 0.001f;
                i.nonNarrativePathwayMaterial = defaultMaterial;
                
                i.SetNodes(i.nodes, true, 0.5f); // Regenerating the new default lines
            }
        }
    }

    /// <summary>
    /// Method to highlight a specific Pathway in the mini brain
    /// </summary>
    /// <param name="path">The Pathway to be highlighted</param>
    public void SetCurrentPathway(Pathway path)
    {
        ConfigurePathways(); // First resets all the pathways

        // Hiding the connectivity display
        if (!hasHiddenConnectivity)
        {
            GetComponentInChildren<DisplayConnectivity>().gameObject.AddComponent<SetLineRendererMaterial>().material = hiddenMaterial;
            hasHiddenConnectivity = true;
        }
        
        foreach (Pathway i in pathways)
        {
            // Getting the pathway is on the right and is the one we're looking for
            if (i.name.Equals(path.name) && i.transform.parent.CompareTag("Right"))
            {
                Destroy(i.gameObject.GetComponent<SetLineRendererMaterial>());
                
                LineRenderer[] temp = i.gameObject.GetComponentsInChildren<LineRenderer>();

                // Removing any old lines
                foreach (LineRenderer j in temp)
                {
                    Destroy(j);
                }
                
                i.nonNarrativePathwayWidth = 0.005f;
                i.nonNarrativePathwayMaterial = defaultMaterial;
                
                i.SetNodes(i.nodes, true, 0.5f); // Setting the new highlighted lines
            }
            else
            {
                i.gameObject.AddComponent<SetLineRendererMaterial>().material = hiddenMaterial; // Hiding the non-active Pathways
            }
        }
    }
}
