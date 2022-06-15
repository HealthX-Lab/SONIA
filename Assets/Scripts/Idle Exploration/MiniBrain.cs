using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniBrain : MonoBehaviour
{
    [Header("Files")]
    [Tooltip("The base folder name within the Resources folder")]
    [SerializeField] string path = "FBX";
    [Tooltip("The .csv of names of structures within the base folder")]
    [SerializeField] string infoPath = "aal_MNI_info";
    [Tooltip("The .csv connectivity matrix within the base folder")]
    [SerializeField] string connectivityPath = "T1-connectivity-AAL118";
    
    [Header("Visualization")]
    [Tooltip("The material to be applied to the structures")]
    [SerializeField] Material material;
    [Tooltip("The position and rotation for the new atlas")]
    [SerializeField] Vector3 position, rotation;
    [Tooltip("The bounds scale of the atlas")]
    [SerializeField] float scale = 1;
    
    [Header("Connectivity")]
    [Tooltip("The highest connectivity between structures in the whole matrix (should be pre-calculated)")]
    [SerializeField] float highestValue = 0.5332578f;
    [Tooltip("The minimum strength that a connection can have for it to be displayed")]
    [SerializeField] float thresholdPercentage = 0.1f;
    [Tooltip("The material to be applied to the connection visualizations")]
    [SerializeField] Material connectionMaterial;

    [HideInInspector] public Transform offset; // The offset parent Transform for the structures
    [HideInInspector] public AtlasInfo info; // The info class about the atlas

    void Start()
    {
        Generate();
    }

    /// <summary>
    /// Generates a new brain atlas as a child
    /// </summary>
    void Generate()
    {
        // Creating and orienting the offset parent
        offset = new GameObject("Offset").transform;
        offset.SetParent(transform);
        offset.localPosition = position;
        offset.localRotation = Quaternion.Euler(rotation);
        
        // Initializing the atlas info
        info = new AtlasInfo(path, connectivityPath);
        info.Structures = Resources.LoadAll<GameObject>(path);
        info.LocalCentres = new Vector3[info.Structures.Length];
        
        string[] names = LoadNames(Resources.Load<TextAsset>(path + "/" + infoPath)); // Loading the names of each structure
        
        for (int i = 0; i < info.Structures.Length; i++)
        {
            // Creating and initializing each structure
            GameObject temp = Instantiate(info.Structures[i], offset);
            temp.name = names[i];
            temp.GetComponent<MeshRenderer>().material = material;
            temp.AddComponent<MeshCollider>();

            // Setting atlas info variables
            info.Structures[i] = temp;
            info.LocalCentres[i] = new BoundsInfo(info.Structures[i]).GlobalCentre;
        }

        // Initializing the connectivity variables
        info.ValidConnections = new List<GameObject>[info.Structures.Length];
        float thresholdValue = highestValue * thresholdPercentage;
        
        for (int j = 0; j < info.Structures.Length; j++)
        {
            info.ValidConnections[j] = new List<GameObject>(); // Creating a new list of connected GameObjects for each structure
            
            for (int k = 0; k < info.Structures.Length; k++)
            {
                // Making sure that the connection is valid
                if (info.Connectivity[j, k] >= thresholdValue)
                {
                    info.ValidConnections[j].Add(info.Structures[k]);
                    
                    // Adding the valid connection lines
                    GameObject lineObject = new GameObject("Connection to " + info.Structures[k].name);
                    lineObject.transform.SetParent(info.Structures[j].transform);
                    
                    LineRenderer line = lineObject.AddComponent<LineRenderer>();
                    line.material = connectionMaterial;
                    line.widthMultiplier = 0.001f;

                    // Setting the connection lines to the bounds centres of each structure
                    line.useWorldSpace = false;
                    line.SetPositions(new []
                    {
                        info.LocalCentres[j],
                        info.LocalCentres[k]
                    });
                }
            }
        }

        // Scaling teh new atlas so that it fits to the scale
        BoundsInfo bounds = new BoundsInfo(offset.gameObject);
        offset.localScale = Vector3.one * (scale / bounds.Magnitude);
    }
    
    /// <summary>
    /// Creates an array of formatted names from the supplied file
    /// </summary>
    /// <param name="infoFile">The file within the Resources/path/infoFile.csv file</param>
    /// <returns>The formatted names as an array</returns>
    string[] LoadNames(TextAsset infoFile)
    {
        string[] split = infoFile.text.Split('\n'); // Splitting by line
        string[] temp = new string[split.Length];

        for (int i = 0; i < split.Length; i++)
        {
            temp[i] = FormatName(split[i].Split(',')[1].Trim()); // Getting the second cel of each line
        }

        return temp;
    }

    /// <summary>
    /// Removes underscores and add a (Right) or (Left) suffix for a name
    /// </summary>
    /// <param name="name">The given unformatted name to be converted</param>
    /// <returns>The given name with underscores removed and a suffix added</returns>
    string FormatName(string name)
    {
        string[] split = name.Split('_'); // Splitting by underscore
        string temp = "";

        for (int i = 0; i < split.Length; i++)
        {
            // Adding each word sequentially
            if (i != split.Length - 1)
            {
                temp += split[i];
                
                temp += " ";
            }
            else
            {
                // Adds (Right) if the last word is R
                if (split[i].Equals("R"))
                {
                    temp += "(Right)";
                }
                // Adds (Left) if the last word is L
                else if (split[i].Equals("L"))
                {
                    temp += "(Left)";
                }
                // If neither, it does as before
                else
                {
                    temp += split[i];
                }
            }
        }

        return temp;
    }
}
