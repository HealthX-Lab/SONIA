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
    public GameObject Object { get; }

    /// <summary>
    /// Array of possible next nodes to progress to
    /// </summary>
    public NarrativeNode[] Next { get; private set; }
    
    /// <summary>
    /// The descriptions corresponding the the Pathway edges connecting this node to the next ones
    /// </summary>
    public string[] EdgeDescriptions { get; private set; }

    /// <summary>
    /// The last set node to be regressed to
    /// </summary>
    public NarrativeNode Previous { get; private set; }

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
    }

    /// <summary>
    /// Sets this node's Next value, and automatically sets those nodes' Previous values to this node
    /// </summary>
    /// <param name="next">Array of possible next nodes to progress to</param>
    public void SetNext(NarrativeNode[] next, string[] descriptions)
    {
        EdgeDescriptions = descriptions;
        
        Next = next;

        foreach (NarrativeNode i in next)
        {
            i.Previous = this;
        }
    }
}