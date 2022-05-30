using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathway : MonoBehaviour
{
    [Header("Non-Narrative Pathways")]
    [Tooltip("Whether the pathway is associated with a Narrative")]
    public bool isNarrativePathway = true;
    [Tooltip("The material to be applied to non-Narrative pathway")]
    public Material nonNarrativePathwayMaterial;
    [Tooltip("The width of the non-Narrative pathway")]
    public float nonNarrativePathwayWidth = 0.01f;
    
    [Header("Nodes")]
    [Tooltip("The nodes to be connected together in a path")]
    public GameObject[] nodes;
    [Tooltip("Whether the pathway should flow both ways (doubles frame cost)")]
    [SerializeField] bool bidirectional;
    
    BoundsInfo[] meshBounds; // Mesh bounds of each GameObject

    public List<particleAttractorLinear>[] particles; // The edge Pathway particles associated with each node
    
    public Narrative narrative; // The Narrative class object associated with this Pathway
    public NarrativeNode[] narrativeNodes; // The specific NarrativeNodes corresponding to the GameObject nodes in the scene

    void Start()
    {
        particles = new List<particleAttractorLinear>[nodes.Length];
        narrativeNodes = new NarrativeNode[nodes.Length];
                    
        SetNodes(nodes, true, 0.5f);

        if (isNarrativePathway)
        {
            NarrativeNode previous = null;
            NarrativeNode first = null;

            // Creating new Narratives from the supplied Pathways
            for (int i = 0; i < nodes.Length; i++)
            {
                // TODO: this Narrative stuff will need to be replaced with reading from a file
                NarrativeNode temp = new NarrativeNode(nodes[i].GetComponent<StructureUIController>().name, "[DESCRIPTION]", nodes[i]);
                narrativeNodes[i] = temp;
                
                // Chaining them all together
                if (previous != null)
                {
                    // If it's bidirectional, make the next options non-linear
                    if (bidirectional && previous.Previous != null)
                    {
                        previous.SetNext(new []{ previous.Previous, temp }, new []{"[EDGE DESCRIPTION]", "[EDGE DESCRIPTION]"});
                    }
                    else
                    {
                        previous.SetNext(new []{temp}, new []{"[EDGE DESCRIPTION]"});
                    }
                }
                else
                {
                    first = temp;
                }

                previous = temp;
            }
            
            narrative = new Narrative(name, "[NARRATIVE DESCRIPTION]",first);   
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

                    if (isNarrativePathway)
                    {
                        // Adding the new edge's particles
                        particleAttractorLinear attractor1 = Instantiate(newEdge, transform).GetComponent<particleAttractorLinear>(); // Creating a pathway at each node
                        attractor1.transform.position = tempPosition;
                        attractor1.target = meshBounds[j+1].GlobalCentre; // Setting the pathway target to the next node in the array
                        attractor1.speed = speed;
                    
                        if (particles[j] == null)
                        {
                            particles[j] = new List<particleAttractorLinear>();
                        }
                    
                        particles[j].Add(attractor1);
                        
                        if (bidirectional)
                        {
                            // Adding backwards particles if its bidirectional
                            particleAttractorLinear attractor2 = Instantiate(newEdge, transform).GetComponent<particleAttractorLinear>();
                            attractor2.transform.position = meshBounds[j+1].GlobalCentre;
                            attractor2.target = tempPosition;
                            attractor2.speed = speed;
                        
                            if (particles[j+1] == null)
                            {
                                particles[j+1] = new List<particleAttractorLinear>();
                            }
                    
                            particles[j+1].Add(attractor2);
                        }
                        else
                        {
                            var temp = attractor1.GetComponent<ParticleSystem>().emission;
                            temp.rateOverTime = 3; // Setting the appropriate rate over time if its unidirectional
                        }
                    }
                    // Skipping over a lot if the pathway isn't associated with a Narrative
                    else
                    {
                        GameObject line = new GameObject("Non-Narrative Edge");
                        line.transform.SetParent(gameObject.transform);
                        
                        LineRenderer renderer = line.AddComponent<LineRenderer>();
                        renderer.material = nonNarrativePathwayMaterial;
                        renderer.SetPositions(new []{ tempPosition, meshBounds[j+1].GlobalCentre });
                        renderer.widthMultiplier = nonNarrativePathwayWidth;
                        renderer.useWorldSpace = false;
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
