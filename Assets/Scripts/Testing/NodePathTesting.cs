using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodePathTesting : MonoBehaviour
{
    [SerializeField] GameObject[] nodePositions;
    [SerializeField] float timeBetweenTransitions = 10;

    Narrative narrative;
    StructureZoom zoom;

    void Start()
    {
        zoom = GetComponent<StructureZoom>();
        
        NarrativeNode last = null;
        
        for (int i = nodePositions.Length; i > 0; i--)
        {
            NarrativeNode temp = new NarrativeNode("Node " + i, "DESCRIPTION", nodePositions[i - 1]);

            if (last != null)
            {
                temp.SetNext(new []{ last }, new []{"PATHWAY DESCRIPTION"});
            }

            last = temp;
        }

        narrative = new Narrative("Narrative", last);
        
        GoToNext();
    }

    void GoToNext()
    {
        zoom.target = narrative.Current.Object;
        narrative.GoToNext(0);

        if (narrative.Current != null)
        {
            Invoke("GoToNext", timeBetweenTransitions);
        }
    }
}
