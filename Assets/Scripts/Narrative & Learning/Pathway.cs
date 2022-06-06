using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathway : MonoBehaviour
{
    public string name;
    public bool hideVisualization;
    
    [Header("Non-Narrative Pathways")]
    [Tooltip("Whether the pathway is associated with a Narrative")]
    public bool isNarrativePathway = true;
    [Tooltip("Whether to display the attractor particles, even if the Pathway is non-Narrative")]
    public bool attractParticlesAnyway;
    [Tooltip("The material to be applied to non-Narrative pathway")]
    public Material nonNarrativePathwayMaterial;
    [Tooltip("The width of the non-Narrative pathway")]
    public float nonNarrativePathwayWidth = 0.01f;
    
    [Header("Nodes")]
    [Tooltip("The nodes to be connected together in a path")]
    public GameObject[] nodes;
    [Tooltip("Whether the pathway should flow both ways (doubles frame cost)")]
    public bool bidirectional;
    
    BoundsInfo[] meshBounds; // Mesh bounds of each GameObject

    public List<particleAttractorLinear>[] particles; // The edge Pathway particles associated with each node
    
    public Narrative narrative; // The Narrative class object associated with this Pathway
    public NarrativeNode[] narrativeNodes; // The specific NarrativeNodes corresponding to the GameObject nodes in the scene

    void Start()
    {
        particles = new List<particleAttractorLinear>[nodes.Length];

        if (!hideVisualization)
        {
            SetNodes(nodes, true, 0.5f);
        }

        name = gameObject.name;
    }

    /// <summary>
    /// Sets the Pathway's nodes dynamically, recalculating everything each time
    /// </summary>
    /// <param name="n">New nodes for the pathway</param>
    /// <param name="forceSet">Whether the method is forced set the Pathway (in some cases it may be skipped, and this bypasses that)</param>
    /// <param name="speed">The new speed for the particles</param>
    public void SetNodes(GameObject[] n, bool forceSet, float speed)
    {
        // Skipping this step if the given nodes are the same as the last
        if (forceSet || n != nodes)
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
                // Making sure not to go over the length of the array
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

                    if (isNarrativePathway || (!isNarrativePathway && attractParticlesAnyway))
                    {
                        // Adding the new edge's particles
                        particleAttractorLinear attractor1 = Instantiate(newEdge, transform).GetComponent<particleAttractorLinear>(); // Creating a pathway at each node
                        attractor1.transform.position = tempPosition;
                        attractor1.target = meshBounds[j+1].Transform.InverseTransformPoint(meshBounds[j+1].GlobalCentre); // Setting the pathway target to the next node in the array
                        attractor1.transformParent = meshBounds[j+1].Transform;
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
                            attractor2.target = meshBounds[j].Transform.InverseTransformPoint(tempPosition);
                            attractor2.transformParent = meshBounds[j].Transform;
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

                        // Modifying the particle values if the particles are non-narrative
                        if (!isNarrativePathway && attractParticlesAnyway)
                        {
                            ParticleSystem system = attractor1.GetComponent<ParticleSystem>();

                            var temp = system.velocityOverLifetime;
                            temp.enabled = false; // Making it so the particles don't curve out at all

                            var temp1 = system.emission;
                            temp1.rateOverTime = 5; // Turning the rate up
                            
                            // Slimming the trails down
                            var temp2 = system.trails;
                            temp2.ratio = 0.5f;
                            temp2.sizeAffectsWidth = false;
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
    /// Whether the Pathway contains a node with the given name and layer
    /// </summary>
    /// <param name="name">The GameObject's name</param>
    /// <param name="layer">The GameObject's LayerMask</param>
    /// <returns>Whether the Pathway contains the desired node</returns>
    public bool DoesContain(string name, LayerMask layer)
    {
        foreach (GameObject i in nodes)
        {
            if (i.name.Equals(name) && i.layer.Equals(layer))
            {
                return true;
            }
        }

        return false;
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
