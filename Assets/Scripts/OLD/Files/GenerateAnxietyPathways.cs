using System;
using UnityEngine;

public class GenerateAnxietyPathways : MonoBehaviour
{
    [Tooltip("The .txt files to be read and had Narratives created from")]
    [SerializeField] TextAsset[] files;

    [Header("Materials and structures")]
    [Tooltip("The default material for each structure")]
    [SerializeField] Material defaultMaterial;
    [Tooltip("The material when the structure is occluding")]
    [SerializeField] Material occlusionMaterial;
    [Tooltip("The actual GameObjects in the scene that will be linked up to individual NarrativeNodes")]
    [SerializeField] GameObject amygdala, mPFC, hippocampus, BNST, striatum, hypothalamus;

    Narrative[] narratives; // The Narratives generated from the files
    PathwayController controller; // The Pathway control/management script

    void Start()
    {
        // Initializing the Pathway controller
        controller = gameObject.AddComponent<PathwayController>();
        controller.defaultMaterial = defaultMaterial;
        controller.occlusionMaterial = occlusionMaterial;
        
        // Occluding all the possible Narrative structures
        controller.SetStructure(amygdala, false);
        controller.SetStructure(mPFC, false);
        controller.SetStructure(hippocampus, false);
        controller.SetStructure(BNST, false);
        controller.SetStructure(striatum, false);
        controller.SetStructure(hypothalamus, false);
        
        // Initializing a necessary variable in PathwaySelectionManager
        PathwaySelectionManager manager = FindObjectOfType<PathwaySelectionManager>();
        manager.pathways = new Pathway[files.Length];
        
        // Initializing the narrative variables
        FileToNarrative temp = new FileToNarrative();
        narratives = new Narrative[files.Length];

        // Creating Pathways for each Narrative
        for (int i = 0; i < files.Length; i++)
        {
            // Loading the narrative from the file
            temp.Load(files[i]);
            narratives[i] = temp.Narrative;

            GameObject newNarrative = new GameObject(narratives[i].Name);
            newNarrative.transform.SetParent(transform);
            
            // Creating the new Pathway and setting its variables
            Pathway newPathway = newNarrative.AddComponent<Pathway>();
            newPathway.name = narratives[i].Name;
            newPathway.hideVisualization = true;
            newPathway.isNarrativePathway = true;
            newPathway.bidirectional = true;

            newPathway.nodes = new GameObject[temp.Nodes.Count];

            // Linking the NarrativeNodes up with the scene GameObjects 
            for (int j = 0; j < temp.Nodes.Count; j++)
            {
                temp.Nodes[j].Object = FindGameObject(temp.Nodes[j].ObjectName);
                
                newPathway.nodes[j] = temp.Nodes[j].Object; // Adding the nodes to the Pathway script
            }
            
            // Initializing the Pathway's narrative variables
            newPathway.narrative = narratives[i];
            newPathway.narrativeNodes = temp.Nodes.ToArray();
            
            manager.pathways[i] = newPathway; // Adding to the necessary variable in PathwaySelectionManager
        }
    }

    /// <summary>
    /// Finds the scene GameObject associated with the given string code (and sets it visible)
    /// </summary>
    /// <param name="name">The code name of the GameObject to be found</param>
    /// <returns>The GameObject that corresponds to the given code</returns>
    GameObject FindGameObject(string name)
    {
        switch (name)
        {
            case "Amygdala":
                controller.SetStructure(amygdala, true);
                return amygdala;
            case "mPFC":
                controller.SetStructure(mPFC, true);
                return mPFC;
            case "Hippocampus":
                controller.SetStructure(hippocampus, true);
                return hippocampus;
            case "BNST":
                controller.SetStructure(BNST, true);
                return BNST;
            case "Striatum":
                controller.SetStructure(striatum, true);
                return striatum;
            case "Hypothalamus":
                controller.SetStructure(hypothalamus, true);
                return hypothalamus;
            default:
                return null;
        }
    }
}