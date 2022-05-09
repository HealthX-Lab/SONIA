public class Narrative
{
    string Name { get; set; }
    NarrativeNode StartNode { get; set; }

    public Narrative()
    {
        Name = "";
        StartNode = null;
    }
    
    public Narrative(string name)
    {
        Name = name;
        StartNode = null;
    }
    
    public Narrative(string name, NarrativeNode startNode)
    {
        Name = name;
        StartNode = startNode;
    }
}