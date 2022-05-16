using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireflies : MonoBehaviour
{
    void Start()
    {
        Bounds meshBounds = GetComponent<MeshFilter>().mesh.bounds;
        
        GameObject firefliesTemp = Resources.Load<GameObject>("Fireflies");
        
        // Adding fireflies to each GameObject, and setting their size proportional to the bounds size
        GameObject flies = Instantiate(firefliesTemp, transform.TransformPoint(meshBounds.center), Quaternion.identity);
        flies.transform.localScale = Vector3.one * meshBounds.size.magnitude * 0.02f;
        flies.transform.SetParent(gameObject.transform);
    }
}
