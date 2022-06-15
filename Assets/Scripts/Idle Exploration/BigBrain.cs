using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BigBrain : MonoBehaviour
{
    [Tooltip("The material to be applied to the big structures")]
    [SerializeField] Material material;
    [Tooltip("The bounds scale of the big atlas")]
    [SerializeField] float scale = 1000;
    
    MiniBrain miniBrain; // The mini brain script
    
    Transform offset; // The offset parent Transform for the structures
    Dictionary<GameObject, GameObject> structureDict; // A dictionary correlating the mini brain structures to matching big brain structures

    void Start()
    {
        miniBrain = FindObjectOfType<MiniBrain>();
        structureDict = new Dictionary<GameObject, GameObject>();
        
        Invoke(nameof(Generate), 0.1f);
    }

    /// <summary>
    /// Generates a copy of the mini brain atlas as a child (and sets the dictionary)
    /// </summary>
    void Generate()
    {
        // Creating and orienting the offset parent
        offset = new GameObject("Offset").transform;
        offset.SetParent(transform);
        offset.localPosition = miniBrain.offset.localPosition;
        offset.localRotation = miniBrain.offset.localRotation;
        offset.localScale = miniBrain.offset.localScale * scale;

        // Creating big brain structures from the mini brain structures
        foreach (GameObject i in miniBrain.info.Structures)
        {
            UpdateStructure(i, true);
        }
    }

    /// <summary>
    /// Deletes a big brain structure, and replaces it with its corresponding mini brain structure
    /// </summary>
    /// <param name="key"></param>
    /// <param name="isFromStart"></param>
    public void UpdateStructure(GameObject key, bool isFromStart)
    {
        if (!structureDict.Keys.Contains(key))
        {
            // If the structure doesn't exist yet, and this method is being called at the start of the scene, it creates a new entry in teh dictionary
            if (isFromStart)
            {
                structureDict.Add(key, null);
            }
            else
            {
                return;
            }
        }
        else
        {
            Destroy(structureDict[key]); // Removing the old big structure
        }

        // Creating the new big structure
        structureDict[key] = Instantiate(key, offset);
        structureDict[key].GetComponent<MeshRenderer>().material = material;
        
        // Updating and removing any unnecessary elements from the new big brain structure
        UpdateLineRenderersInChildren(structureDict[key]);
        UpdateOutlinesInChildren(structureDict[key]);
        RemoveComponentsInChildren<Collider>(structureDict[key]);

        // In case the user was pointing the controller at teh mini brain as the scene starts
        if (isFromStart)
        {
            UpdateMeshRenderersInChildren(structureDict[key]);
            RemoveComponentsInChildren<Outline>(structureDict[key]);
        }
    }
    
    /// <summary>
    /// Quick method to scale up the width of the big brain LineRenderers
    /// </summary>
    /// <param name="obj">The object in which to search</param>
    void UpdateLineRenderersInChildren(GameObject obj)
    {
        foreach (LineRenderer i in obj.GetComponentsInChildren<LineRenderer>())
        {
            i.widthMultiplier *= scale;
        }
    }

    /// <summary>
    /// Quick method to remove excess Materials in the big brain MeshRenderers
    /// </summary>
    /// <param name="obj">The object in which to search</param>
    void UpdateMeshRenderersInChildren(GameObject obj)
    {
        foreach (MeshRenderer i in obj.GetComponentsInChildren<MeshRenderer>())
        {
            i.materials = new[] { i.material };
        }
    }

    /// <summary>
    /// Quick method to scale up the width of the big brain Outlines
    /// </summary>
    /// <param name="obj">The object in which to search</param>
    void UpdateOutlinesInChildren(GameObject obj)
    {
        foreach (Outline i in obj.GetComponentsInChildren<Outline>())
        {
            i.OutlineWidth *= 10;
        }
    }

    /// <summary>
    /// Quick method to remove all instances of the given Component in the given GameObject
    /// </summary>
    /// <param name="obj">The GameObject to be searched through</param>
    /// <typeparam name="T">The Component type to be removed</typeparam>
    void RemoveComponentsInChildren<T>(GameObject obj) where T : Component
    {
        foreach (T i in obj.GetComponentsInChildren<T>())
        {
            Destroy(i);
        }
    }
}
