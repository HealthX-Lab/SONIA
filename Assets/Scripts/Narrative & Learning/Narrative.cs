using System.Collections.Generic;

/// <summary>
/// Class that represents a branching narrative tree
/// </summary>
public class Narrative
{
    /// <summary>
    /// The name of the narrative
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// The initial node from which the narrative begins
    /// </summary>
    public NarrativeNode Start { get; set; }

    /// <summary>
    /// The current node that the narrative has progressed to
    /// </summary>
    public NarrativeNode Current { get; private set; }

    /// <summary>
    /// The path of all nodes visited by the user throughout the narrative
    /// </summary>
    public List<NarrativeNode> Path { get; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public Narrative()
    {
        Name = "";
        Start = null;
        Current = Start;
        Path = new List<NarrativeNode>();
    }
    
    /// <summary>
    /// Name parameterized constructor
    /// </summary>
    /// <param name="name">The name of the narrative</param>
    public Narrative(string name)
    {
        Name = name;
        Start = null;
        Current = Start;
        Path = new List<NarrativeNode>();
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
        Current = Start;
        Path = new List<NarrativeNode> { Current };
    }

    /// <summary>
    /// Method to progress the narrative to one of the next possible nodes
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
    /// Method to regress the narrative to the previous node
    /// </summary>
    public void GoToPrevious()
    {
        if (Current.Previous != null)
        {
            Current = Current.Previous;
            Path.Add(Current);
        }
    }
}