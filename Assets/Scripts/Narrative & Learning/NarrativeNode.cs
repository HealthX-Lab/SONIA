public class NarrativeNode
{
    string Description { set; get; }
    bool IsEnd { set; get; }
    NarrativeNode[] Next { set; get; }

    public NarrativeNode()
    {
        Description = "";
        IsEnd = false;
        Next = null;
    }
    
    public NarrativeNode(string description)
    {
        Description = description;
        IsEnd = false;
        Next = null;
    }
    
    public NarrativeNode(string description, bool isEnd, NarrativeNode[] next)
    {
        Description = description;
        IsEnd = isEnd;
        Next = next;
    }
}