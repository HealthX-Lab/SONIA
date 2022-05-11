using UnityEngine;

/// <summary>
/// Class representing a single node in a larger branching narrative tree
/// </summary>
public class NarrativeNode
{
    /// <summary>
    /// The name of the structure at which the node resides
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// The information text regarding the structure/functions of the node
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// The structure that the node is associated with
    /// </summary>
    public GameObject Object { get; set; }

    NarrativeNode[] next;
    /// <summary>
    /// Array of possible next nodes to progress to
    /// </summary>
    public NarrativeNode[] Next => next;

    /// <summary>
    /// The last set node to be regressed to
    /// </summary>
    public NarrativeNode Previous { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public NarrativeNode()
    {
        Name = "";
        Description = "";
        Object = null;
        next = null;
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
        next = null;
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
        next = null;
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
        next = null;
        Previous = null;
    }

    /// <summary>
    /// Sets this node's Next value, and automatically sets those nodes' Previous values to this node
    /// </summary>
    /// <param name="next">Array of possible next nodes to progress to</param>
    public void SetNext(NarrativeNode[] next)
    {
        this.next = next;

        foreach (NarrativeNode i in next)
        {
            i.Previous = this;
        }
    }
}