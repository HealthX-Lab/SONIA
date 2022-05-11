using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathway : MonoBehaviour
{
    [Tooltip("The nodes to be connected together in a path")]
    [SerializeField] GameObject[] nodes;

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
        GameObject firefliesTemp = Resources.Load<GameObject>("Fireflies");

        for (int j = 0; j < nodes.Length; j++)
        {
            Vector3 temp1 = nodes[j].transform.TransformPoint(meshBounds[j].center); // Getting the local position of the bounds' centres
            
            if (j + 1 < nodes.Length)
            {
                Vector3 temp2 = nodes[j+1].transform.TransformPoint(meshBounds[j+1].center); // Getting the next GameObject's local bound centre position
                
                particleAttractorLinear attractor = Instantiate(pathwayTemp, temp1, Quaternion.identity).GetComponent<particleAttractorLinear>(); // Creating a pathway at each node
                attractor.target = temp2; // Setting the pathway target to the next node in the array
                attractor.transform.SetParent(nodes[j].transform);
            }
            
            // Adding fireflies to each GameObject, and setting their size proportional to the bounds size
            GameObject flies = Instantiate(firefliesTemp, temp1, Quaternion.identity);
            flies.transform.localScale = Vector3.one * meshBounds[j].size.magnitude * 0.02f;
            flies.transform.SetParent(nodes[j].transform);
        }
    }
}
