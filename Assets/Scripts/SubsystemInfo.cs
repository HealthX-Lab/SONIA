using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class to generate and store info about the subsystems within the loaded atlas
/// </summary>
public class SubsystemInfo
{
    /// <summary>
    /// The atlas information to which this Subsystem belongs
    /// </summary>
    readonly AtlasInfo info;
    
    /// <summary>
    /// The name of this Subsystem
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// The description of this Subsystem
    /// </summary>
    public string Description { get; }
    
    /// <summary>
    /// The colour coding associated with this Subsystem
    /// </summary>
    public Color Colour { get; }
    
    /// <summary>
    /// The atlas structures contained within this Subsystem
    /// </summary>
    public List<GameObject> ValidStructures { get; }
    
    /// <summary>
    /// Which structures are contained within this Subsystem (corresponds to atlas's Structures)
    /// </summary>
    readonly bool[] connectivity;

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="info">The atlas that this Subsystem belongs to</param>
    /// <param name="name">The name of this Subsystem</param>
    /// <param name="description">The description of this Subsystem</param>
    /// <param name="colour">The colour coding associated with this Subsystem</param>
    /// <param name="connectivity">The connectivity array of this Subsystem</param>
    public SubsystemInfo(AtlasInfo info, string name, string description, Color colour, bool[] connectivity)
    {
        this.info = info;

        Name = name;
        Description = description;
        Colour = colour;
        ValidStructures = new List<GameObject>();
        this.connectivity = connectivity;
    }

    /// <summary>
    /// Method to synthesize the valid structures in this Subsystem after the atlas's structures have been generated
    /// </summary>
    public void SetValidStructures()
    {
        for (int i = 0; i < info.Structures.Length; i++)
        {
            // Comparing the atlas structure with the connectivity array
            if (connectivity[i])
            {
                ValidStructures.Add(info.Structures[i]);
            }
        }
    }
}