using System;
using System.Collections.Generic;
using Unity.Tutorials.Core.Editor;
using UnityEngine;

public class MiniBrain : MonoBehaviour
{
    [Header("Files")]
    [SerializeField, Tooltip("The base folder name within the Resources folder")]
    string path;
    [SerializeField, Tooltip("The corresponding local file names for the atlas info (names/descriptions and connectivity)")]
    string infoPath, connectivityPath;
    [SerializeField, Tooltip(
         "The corresponding local file names for the atlas info" +
         " (subsystem names/descriptions, subsystem connectivity, and subsystem connection descriptions)"
    )]
    string subsystemsInfoPath, subsystemsConnectivityPath, subsystemsConnectionDescriptionsPath;

    [Header("Visualization")]
    [SerializeField, Tooltip("The material to be applied to the structures")]
    Material material;
    [SerializeField, Tooltip("The material to be applied to the left half of the structures (if ignoreLeft is enabled)")]
    Material leftMaterial;
    [SerializeField, Tooltip("The colour to be applied to the ring around the origin")]
    Color originOutlineColour = Color.white;
    [SerializeField, Tooltip("The position and rotation for the new atlas")]
    Vector3 position, rotation;
    [SerializeField, Tooltip("The bounds scale of the atlas")]
    float scale = 2;
    [Tooltip("Whether or not to ignore every other structure in the list (starting with the first)")]
    public bool ignoreLeft;
    [SerializeField, Tooltip("Whether or not to hide the left side of the atlas")]
    bool hideLeft;
    [Tooltip("Whether or not to replace all selectable structures with spherical nodes")]
    public bool replaceWithNodes = true;
    
    [Header("Connectivity")]
    [SerializeField, Tooltip("The highest connectivity between structures in the whole matrix (should be pre-calculated)")]
    float highestValue;
    [SerializeField, Tooltip("The minimum strength that a connection can have for it to be displayed")]
    float thresholdPercentage = 0.1f;
    [Tooltip("The material to be applied to the connection visualizations")]
    public Material connectionMaterial;
    
    [Header("Extra structures")]
    [SerializeField, Tooltip("The base folder name within the Resources folder for the extra structures")]
    string extraPath;
    [SerializeField, Tooltip("The corresponding local file name for the connectivity for the extra structures")]
    string extraConnectivityPath;
    [SerializeField, Tooltip("Whether or not to visualize the connectivity between the extra structures")]
    bool showExtraConnectivity = true;
    [SerializeField, Tooltip("The highest connectivity between structures in the whole matrix (should be pre-calculated)for the extra structures")]
    float extraHighestValue;
    [SerializeField, Tooltip("The minimum strength that a connection can have for it to be displayed for the extra structures")]
    float extraThresholdPercentage = 0.1f;
    [SerializeField, Tooltip("The material to be applied to the extra structures")]
    Material extraMaterial;
    [SerializeField, Tooltip("The material to be applied to the connection visualizations for the extra structures")]
    Material extraConnectionMaterial;
    [Tooltip("Whether or not to visualize the extra structures in the mini brain")]
    public bool miniExtraStructures = true;
    [Tooltip("Whether or not to visualize the extra structures in the big brain")]
    public bool bigExtraStructures = true;
    
    [HideInInspector] public Transform offset, extraOffset; // The offset parent Transform for the structures and the extra structures
    [HideInInspector] public AtlasInfo info; // The info class about the atlas

    void Start()
    {
        // If the left is hidden, it is also ignored by default
        if (hideLeft)
        {
            ignoreLeft = true;
        }
        
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

        // Adding an origin marker for visual aid
        GameObject origin = Instantiate(Resources.Load<GameObject>("Node"), transform);
        origin.transform.localScale = Vector3.one * 2;
        origin.GetComponent<MeshRenderer>().material = connectionMaterial;
        
        // Adding an outline to the origin marker
        Outline originOutline = origin.AddComponent<Outline>();
        originOutline.OutlineColor = originOutlineColour;
        originOutline.OutlineWidth = 3;
        originOutline.OutlineMode = Outline.Mode.OutlineVisible;
        
        // Initializing the atlas info
        info = new AtlasInfo(
            path,
            infoPath,
            connectivityPath,
            subsystemsInfoPath,
            subsystemsConnectivityPath,
            subsystemsConnectionDescriptionsPath
        );
        
        GameObject[] tempStructures = Resources.LoadAll<GameObject>(path);

        int structureLength = tempStructures.Length;

        // Splitting the structure length in half if the left is being ignored
        if (ignoreLeft)
        {
            structureLength /= 2;
            info.LeftStructures = new GameObject[structureLength];
        }
        
        info.Structures = new GameObject[structureLength];
        info.LocalCentres = new Vector3[structureLength];
        
        // The current index being added to the connected structures and the left structures
        int rightStructureCount = 0;
        int leftStructureCount = 0;

        for (int i = 0; i < tempStructures.Length; i++)
        {
            // Creating and initializing each structure
            GameObject temp = Instantiate(tempStructures[i], offset);
            temp.name = info.Names[i];
            
            // Adding the structures if they're on the right, or the left isn't being ignored
            if (!ignoreLeft || (ignoreLeft && i % 2 == 1))
            {
                temp.GetComponent<MeshRenderer>().material = material;
                temp.AddComponent<MeshCollider>();
                
                // Setting atlas info variables
                info.Structures[rightStructureCount] = temp;
                info.LocalCentres[rightStructureCount] = new BoundsInfo(info.Structures[rightStructureCount]).GlobalCentre;

                rightStructureCount++;
            }
            // Adding the left structures
            else if (ignoreLeft)
            {
                temp.GetComponent<MeshRenderer>().material = leftMaterial;
                
                info.LeftStructures[leftStructureCount] = temp;

                if (hideLeft)
                {
                    temp.SetActive(false);
                }

                leftStructureCount++;
            }
        }

        // Initializing the connectivity variables
        info.ValidConnections = new List<GameObject>[info.Structures.Length];
        float thresholdValue = highestValue * thresholdPercentage;
        
        for (int j = 0; j < info.Structures.Length; j++)
        {
            // Creating a new list of connected GameObjects for each structure
            info.ValidConnections[j] = new List<GameObject>();
            
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
                    //line.sortingLayerName = "Connections";
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

        // Only setting the Subsystems' structures if there are subsystems (aka the files have been provided)
        if (info.Subsystems != null)
        {
            foreach (SubsystemInfo l in info.Subsystems)
            {
                l.SetValidStructures();
            }   
        }

        // Scaling the new atlas so that it fits to the scale
        BoundsInfo bounds = new BoundsInfo(offset.gameObject);
        transform.localScale = Vector3.one * (scale / bounds.Magnitude);

        // Making sure that the path to the extra structure is valid, and that it should be visualized somewhere
        if (extraPath.IsNotNullOrEmpty() && (miniExtraStructures || bigExtraStructures))
        {
            // Creating the extra structures' offset
            extraOffset = new GameObject("Extra Structures").transform;
            extraOffset.SetParent(transform);
            extraOffset.localPosition = position;
            extraOffset.localRotation = Quaternion.Euler(rotation);;
            extraOffset.localScale = Vector3.one;

            GameObject[] extraStructures = Resources.LoadAll<GameObject>(extraPath);

            // Instantiating each extra structure
            foreach (GameObject m in extraStructures)
            {
                GameObject tempExtra = Instantiate(m, extraOffset);
                tempExtra.GetComponent<MeshRenderer>().material = extraMaterial;
                Destroy(tempExtra.GetComponent<Collider>());
            }

            // Making sure that the connectivity path is valid, and that they should be visualized
            if (extraConnectivityPath.IsNotNullOrEmpty() && showExtraConnectivity)
            {
                // Getting teh extra structures' threshold value
                float extraThresholdValue = extraHighestValue * thresholdPercentage;
                
                // Loading the connectivity
                float[,] extraConnectivity = LoadFloatMatrix(extraConnectivityPath, ',');

                for (int m = 0; m < extraOffset.childCount; m++)
                {
                    for (int n = 0; n < extraOffset.childCount; n++)
                    {
                        // Making sure that each connection is valid
                        if (extraConnectivity[m, n] >= extraThresholdValue)
                        {
                            // Adding the valid connection lines
                            GameObject lineObject = new GameObject("Connection to " + extraStructures[n].name);
                            lineObject.transform.SetParent(extraOffset.GetChild(m).transform);
                    
                            LineRenderer line = lineObject.AddComponent<LineRenderer>();
                            line.material = extraConnectionMaterial;
                            line.widthMultiplier = 0.001f;

                            // Setting the connection lines to the bounds centres of each structure
                            line.useWorldSpace = false;
                            line.SetPositions(
                                new [] {
                                    new BoundsInfo(extraOffset.GetChild(m).gameObject).GlobalCentre,
                                    new BoundsInfo(extraOffset.GetChild(n).gameObject).GlobalCentre
                                }
                            );
                        }
                    }
                }   
            }
            else
            {
                showExtraConnectivity = false;
            }
        }
        // If the path is invalid, the visualization variables are set to false
        // (so big brain doesn't try to copy them later)
        else
        {
            miniExtraStructures = false;
            bigExtraStructures = false;
        }
    }
    
    /// <summary>
    /// Creates a 2D array float matrix from the supplied file
    /// </summary>
    /// <param name="fileName">The file at Resources/basePath/fileName.csv</param>
    /// <param name="delim">The delimiter character for the rows in the file</param>
    /// <returns>The float matrix as a 2D array</returns>
    float[,] LoadFloatMatrix(string fileName, char delim)
    {
        TextAsset file = Resources.Load<TextAsset>(extraPath + "/" + fileName);

        if (file != null)
        {
            string[] split = file.text.Split('\n'); // Splitting by line
            float[,] temp = new float[split.Length, split.Length];
        
            for (int i = 0; i < split.Length; i++)
            {
                string[] splitSplit = split[i].Split(delim); // Splitting by comma
            
                for (int j = 0; j < splitSplit.Length; j++)
                {
                    temp[i, j] = float.Parse(splitSplit[j].Trim()); // Setting the values
                }
            }

            return temp;
        }

        return null;
    }

    /// <summary>
    /// Method to 'convert' each structure into a representative node
    /// </summary>
    public void ReplaceWithNodes()
    {
        for (int i = 0; i < info.Structures.Length; i++)
        {
            // Creating new nodes
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            temp.GetComponent<MeshRenderer>().material = material;
            Transform tempTransform = temp.transform;
            
            // Positioning the new nodes
            tempTransform.localScale = Vector3.one / 20f;
            tempTransform.localPosition = new BoundsInfo(info.Structures[i]).GlobalCentre;
            tempTransform.SetParent(info.Structures[i].transform);
            tempTransform.SetAsFirstSibling();
            
            // Hiding the structures
            info.Structures[i].GetComponent<MeshRenderer>().enabled = false;
            info.Structures[i].GetComponent<MeshCollider>().enabled = false;
        }
    }
}
