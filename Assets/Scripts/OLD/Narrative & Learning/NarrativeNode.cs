using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Class representing a single node in a larger branching narrative tree
/// </summary>
public class NarrativeNode
{
    /// <summary>
    /// The name of the structure at which the node resides
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// The information text regarding the structure/functions of the node
    /// </summary>
    public string Description { get; }
    
    /// <summary>
    /// The structure that the node is associated with
    /// </summary>
    public GameObject Object { get; set; }
    
    /// <summary>
    /// Temporarily-stored name of the GameObject associated with this NarrativeNode
    /// </summary>
    public string ObjectName { get; set; }

    /// <summary>
    /// Array of possible next nodes to progress to
    /// </summary>
    public List<NarrativeNode> Next { get; set; }
    
    /// <summary>
    /// Temporarily-stored names of next NarrativeNodes in the Narrative
    /// </summary>
    public List<string> NextNames { get; set; }
    
    /// <summary>
    /// The descriptions corresponding the the Pathway edges connecting this node to the next ones
    /// </summary>
    public List<string> EdgeDescriptions { get; set; }

    /// <summary>
    /// The last set node to be regressed to
    /// </summary>
    public NarrativeNode Previous { get; set; }
    
    /// <summary>
    /// Temporarily-stored name of the previous NarrativeNode in the Narrative
    /// </summary>
    public string PreviousName { get; set; }
    
    /// <summary>
    /// The descriptions corresponding the the Pathway edges connecting the previous node to this one
    /// </summary>
    public string PreviousEdgeDescription { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public NarrativeNode()
    {
        Name = "";
        Description = "";
        Object = null;
        Next = null;
        EdgeDescriptions = null;
        Previous = null;
        PreviousEdgeDescription = "";
    }
    
    /// <summary>
    /// Name parameterized constructor
    /// </summary>
    /// <param name="name">The name of the structure at which the node resides</param>
    public NarrativeNode(string name)
    {
        Name = name;
        Description = "";
        Object = null;
        Next = null;
        EdgeDescriptions = null;
        Previous = null;
        PreviousEdgeDescription = "";
    }
    
    /// <summary>
    /// Name and description parameterized constructor
    /// </summary>
    /// <param name="name">The name of the structure at which the node resides</param>
    /// <param name="description">The information text regarding the structure/functions of the node</param>
    public NarrativeNode(string name, string description)
    {
        Name = name;
        Description = description;
        Object = null;
        Next = null;
        EdgeDescriptions = null;
        Previous = null;
        PreviousEdgeDescription = "";
    }
    
    /// <summary>
    /// Name, description, and position parameterized constructor
    /// </summary>
    /// <param name="name">The name of the structure at which the node resides</param>
    /// <param name="description">The information text regarding the structure/functions of the node</param>
    /// <param name="obj">The structure that the node is associated with</param>
    public NarrativeNode(string name, string description, GameObject obj)
    {
        Name = name;
        Description = description;
        Object = obj;
        Next = null;
        EdgeDescriptions = null;
        Previous = null;
        PreviousEdgeDescription = "";
    }

    /// <summary>
    /// Sets this node's Next value, and automatically sets those nodes' Previous values to this node
    /// </summary>
    /// <param name="next">Array of possible next nodes to progress to</param>
    /// <param name="descriptions">The edge descriptions for the next nodes</param>
    /// <param name="previousDescriptions">The edge description for the previous node</param>
    public void SetNext(NarrativeNode[] next, string[] descriptions, string[] previousDescriptions)
    {
        EdgeDescriptions = descriptions.ToList();
        
        Next = next.ToList();

        for (int i = 0; i < next.Length; i++)
        {
            next[i].Previous = this;
            next[i].PreviousEdgeDescription = previousDescriptions[i];
        }
    }
}