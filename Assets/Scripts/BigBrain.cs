using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class BigBrain : MonoBehaviour
{
    [SerializeField, Tooltip("The material to be applied to the big structures")]
    Material material;
    [SerializeField, Tooltip("The bounds scale of the big atlas")]
    float scale = 1;
    
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
        transform.localScale = miniBrain.offset.localScale * scale;

        // Creating big brain structures from the mini brain structures
        foreach (GameObject i in miniBrain.info.Structures)
        {
            UpdateStructure(i, true, true, false);
        }
        
        // Also creating the left structures (if they are being ignored for connectivity purposes)
        if (miniBrain.ignoreLeft)
        {
            foreach (GameObject j in miniBrain.info.LeftStructures)
            {
                UpdateStructure(j, true, true, false);
            }   
        }

        // After the big brain has been loaded, the mini brain structures are converted to nodes
        if (miniBrain.replaceWithNodes)
        {
            miniBrain.ReplaceWithNodes();   
        }
    }

    /// <summary>
    /// Deletes a big brain structure, and replaces it with its corresponding mini brain structure
    /// </summary>
    /// <param name="key">The mini brain structure to be copied into the big brain</param>
    /// <param name="isFromStart">If this is the first time this method is being called for this structure</param>
    /// <param name="checkAndAddOutline">
    /// Whether or not to add an outline to the structure
    /// (if the structures in the mini brain are replaced by nodes)
    /// </param>
    /// <param name="showName">Whether or not to show the structure's name on the structure when selected</param>
    public void UpdateStructure(GameObject key, bool isFromStart, bool checkAndAddOutline, bool showName)
    {
        if (!structureDict.Keys.Contains(key))
        {
            // If the structure doesn't exist yet,
            // and this method is being called at the start of the scene,
            // it creates a new entry in the dictionary
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
        structureDict[key].SetActive(true);
        structureDict[key].GetComponent<MeshRenderer>().material = material;
        
        UpdateMeshRenderersInChildren(structureDict[key]);

        Transform structureTransform = structureDict[key].transform;
        
        // Making sure that the outline is gone (if it should be)
        if (miniBrain.replaceWithNodes
            && structureTransform.childCount > 0
            && !checkAndAddOutline
            && structureTransform.GetChild(0).GetComponent<Outline>())
        {
            Destroy(structureTransform.GetChild(0).GetComponent<Outline>());
        }

        if (miniBrain.replaceWithNodes && structureTransform.childCount > 0)
        {
            // Updating the colour of the big structure (which has just been made visible)
            if (checkAndAddOutline && structureTransform.GetChild(0).GetComponent<Outline>())
            {
                Outline temp = structureDict[key].AddComponent<Outline>();
                
                temp.OutlineColor = structureTransform.GetChild(0).GetComponent<Outline>().OutlineColor;
                temp.OutlineMode = Outline.Mode.OutlineVisible;
            }

            // Removing the node object if the mini structures have them
            Destroy(structureTransform.GetChild(0).gameObject);
        }

        // Updating and removing any unnecessary elements from the new big brain structure
        UpdateLineRenderersInChildren(structureDict[key]);
        UpdateOutlinesInChildren(structureDict[key]);
        RemoveComponentsInChildren<Collider>(structureDict[key]);

        // In case the user was pointing the controller at the mini brain as the scene starts
        if (isFromStart)
        {
            RemoveComponentsInChildren<Outline>(structureDict[key]);
        }

        // If the updated structure is to show its name
        if (showName)
        {
            // Adding a new name canvas
            GameObject nameCanvas = Instantiate(
                Resources.Load<GameObject>("Big Structure Canvas"),
                structureTransform
            );
            
            nameCanvas.transform.position = new BoundsInfo(structureDict[key]).GlobalCentre; // Setting its position
            nameCanvas.GetComponentInChildren<TMP_Text>().text = key.name; // Setting its name
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
            i.widthMultiplier *= scale * 150;
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
            i.enabled = true; 
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
            // Doing a check to reset the outline script
            i.enabled = false;
            i.enabled = true;
            
            Material[] mats = i.GetComponent<MeshRenderer>().materials;
            
            for (int j = 1; j < mats.Length; j++)
            {
                mats[j].renderQueue = 3000;
            }
            
            i.OutlineWidth *= 5;
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
