using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ControllerParticles : MonoBehaviour
{
    Pathway controllerPathway; // The Pathway attached to this controller
    List<Transform> nodeTransforms; // Transforms of all the structure nodes nearby
    FindClosest closest; // Script to find the closest structure nodes
    ParticleSystem particles; // The ParticleSystem attached to this controller
    GameObject lastTarget; // The last new node emitted towards
    
    void Start()
    {
        Pathway[] temp = FindObjectsOfType<Pathway>();
        nodeTransforms = new List<Transform>();

        // Getting all the structure nodes available
        foreach (Pathway i in temp)
        {
            if (!i.isController)
            {
                foreach (GameObject j in i.nodes)
                {
                    nodeTransforms.Add(j.transform);
                }
            }
        }
        
        // Sorting all the nodes by distance
        closest = new FindClosest();
        closest.SetFromAndTo(transform, nodeTransforms.ToArray());
        
        // Adding a pathway, and setting its initial nodes
        controllerPathway = gameObject.AddComponent<Pathway>();
        controllerPathway.isController = true;
        controllerPathway.SetNodes(new[] { gameObject, closest.To[0].gameObject }, false, 10);
        
        particles = GetComponentInChildren<ParticleSystem>();
    }

    void FixedUpdate()
    {
        // Sorting the nodes and accessing the closest
        closest.SetFromAndTo(transform, nodeTransforms.ToArray());
        GameObject temp = closest.To[0].gameObject;

        // Making sure that the closest one is a new target
        if (lastTarget != temp)
        {
            // Checking to see if the node is close enough to be emitted towards
            if (Vector3.Distance(transform.position, temp.transform.position) < 20)
            {
                if (!particles.isEmitting)
                {
                    particles.Play();
                }
            
                controllerPathway.SetNodeTarget(0, temp); // Setting the new target for the particles
            }
            else
            {
                if (particles.isEmitting)
                {
                    particles.Stop();
                }
            }
            
            lastTarget = temp;
        }
    }
}
