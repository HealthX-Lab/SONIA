using System;
using UnityEngine;

/// <summary>
/// MonoBehaviour to constantly point towards the main camera (and scale with distance)
/// </summary>
/// <organization>Health-X Lab</organization>
/// <project>Insideout (May-August 2022)</project>
/// <author>Owen Hellum</author>
public class LookAtCamera : MonoBehaviour
{
    Transform cameraTransform; // The Transform component of the headset

    void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    void FixedUpdate()
    {
        // Looking towards the headset and scaling appropriately
        transform.LookAt(cameraTransform);
        transform.localScale = Vector3.one * (Vector3.Distance(transform.position, cameraTransform.position) / 4f);
    }
}