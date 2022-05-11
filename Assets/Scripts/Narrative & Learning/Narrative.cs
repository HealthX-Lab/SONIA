using System.Collections.Generic;

/// <summary>
/// Class that represents a branching narrative tree
/// </summary>
public class Narrative
{
    /// <summary>
    /// The name of the narrative
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// The initial node from which the narrative begins
    /// </summary>
    public NarrativeNode Start { get; set; }
    
    NarrativeNode current;
    /// <summary>
    /// The current node that the narrative has progressed to
    /// </summary>
    public NarrativeNode Current => current;

    List<NarrativeNode> path;
    /// <summary>
    /// The path of all nodes visited by the user throughout the narrative
    /// </summary>
    public List<NarrativeNode> Path => path;

    /// <summary>
    /// Default constructor
    /// </summary>
    public Narrative()
    {
        Name = "";
        Start = null;
        current = Start;
        path = new List<NarrativeNode>();
    }
    
    /// <summary>
    /// Name parameterized constructor
    /// </summary>
    /// <param name="name">The name of the narrative</param>
    public Narrative(string name)
    {
        Name = name;
        Start = null;
        current = Start;
        path = new List<NarrativeNode>();
    }
    
    /// <summary>
    /// Name and start parameterized constructor
    /// </summary>
    /// <param name="name">The name of the narrative</param>
    /// <param name="start">the initial node from which the narrative begins</param>
    public Narrative(string name, NarrativeNode start)
    {
        Name = name;
        Start = start;
        current = Start;
        path = new List<NarrativeNode> {current};
    }

    /// <summary>
    /// Method to progress the narrative to one of the next possible nodes
    /// </summary>
    /// <param name="index">Index of the next node among the array of possible next nodes</param>
    public void GoToNext(int index)
    {
        if (current.Next != null && index < current.Next.Length)
        {
            current = current.Next[index];
            path.Add(current);
        }
    }

    /// <summary>
    /// Method to regress the narrative to the previous node
    /// </summary>
    public void GoToPrevious()
    {
        if (current.Previous != null)
        {
            current = current.Previous;
            path.Add(current);
        }
    }
}