using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniatureBrainController : MonoBehaviour
{
    [Tooltip("The miniature brain object to be created")]
    [SerializeField] GameObject brain;
    [Tooltip("Any other objects to be created in the mini brain")]
    [SerializeField] GameObject[] additionalObjects;

    void Start()
    {
        CreateMiniObject(brain);

        // Looping through, creating any of the extra objects
        foreach (GameObject i in additionalObjects)
        {
            CreateMiniObject(i);
        }
    }

    /// <summary>
    /// Quick method to create a miniature object and place it inside the mini brain
    /// </summary>
    /// <param name="obj">The GameObject to be instantiated</param>
    /// <returns>The new mini object</returns>
    GameObject CreateMiniObject(GameObject obj)
    {
        GameObject temp = Instantiate(obj, transform);
        return temp;
    }
}
