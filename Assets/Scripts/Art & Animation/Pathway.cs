using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathway : MonoBehaviour
{
    [Tooltip("The nodes to be connected together in a path")]
    public GameObject[] nodes;
    [Tooltip("Whether the pathway should flow both ways (doubles frame cost)")]
    [SerializeField] bool bidirectional = false;

    Bounds[] meshBounds; // Mesh bounds of each GameObject

    void Start()
    {
        meshBounds = new Bounds[nodes.Length];
        
        // Getting each GameObject's mesh's bounds (so they don't have to be calculated every time)
        for (int i = 0; i < nodes.Length; i++)
        {
            meshBounds[i] = nodes[i].GetComponent<MeshFilter>().mesh.bounds;
        }
        
        GameObject pathwayTemp = Resources.Load<GameObject>("Pathway");

        for (int j = 0; j < nodes.Length; j++)
        {
            Vector3 temp1 = nodes[j].transform.TransformPoint(meshBounds[j].center); // Getting the local position of the bounds' centres
            
            if (j + 1 < nodes.Length)
            {
                Vector3 temp2 = nodes[j+1].transform.TransformPoint(meshBounds[j+1].center); // Getting the next GameObject's local bound centre position

                particleAttractorLinear attractor1 = Instantiate(pathwayTemp, temp1, Quaternion.identity).GetComponent<particleAttractorLinear>(); // Creating a pathway at each node
                attractor1.target = temp2; // Setting the pathway target to the next node in the array
                attractor1.transform.SetParent(nodes[j].transform);

                if (bidirectional)
                {
                    particleAttractorLinear attractor2 = Instantiate(pathwayTemp, temp2, Quaternion.identity).GetComponent<particleAttractorLinear>(); // Creating a pathway at each node
                    attractor2.target = temp1; // Setting the pathway target to the next node in the array
                    attractor2.transform.SetParent(nodes[j+1].transform);   
                }
                else
                {
                    var temp = attractor1.GetComponent<ParticleSystem>().emission;
                    temp.rateOverTime = 3;
                }
            }
        }
    }
}
