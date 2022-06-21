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

    GameObject lastTarget; // The last structure targeted (for checking when the target changes)
    Vector3 targetPosition; // The centre of the target's mesh's bounds
    float minDistance; // The distance away from the target that the camera stops zooming

    Vector3 increment; // Amount that the view moves when going toward the target structure

    void FixedUpdate()
    {
        if (target != null)
        {
            // Resetting values if the target changes
            if (lastTarget != target)
            {
                BoundsInfo temp = new BoundsInfo(target);
                targetPosition = temp.GlobalCentre;
                
                increment = (targetPosition - transform.position).normalized;
            
                minDistance = 10; // TODO: set this relative to the bounds size
            
                lastTarget = target;
            }
            // Otherwise, moving and rotating towards the target
            else
            {
                float distanceBetween = Vector3.Distance(transform.position, targetPosition);
                
                // Stopping at the minimum distance
                if (distanceBetween > minDistance)
                {
                    transform.position += (targetPosition - transform.position).normalized * (speed * (distanceBetween / 25f)); // Moving towards the target
                    
                    float angleBetween = Vector3.Angle(transform.forward, targetPosition - transform.position) / 120f; // Getting a scaled angle towards the target

                    // TODO: this should be in Update (but Time.deltaTime is being weird)
                    if (angleBetween > 0.001)
                    {
                        // Rotating towards the target
                        Vector3 newDirection = Vector3.RotateTowards(transform.forward, increment, speed * angleBetween, 0);
                        transform.rotation = Quaternion.LookRotation(newDirection);
                        transform.rotation = Quaternion.Euler(Vector3.up * transform.rotation.eulerAngles.y); // Clamping the rotation so the view stays flat
                    }
                }
            }
        }
    }
}
