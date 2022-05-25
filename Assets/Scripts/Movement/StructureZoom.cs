using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureZoom : MonoBehaviour
{
    [Tooltip("The speed at which the camera zooms through the scene")]
    [SerializeField] float speed = 0.01f;
    [Tooltip("The structure that the camera is currently looking at and/or moving towards")]
    public GameObject target;
    [Tooltip("Whether the view rotates around the structure when it reaches it")]
    [SerializeField] bool rotating = true;

    GameObject lastTarget; // The last structure targeted (for checking when the target changes)
    Vector3 targetPosition; // The centre of the target's mesh's bounds
    float minDistance; // The distance away from the target that the camera stops zooming
    bool hasArrived; // Whether the camera has arrived at the minDistance

    Vector3 increment; // Amount that the view moves when going toward the target structure
    Vector3 flatIncrement; // Flattened version that the view looks towards when moving

    void FixedUpdate()
    {
        // Resetting information when the target is changed
        if (lastTarget != target)
        {
            // Getting the target's mesh's bounds, and using them to calculate the targetPosition and stopping minDistance
            BoundsInfo temp = new BoundsInfo(target);
            targetPosition = temp.GlobalCentre;

            hasArrived = false;

            minDistance = 10;
            
            increment = (targetPosition - transform.position).normalized * speed;
            flatIncrement = Vector3.right * increment.x;
            
            lastTarget = target;
        }
        else
        {
            // Turning the camera towards the target naturally, instead of snapping
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, flatIncrement, speed / 7.5f, 0);
            transform.rotation = Quaternion.LookRotation(newDirection);
        
            if (!hasArrived)
            {
                // Moving closer if the target is too far away
                if (Vector3.Distance(transform.position + increment, targetPosition) > minDistance)
                {
                    transform.position += increment;
                }
                // Moving away if the target is too close
                else if (Vector3.Distance(transform.position - increment, targetPosition) < minDistance)
                {
                    transform.position -= increment;
                }
                else
                {
                    hasArrived = true;
                }
            }
        }
    }

    void Update()
    {
        // Only rotating if it's reached the target and there's an actual target
        if (rotating && hasArrived && target != null)
        {
            transform.RotateAround(targetPosition, Vector3.up, 4 * speed * Time.deltaTime); // If the camera has arrived, it starts orbiting around the target
                    
            // TODO: this isn't perfect
            // Resetting the increment values so that the view continues to look at the structure while it rotates
            increment = (targetPosition - transform.position).normalized * speed;
            flatIncrement = Vector3.right * increment.x;
        }
    }
}
