using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathway : MonoBehaviour
{
    [Tooltip("The nodes to be connected together in a path")]
    public GameObject[] nodes;
    [Tooltip("Whether the pathway should flow both ways (doubles frame cost)")]
    [SerializeField] bool bidirectional;
    
    BoundsInfo[] meshBounds; // Mesh bounds of each GameObject
    [HideInInspector] public bool isController;

    void Start()
    {
        // Making sure only to create a path at the start if it's not from the controller (aka not going to change)
        if (!isController)
        {
            SetNodes(nodes, true, 0.5f);
        }
    }

    /// <summary>
    /// Sets the Pathway's nodes dynamically, recalculating everything each time
    /// </summary>
    /// <param name="n">New nodes for the pathway</param>
    /// <param name="isStart">Whether the method is being called from the Start method</param>
    /// <param name="speed">The new speed for the particles</param>
    public void SetNodes(GameObject[] n, bool isStart, float speed)
    {
        // Skipping this step if the given nodes are the same as the last
        if (isStart || n != nodes)
        {
            nodes = n;
        
            meshBounds = new BoundsInfo[nodes.Length];
            
            // Getting each GameObject's mesh's bounds info (so they don't have to be calculated every time)
            for (int i = 0; i < nodes.Length; i++)
            {
                // Adding to the bounds info array only if the node has a MeshFilter
                if (nodes[i].GetComponentsInChildren<MeshFilter>() != null)
                {
                    meshBounds[i] = new BoundsInfo(nodes[i]);
                }

                // Deleting old pathway particles each time a new position is calculated for the controller
                if (isController && i == 0)
                {
                    particleAttractorLinear[] temp = nodes[i].GetComponentsInChildren<particleAttractorLinear>();

                    foreach (particleAttractorLinear part in temp)
                    {
                        Destroy(part.gameObject);
                    }
                }
            }
        
            GameObject newEdge = Resources.Load<GameObject>("Pathway Edge"); // Getting the edge object
            GameObject newEdgeDescription = Resources.Load<GameObject>("Edge Description Canvas");

            for (int j = 0; j < nodes.Length; j++)
            {
                // making sure not to go over the length of the array
                if (j + 1 < nodes.Length)
                {
                    BoundsInfo tempBounds = meshBounds[j];
                    Vector3 tempPosition;

                    // If the target doesn't have a mesh, just use its position
                    if (tempBounds != null)
                    {
                        tempPosition = meshBounds[j].GlobalCentre;
                    }
                    else
                    {
                        tempPosition = nodes[j].transform.position;
                    }
                    
                    // Adding the new edge's particles
                    particleAttractorLinear attractor1 = Instantiate(newEdge, transform).GetComponent<particleAttractorLinear>(); // Creating a pathway at each node
                    attractor1.transform.position = tempPosition;
                    attractor1.target = meshBounds[j+1].GlobalCentre; // Setting the pathway target to the next node in the array
                    attractor1.speed = speed;

                    // Adding an edge description
                    EdgeDescriptionController description1 = Instantiate(newEdgeDescription, attractor1.transform).GetComponent<EdgeDescriptionController>();
                    description1.SetAlignment();

                    if (bidirectional && !isController)
                    {
                        // Adding backwards particles if its bidirectional
                        particleAttractorLinear attractor2 = Instantiate(newEdge, transform).GetComponent<particleAttractorLinear>();
                        attractor2.transform.position = meshBounds[j+1].GlobalCentre;
                        attractor2.target = tempPosition;
                        attractor2.speed = speed;
                        
                        // Adding an edge description
                        EdgeDescriptionController description2 = Instantiate(newEdgeDescription, attractor2.transform).GetComponent<EdgeDescriptionController>();
                        description2.SetAlignment();
                    }
                    else
                    {
                        var temp = attractor1.GetComponent<ParticleSystem>().emission;
                        temp.rateOverTime = 3; // Setting the appropriate rate over time if its unidirectional
                    }
                }
            }   
        }
    }

    /// <summary>
    /// Sets a new edge target without having to recalculate the whole thing
    /// </summary>
    /// <param name="from">Starting index among the nodes</param>
    /// <param name="to">New target GameObject</param>
    public void SetNodeTarget(int from, GameObject to)
    {
        Vector3 tempPosition;

        // If the target doesn't have a mesh, just use its position
        if (to.GetComponentsInChildren<MeshFilter>() != null)
        {
            tempPosition = new BoundsInfo(to).GlobalCentre;
        }
        else
        {
            tempPosition = to.transform.position;
        }
        
        nodes[from].GetComponentInChildren<particleAttractorLinear>().target = tempPosition; // Setting the new target
    }
    
    /// <summary>
    /// Sets a new edge target without having to recalculate the whole thing
    /// </summary>
    /// <param name="from">Starting index among the nodes</param>
    /// <param name="to">New target position</param>
    public void SetNodeTarget(int from, Vector3 to)
    {
        nodes[from].GetComponentInChildren<particleAttractorLinear>().target = to;
    }
}
