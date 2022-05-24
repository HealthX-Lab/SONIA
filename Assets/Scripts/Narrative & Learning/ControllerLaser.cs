using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ControllerLaser : MonoBehaviour
{
    [Tooltip("Material to be applied to the laser")]
    [SerializeField] Material laserMaterial;
    [Tooltip("The thickness of the laser")]
    [SerializeField] float laserWidth = 0.01f;
    [Tooltip("The distance that the laser goes out to when not hitting anything")]
    [SerializeField] float nullDistance = 0.5f;
    [Tooltip("The only tags that the laser can hit")]
    [SerializeField] string[] raycastTags;

    LineRenderer line; // The LineRenderer representing the laser
    bool hasReset; // Whether the laser has been reset to its null position
    [HideInInspector] public GameObject hitObject; // The object that the laser is hitting (null if not hitting anything)

    void Start()
    {
        line = gameObject.AddComponent<LineRenderer>();

        // Initializing the LineRenderer values
        line.useWorldSpace = false;
        line.material = laserMaterial;
        line.widthMultiplier = laserWidth;
        
        line.positionCount = 2;
        line.SetPosition(0, Vector3.zero);
    }
    
    void FixedUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, Single.PositiveInfinity))
        {
            // Checking to see if the object that was hit has one that can be hit
            if (raycastTags.Contains(hit.transform.gameObject.tag))
            {
                // Making the laser snap to that object
                line.SetPosition(1, transform.InverseTransformPoint(hit.point));
                hitObject = hit.transform.gameObject;
                hasReset = false;
            }
            // Otherwise resetting
            else
            {
                ResetLaser();
            }
        }
        else
        {
            ResetLaser();   
        }
    }

    /// <summary>
    /// Quick method to reset the laser to its null position
    /// </summary>
    void ResetLaser()
    {
        if (!hasReset)
        {
            line.SetPosition(1, Vector3.forward * nullDistance);
            hitObject = null;
            hasReset = true;
        }
    }
}
