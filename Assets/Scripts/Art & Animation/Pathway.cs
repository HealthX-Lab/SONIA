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
    /// Sets a pathways nodes dynamically, recalculating everything each time
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
                        tempPosition = meshBounds[j].LocalCentre;
                    }
                    else
                    {
                        tempPosition = nodes[j].transform.position;
                    }
                    
                    // Adding the new edge's particles
                    particleAttractorLinear attractor1 = Instantiate(newEdge, tempPosition, Quaternion.identity).GetComponent<particleAttractorLinear>(); // Creating a pathway at each node
                    attractor1.target = meshBounds[j+1].LocalCentre; // Setting the pathway target to the next node in the array
                    attractor1.transform.SetParent(nodes[j].transform);
                    attractor1.speed = speed;

                    if (bidirectional && !isController)
                    {
                        // Adding backwards particles if its bidirectional
                        particleAttractorLinear attractor2 = Instantiate(newEdge, meshBounds[j+1].LocalCentre, Quaternion.identity).GetComponent<particleAttractorLinear>(); // Creating a pathway at each node
                        attractor2.target = meshBounds[j].LocalCentre; // Setting the pathway target to the next node in the array
                        attractor2.transform.SetParent(nodes[j+1].transform); 
                        attractor2.speed = speed;  
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
            tempPosition = new BoundsInfo(to).LocalCentre;
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
