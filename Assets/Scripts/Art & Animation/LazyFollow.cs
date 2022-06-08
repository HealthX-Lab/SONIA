using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LazyFollow : MonoBehaviour
{
    [Tooltip("Whether or not to actually follow the target (the rotation is still fixed though, if checked)")]
    [SerializeField] bool follow = true;
    [Tooltip("Whether or not to immediately snap the the target's position (instead of drifting)")]
    [SerializeField] bool snap;
    [Tooltip("The target that the object if drifting towards")]
    public Transform target;
    [Tooltip("The speed at which it is drifting")]
    [SerializeField] float speed = 2;
    [Tooltip("The minimum global height that the object can rest at")]
    [SerializeField] float minHeight = 0.5f;
    [Tooltip("Whether to fix the object's rotation at global zero")]
    [SerializeField] bool ignoreRotation = true;

    void FixedUpdate()
    {
        if (follow)
        {
            if (snap)
            {
                transform.position = target.position;
            }
            else
            {
                float distance = Vector3.Distance(transform.position, target.position); // Getting the distance from this object to its target
        
                // Stopping at a minimum distance from the target
                if (distance > speed / 10f)
                {
                    transform.position += (target.position - transform.position).normalized * ((speed * distance) * Time.deltaTime); // Moving the object towards the target

                    // Making sure that the object is above the minimum height
                    if (transform.position.y < minHeight)
                    {
                        transform.position = new Vector3(transform.position.x, minHeight, transform.position.z);
                    }
                }
            }
        }
    }

    void Update()
    {
        if (ignoreRotation)
        {
            transform.rotation = Quaternion.identity; // Resetting rotation
        }
    }
}
