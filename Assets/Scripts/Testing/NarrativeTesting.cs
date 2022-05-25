using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NarrativeTesting : MonoBehaviour
{
    [SerializeField] TMP_Text narrativeName, nextButton, previousButton;
    [SerializeField] TMP_InputField nextIndex;
    [SerializeField] TMP_Text nodeName, nodeObject, nodeDescription;
    
    Narrative narrative;
    
    void Start()
    {
        NarrativeNode n1 = new NarrativeNode("Node 1", "Node 1's description", null);
        NarrativeNode n2 = new NarrativeNode("Node 2", "Node 2's description", null);
        NarrativeNode n3 = new NarrativeNode("Node 3", "Node 3's description", null);
        NarrativeNode n4 = new NarrativeNode("Node 4", "Node 4's description", null);
        NarrativeNode n5 = new NarrativeNode("Node 5", "Node 5's description", null);
        
        n1.SetNext(new []{ n2, n3 }, new []{"PATHWAY DESCRIPTION", "PATHWAY DESCRIPTION"});
        n3.SetNext(new []{ n4, n5 }, new []{"PATHWAY DESCRIPTION", "PATHWAY DESCRIPTION"});
        
        narrative = new Narrative("Narrative 1", n1);
        
        SetFields();
    }

    public void GoToNext()
    {
        narrative.GoToNext(int.Parse(nextIndex.text));
        SetFields();
    }
    
    public void GoToPrevious()
    {
        narrative.GoToPrevious();
        SetFields();
    }

    void SetFields()
    {
        narrativeName.text = narrative.Name;

        if (narrative.Current.Next != null)
        {
            nextButton.transform.parent.gameObject.SetActive(true);
            nextButton.text = "Next (" + narrative.Current.Next.Length + " options(s) available)";
            nextIndex.gameObject.SetActive(true);
        }
        else
        {
            nextButton.transform.parent.gameObject.SetActive(false);
            nextIndex.gameObject.SetActive(false);
        }
        
        nextIndex.text = "0";
        
        if (narrative.Current.Previous != null)
        {
            previousButton.transform.parent.gameObject.SetActive(true);
            previousButton.text = "Previous";
        }
        else
        {
            previousButton.transform.parent.gameObject.SetActive(false);
        }

        nodeName.text = narrative.Current.Name;
        nodeObject.text = "Object: " + narrative.Current.Object;
        nodeDescription.text = narrative.Current.Description;

        string temp = "Path: ";
        
        foreach (NarrativeNode i in narrative.Path)
        {
            temp += i.Name + ", ";
        }
        
        print(temp.Substring(0, temp.Length - 2));
    }
}
