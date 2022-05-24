using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [Tooltip("The speed at which the camera zooms through the scene")]
    [SerializeField] float speed = 0.1f;
    [Tooltip("The structure that the camera is currently looking at and/or moving towards")]
    public GameObject target;

    GameObject lastTarget; // The last structure targeted (for checking when the target changes)
    Vector3 targetPosition; // The centre of the target's mesh's bounds
    float minDistance; // The distance away from the target that the camera stops zooming
    bool hasArrived; // Whether the camera has arrived at the minDistance

    void FixedUpdate()
    {
        // Resetting information when the target is changed
        if (lastTarget != target)
        {
            // Getting the target's mesh's bounds, and using them to calculate the targetPosition and stopping minDistance
            BoundsInfo temp = new BoundsInfo(gameObject);
            targetPosition = temp.GlobalCentre;
            minDistance = temp.Magnitude * 0.35f;

            hasArrived = false;
            
            lastTarget = target;
        }
        
        Vector3 increment = (targetPosition - transform.position).normalized * speed; // The amount that the camera may move in the next frame
        
        // Turning the camera towards the target naturally, instead of snapping
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, increment, speed / 7.5f, 0);
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
        else
        {
            transform.RotateAround(targetPosition, Vector3.up, speed); // If the camera has arrived, it starts orbiting around the target
        }
    }
}
