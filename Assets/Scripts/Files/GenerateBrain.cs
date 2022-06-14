using System;
using System.Linq;
using UnityEngine;

public class GenerateBrain : MonoBehaviour
{
    [SerializeField] string path = "FBX";
    [SerializeField] Material material;
    [SerializeField] int[] keyStructureIndices;

    GeneratePathways generatePathways;

    void Start()
    {
        generatePathways = GetComponent<GeneratePathways>();
        generatePathways.keyStructures = new GameObject[keyStructureIndices.Length];
        int keyStructuresAdded = 0;
        
        Mesh[] meshes = Resources.LoadAll<Mesh>(path); // TODO: this will need to be replaced with something external
        GameObject brain = new GameObject("Brain");
        FindObjectOfType<UIManager>().brain = brain;

        for (int i = 0; i < meshes.Length; i++)
        {
            GameObject temp = new GameObject(meshes[i].name);
            temp.transform.SetParent(brain.transform);
            temp.AddComponent<MeshFilter>().mesh = meshes[i];
            temp.AddComponent<MeshRenderer>().material = material;
            temp.AddComponent<MeshCollider>();
            
            if (keyStructureIndices.Contains(i))
            {
                StructureUIController structureUI = temp.AddComponent<StructureUIController>();
                structureUI.name = temp.name;
                structureUI.layer = new LayerMask();
                
                generatePathways.keyStructures[keyStructuresAdded] = temp;
                keyStructuresAdded++;
            }
        }
        
        generatePathways.Generate();
    }
}