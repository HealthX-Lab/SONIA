using System;
using System.Collections.Generic;
using UnityEngine;

public class ColourPips : MonoBehaviour
{
    [SerializeField, Tooltip("The colour pip spawnable object")]
    GameObject pip;
    [Tooltip("The colours of the pips to be instantiated")]
    public List<Color> colours;
    [SerializeField, Tooltip("Whether or not to automatically generate the pips when the scene starts")]
    bool generateOnStart;

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
        
        // Adding colours for each shared Subsystem
        foreach (SubsystemInfo i in miniBrain.info.FindSharedSubsystems(selected, target))
        {
            colours.Add(i.Colour);
        }
        
        transform.localPosition += offset;

        Generate();
    }
}