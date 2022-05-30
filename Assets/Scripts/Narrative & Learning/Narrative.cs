using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that represents a branching narrative tree
/// </summary>
public class Narrative
{
    /// <summary>
    /// The name of the Narrative
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// The description of the Narrative
    /// </summary>
    public string Description { get; }
    
    /// <summary>
    /// The initial node from which the Narrative begins
    /// </summary>
    public NarrativeNode Start { get; set; }

    /// <summary>
    /// The current node that the Narrative has progressed to
    /// </summary>
    public NarrativeNode Current { get; private set; }

    /// <summary>
    /// The path of all nodes visited by the user throughout the Narrative
    /// </summary>
    public List<NarrativeNode> Path { get; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public Narrative()
    {
        Name = "";
        Description = "";
        Start = null;
        Current = Start;
        Path = new List<NarrativeNode>();
    }
    
    /// <summary>
    /// Name parameterized constructor
    /// </summary>
    /// <param name="name">The name of the Narrative</param>
    public Narrative(string name)
    {
        Name = name;
        Description = "";
        Start = null;
        Current = Start;
        Path = new List<NarrativeNode>();
    }
    
    /// <summary>
    /// Name and description parameterized constructor
    /// </summary>
    /// <param name="name">The name of the Narrative</param>
    /// <param name="description">The description of the Narrative</param>
    public Narrative(string name, string description)
    {
        Name = name;
        Description = description;
        Start = null;
        Current = Start;
        Path = new List<NarrativeNode>();
    }
    
    /// <summary>
    /// Name and start parameterized constructor
    /// </summary>
    /// <param name="name">The name of the Narrative</param>
    /// <param name="description">The description of the Narrative</param>
    /// <param name="start">the initial node from which the Narrative begins</param>
    public Narrative(string name, string description, NarrativeNode start)
    {
        Name = name;
        Description = description;
        Start = start;
        Current = Start;
        Path = new List<NarrativeNode> { Current };
    }

    /// <summary>
    /// Method to progress the Narrative to one of the next possible nodes
    /// </summary>
    /// <param name="index">Index of the next node among the array of possible next nodes</param>
    public void GoToNext(int index)
    {
        if (Current.Next != null && index < Current.Next.Length)
        {
            Current = Current.Next[index];
            Path.Add(Current);
        }
    }

    /// <summary>
    /// Method to regress the Narrative to the previous node
    /// </summary>
    public void GoToPrevious()
    {
        if (Current.Previous != null)
        {
            Current = Current.Previous;
            Path.Add(Current);
        }
    }

    /// <summary>
    /// Recursive method to find a NarrativeNode in the Narrative with a corresponding name
    /// </summary>
    /// <param name="name">Name of the NarrativeNode to be found</param>
    /// <param name="node">The NarrativeNode currently being checked</param>
    /// <returns>The retrieved NarrativeNode (null if not found)</returns>
    public NarrativeNode FindNode(string name, NarrativeNode node)
    {
        if (node.Name.Equals(name))
        {
            return node;
        }

        if (node.Next != null)
        {
            foreach (NarrativeNode i in node.Next)
            {
                NarrativeNode temp = FindNode(name, i);

                if (temp != null)
                {
                    return temp;
                }
            }
        }

        return null;
    }
}