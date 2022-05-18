using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireflies : MonoBehaviour
{
    void Start()
    {
        BoundsInfo temp = new BoundsInfo(gameObject);
        
        // Adding fireflies to each GameObject, and setting their size proportional to the bounds size
        GameObject flies = Instantiate(Resources.Load<GameObject>("Fireflies"), temp.LocalCentre, Quaternion.identity);
        flies.transform.localScale = Vector3.one * temp.Magnitude * 0.01f;
        flies.transform.SetParent(gameObject.transform);
    }
}
