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
    [Tooltip("The flesh, active, and non-active materials for the mini brain preview")]
    public Material fleshMaterial, defaultMaterial, hiddenMaterial;
    
    [HideInInspector] public bool hasHiddenConnectivity; // Whether the connectivity matrix has been hidden (when in structure viewing mode)
    [HideInInspector] public Pathway[] pathways; // The Pathway scripts within the mini brain

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
    public void ConfigurePathways()
    {
        pathways = GetComponentsInChildren<Pathway>();

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
            ApplyLineRendererMaterial tempApplyLineRendererMaterial = GetComponentInChildren<DisplayConnectivity>().gameObject.GetComponent<ApplyLineRendererMaterial>();

            if (tempApplyLineRendererMaterial == null)
            {
                tempApplyLineRendererMaterial = GetComponentInChildren<DisplayConnectivity>().gameObject.AddComponent<ApplyLineRendererMaterial>();
            }
            
            tempApplyLineRendererMaterial.material = hiddenMaterial;
            tempApplyLineRendererMaterial.Apply();
            hasHiddenConnectivity = true;
        }
        
        // Hiding the non-active Pathways
        foreach (Pathway i in pathways)
        {
            i.gameObject.AddComponent<ApplyLineRendererMaterial>().material = hiddenMaterial;
                
            SetMeshMaterials(i, hiddenMaterial);
        }

        // Showing the active Pathway
        foreach (Pathway j in pathways)
        {
            // Getting the pathway is on the right and is the one we're looking for
            if (j.name.Equals(path.name) && j.transform.parent.CompareTag("Right"))
            {
                j.gameObject.AddComponent<ApplyLineRendererMaterial>().material = defaultMaterial;
                
                LineRenderer[] temp = j.gameObject.GetComponentsInChildren<LineRenderer>();

                // Removing any old lines
                foreach (LineRenderer k in temp)
                {
                    Destroy(k);
                }
                
                j.nonNarrativePathwayWidth = 0.005f;
                j.nonNarrativePathwayMaterial = defaultMaterial;
                
                j.SetNodes(j.nodes, true, 0.5f); // Setting the new highlighted lines
                
                SetMeshMaterials(j, fleshMaterial);

                break;
            }
        }
    }

    /// <summary>
    /// Applies a Material to all the Meshes of a Pathway
    /// </summary>
    /// <param name="path">The given Pathway</param>
    /// <param name="mat">The Material being applied</param>
    public void SetMeshMaterials(Pathway path, Material mat)
    {
        foreach (GameObject i in path.nodes)
        {
            ApplyMeshMaterial tempApplyMeshMaterial = i.GetComponent<ApplyMeshMaterial>();

            if (tempApplyMeshMaterial == null)
            {
                tempApplyMeshMaterial = i.AddComponent<ApplyMeshMaterial>();
            }

            tempApplyMeshMaterial.material = mat;
            tempApplyMeshMaterial.Apply();
        }
    }
}
