﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class ColourPips : MonoBehaviour
{
    [Tooltip("The colour pip spawnable object")]
    [SerializeField] GameObject pip;
    [Tooltip("The colours of the pips to be instantiated")]
    public List<Color> colours;
    [Tooltip("Whether or not to automatically generate the pips when the scene starts")]
    [SerializeField] bool generateOnStart;

    MiniBrain miniBrain; // The mini brain script

    void Start()
    {
        if (generateOnStart)
        {
            Generate();
        }
    }

    /// <summary>
    /// Creates new pip children objects based on the colours
    /// </summary>
    void Generate()
    {
        foreach (Color i in colours)
        {
            Instantiate(pip, transform).GetComponent<SpriteRenderer>().color = i;
        }
    }
    
    /// <summary>
    /// Adds pips for all the Subsystems that the structure in question is a part of
    /// </summary>
    /// <param name="selected">The structure to be checked for membership to Subsystems</param>
    public void AddPips(GameObject selected) { AddPips(selected, Vector3.zero); }
    
    /// <summary>
    /// Adds pips for all the Subsystems that the structure in question is a part of
    /// </summary>
    /// <param name="selected">The structure to be checked for membership to Subsystems</param>
    /// <param name="offset">The local offset of the pips (should it be needed)</param>
    public void AddPips(GameObject selected, Vector3 offset) { AddPips(selected, selected, offset); }

    /// <summary>
    /// Adds pips for all the Subsystems that both structures are a part of
    /// </summary>
    /// <param name="selected">The structure to be checked for membership to Subsystems</param>
    /// <param name="target">A secondary structure to be checked for membership</param>
    public void AddPips(GameObject selected, GameObject target) { AddPips(selected, target, Vector3.zero); }

    /// <summary>
    /// Adds pips for all the Subsystems that both structures are a part of
    /// </summary>
    /// <param name="selected">The structure to be checked for membership to Subsystems</param>
    /// <param name="target">A secondary structure to be checked for membership</param>
    /// <param name="offset">The local offset of the pips (should it be needed)</param>
    public void AddPips(GameObject selected, GameObject target, Vector3 offset)
    {
        colours = new List<Color>(); // Making sure the colour list is empty
        
        if (miniBrain == null)
        {
            miniBrain = FindObjectOfType<MiniBrain>();
        }
        
        foreach (SubsystemInfo i in miniBrain.info.Subsystems)
        {
            // Checking which Subsystems both structures belong to, and that the list doesn't already contain the colour
            if (i.ValidStructures.Contains(selected) && i.ValidStructures.Contains(target) && !colours.Contains(i.Colour))
            {
                colours.Add(i.Colour);
            }
        }
        
        transform.localPosition += offset;

        Generate();
    }
}