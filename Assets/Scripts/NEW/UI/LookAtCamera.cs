using System;
using UnityEngine;

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