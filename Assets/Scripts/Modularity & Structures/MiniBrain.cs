using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// MonoBehaviour to generate the visual representations of the brain atlas's structures and connections
/// </summary>
/// <organization>Health-X Lab</organization>
/// <project>Insideout (May-August 2022)</project>
/// <author>Owen Hellum</author>
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
    [SerializeField, Tooltip("Whether to force the structures to have to all be viewed first, before the subsystems")]
    bool structureSelectionFirst = true;

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
    [SerializeField, Tooltip(
         "Whether to set the structures of the brain to various unique flesh colours" +
         " (uses standing 'rainbow' colour coding otherwise)"
    )]
    bool useFleshColours = true;
    public LayerMask structureLayer;
    
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
    [SerializeField, Tooltip("Which indices (if any) of the structures should be generated in the mini brain")]
    int[] extraWhitelist;
    [SerializeField, Tooltip("The corresponding local file name for the connectivity for the extra structures")]
    string extraConnectivityPath;
    [SerializeField, Tooltip("Whether or not to visualize the connectivity between the extra structures")]
    bool showExtraConnectivity = true;
    [SerializeField, Tooltip(
         "The highest connectivity between structures in the whole matrix" +
         " (should be pre-calculated) for the extra structures"
    )]
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
    List<Color> usedColours; // The previously assigned flesh colours to the structures
    GameObject[] extraStructuresInBrain; // The array of the extra structures generated in the brain

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
        
        // Making sure some of the other scripts 'start' after this one has generated the AtlasInfo
        FindObjectOfType<CompletionController>().CompletionStart(structureSelectionFirst);
        FindObjectOfType<StructureInformation>().InformationStart();
        
        GameObject[] tempStructures = Resources.LoadAll<GameObject>(path);

        int structureLength = tempStructures.Length;

        // Splitting the structure length in half if the left is being ignored
        if (ignoreLeft)
        {
            structureLength /= 2;
            info.LeftStructures = new GameObject[structureLength];
        }

        // Setting the ranges that the flesh materials can be generated within
        Vector2 hueRange = new Vector2(0.9f, 1);
        Vector2 saturationRange = new Vector2(0.1f, 1);
        Vector2 valueRange = new Vector2(0.4f, 1.2f);

        // Modifying parts of the ranges so that the full scope is captured
        saturationRange.x -= 0.1f;
        valueRange.y += 0.1f;
        
        // Generating small intervals within the ranges (so hue, saturation, and value is equidistant from others)
        float hueInterval = (hueRange.y - hueRange.x) / structureLength;
        float saturationInterval = (saturationRange.y - saturationRange.x) / structureLength;
        float valueInterval = (valueRange.y - valueRange.x) / structureLength;

        usedColours = new List<Color>();
        
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
            temp.layer = LayerMask.NameToLayer("Selectable");

            // Adding the structures if they're on the right, or the left isn't being ignored
            if (!ignoreLeft || (ignoreLeft && i % 2 == 1))
            {
                MeshRenderer tempRenderer = temp.GetComponent<MeshRenderer>();
                tempRenderer.material = material;

                // Generating a unique colour for each structure if the left isn't being ignored
                if (!ignoreLeft)
                {
                    if (useFleshColours)
                    {
                        tempRenderer.material.color = GetFleshColourByIndex(
                            rightStructureCount,
                            hueRange.y,
                            saturationRange.y, 
                            valueRange.x,
                            hueInterval,
                            saturationInterval,
                            valueInterval
                        );

                        /*
                        tempRenderer.material.color = GetUniqueRandomFleshColour(
                            structureLength,
                            hueRange.y,
                            saturationRange.y, 
                            valueRange.x,
                            hueInterval,
                            saturationInterval,
                            valueInterval
                        );
                        */
                    }
                    else
                    {
                        // Getting the structure's equidistant unique colour along the spectrum 
                        tempRenderer.material.color = Color.HSVToRGB(
                            rightStructureCount * (1f / structureLength),
                            0.2f,
                            1
                        );
                    }
                }
                // Otherwise, copying the colour from the previously generated left structure
                else
                {
                    tempRenderer.material.color = info.LeftStructures[leftStructureCount - 1]
                        .GetComponent<MeshRenderer>().material.color;
                }
                
                temp.AddComponent<MeshCollider>();
                
                // Setting atlas info variables
                info.Structures[rightStructureCount] = temp;
                info.LocalCentres[rightStructureCount] = new BoundsInfo(info.Structures[rightStructureCount]).GlobalCentre;

                rightStructureCount++;
            }
            // Adding the left structures
            else if (ignoreLeft)
            {
                MeshRenderer tempRenderer = temp.GetComponent<MeshRenderer>();
                tempRenderer.material = leftMaterial;

                // Generating a unique colour for each left structure
                if (useFleshColours)
                {
                    tempRenderer.material.color = GetFleshColourByIndex(
                        leftStructureCount,
                        hueRange.y,
                        saturationRange.y, 
                        valueRange.x,
                        hueInterval,
                        saturationInterval,
                        valueInterval
                    );
                    
                    /*
                    // Generating a unique colour for each left structure
                    tempRenderer.material.color = GetUniqueRandomFleshColour(
                        structureLength,
                        hueRange.y,
                        saturationRange.y, 
                        valueRange.x,
                        hueInterval,
                        saturationInterval,
                        valueInterval
                    );
                    */
                }
                else
                {
                    // Getting the structure's equidistant unique colour along the spectrum
                    tempRenderer.material.color = Color.HSVToRGB(
                        leftStructureCount * (1f / structureLength),
                        0.2f,
                        1
                    );
                }

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
                    lineObject.transform.position = new BoundsInfo(info.Structures[j]).GlobalCentre;
                    
                    LineRenderer line = lineObject.AddComponent<LineRenderer>();
                    line.material = connectionMaterial;
                    line.widthMultiplier = 0.001f;
                    line.useWorldSpace = false;

                    // Setting the connection lines to the bounds centres of each structure
                    line.SetPositions(new []
                    {
                        Vector3.zero,
                        lineObject.transform.InverseTransformPoint(new BoundsInfo(info.Structures[k]).GlobalCentre)
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
        if (!string.IsNullOrEmpty(extraPath) && (miniExtraStructures || bigExtraStructures))
        {
            // Creating the extra structures' offset
            extraOffset = new GameObject("Extra Structures").transform;
            extraOffset.SetParent(transform);
            extraOffset.localPosition = position;
            extraOffset.localRotation = Quaternion.Euler(rotation);
            extraOffset.localScale = Vector3.one;

            GameObject[] extraStructures = Resources.LoadAll<GameObject>(extraPath);
            extraStructuresInBrain = new GameObject[extraStructures.Length];

            // Instantiating each extra structure
            for (int m = 0; m < extraStructures.Length; m++)
            {
                GameObject tempExtra = Instantiate(extraStructures[m], extraOffset);
                tempExtra.GetComponent<MeshRenderer>().material = extraMaterial;
                Destroy(tempExtra.GetComponent<Collider>());

                extraStructuresInBrain[m] = tempExtra; // Adding the newly-generated extra structure to the array
            }

            // Making sure that the connectivity path is valid, and that they should be visualized
            if (!string.IsNullOrEmpty(extraConnectivityPath) && showExtraConnectivity)
            {
                // Getting the extra structures' threshold value
                float extraThresholdValue = extraHighestValue * extraThresholdPercentage;
                
                // Loading the connectivity
                float[,] extraConnectivity = LoadFloatMatrix(extraConnectivityPath, ',');

                for (int n = 0; n < extraOffset.childCount; n++)
                {
                    for (int o = 0; o < extraOffset.childCount; o++)
                    {
                        // Making sure that each connection is valid and less than the number of structures
                        if (n < extraConnectivity.GetLength(0)
                            && o < extraConnectivity.GetLength(0)
                            && extraConnectivity[n, o] >= extraThresholdValue)
                        {
                            // Adding the valid connection lines
                            GameObject lineObject = new GameObject("Connection to " + extraStructures[o].name);
                            lineObject.transform.SetParent(extraOffset.GetChild(n).transform);
                    
                            LineRenderer line = lineObject.AddComponent<LineRenderer>();
                            line.material = extraConnectionMaterial;
                            line.widthMultiplier = 0.001f;
                            line.useWorldSpace = false;

                            // Setting the connection lines to the bounds centres of each structure
                            line.SetPositions(
                                new [] {
                                    new BoundsInfo(extraOffset.GetChild(n).gameObject).GlobalCentre,
                                    new BoundsInfo(extraOffset.GetChild(o).gameObject).GlobalCentre
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
    /// Method to generate a random colour along hue, saturation, and value ranges that hasn't been generated before
    /// </summary>
    /// <param name="len">The number of structures that will have flesh colours added to them</param>
    /// <param name="hueMax">The maximum amount that a hue can be</param>
    /// <param name="saturationMax">The maximum amount that a saturation can be</param>
    /// <param name="valueMin">The minimum amount that a value can be</param>
    /// <param name="hueInterval">A factor for making sure that all hues aren't too close/far from each other</param>
    /// <param name="saturationInterval">A factor for making sure that all saturations aren't too close/far from each other</param>
    /// <param name="valueInterval">A factor for making sure that all values aren't too close/far from each other</param>
    /// <returns>A unique randomly-generated fleshy colour</returns>
    Color GetUniqueRandomFleshColour(
        int len,
        float hueMax,
        float saturationMax,
        float valueMin,
        float hueInterval,
        float saturationInterval,
        float valueInterval)
    {
        // Setting the flesh colour initially
        Color temp = GetRandomFleshColour(
            len,
            hueMax,
            saturationMax,
            valueMin,
            hueInterval,
            saturationInterval,
            valueInterval
        );

        // If (for some strange reason) the used colours list is already full,
        // the execution ends and a non-unique colour is returned
        if (usedColours.Count >= len)
        {
            return temp;
        }
        
        // Looping and resetting while the generated colour already exists
        while (usedColours.Contains(temp))
        {
            temp = GetRandomFleshColour(
                len,
                hueMax,
                saturationMax,
                valueMin,
                hueInterval,
                saturationInterval,
                valueInterval
            );
        }
                    
        usedColours.Add(temp); // Making sure to add it to the list so that it isn't replicated in the future

        return temp;
    }

    /// <summary>
    /// Quick method to generate a random colour along hue, saturation, and value ranges
    /// </summary>
    /// <param name="len">The number of structures that will have flesh colours added to them</param>
    /// <param name="hueMax">The maximum amount that a hue can be</param>
    /// <param name="saturationMax">The maximum amount that a saturation can be</param>
    /// <param name="valueMin">The minimum amount that a value can be</param>
    /// <param name="hueInterval">A factor for making sure that all hues aren't too close/far from each other</param>
    /// <param name="saturationInterval">A factor for making sure that all saturations aren't too close/far from each other</param>
    /// <param name="valueInterval">A factor for making sure that all values aren't too close/far from each other</param>
    /// <returns>A randomly-generated fleshy colour</returns>
    Color GetRandomFleshColour(
        int len,
        float hueMax,
        float saturationMax,
        float valueMin,
        float hueInterval,
        float saturationInterval,
        float valueInterval)
    {
        return Color.HSVToRGB(
            hueMax - (Random.Range(0, len) * hueInterval),
            saturationMax - (Random.Range(0, len) * saturationInterval),
            valueMin + (Random.Range(0, len) * valueInterval)
        );
    }
    
    /// <summary>
    /// Quick method to generate a colour along hue, saturation, and value ranges at a certain index
    /// </summary>
    /// <param name="index">The index along the HSV ranges</param>
    /// <param name="hueMax">The maximum amount that a hue can be</param>
    /// <param name="saturationMax">The maximum amount that a saturation can be</param>
    /// <param name="valueMin">The minimum amount that a value can be</param>
    /// <param name="hueInterval">A factor for making sure that all hues aren't too close/far from each other</param>
    /// <param name="saturationInterval">A factor for making sure that all saturations aren't too close/far from each other</param>
    /// <param name="valueInterval">A factor for making sure that all values aren't too close/far from each other</param>
    /// <returns>A fleshy colour at the given index</returns>
    Color GetFleshColourByIndex(
        int index,
        float hueMax,
        float saturationMax,
        float valueMin,
        float hueInterval,
        float saturationInterval,
        float valueInterval)
    {
        return Color.HSVToRGB(
            hueMax - (index * hueInterval),
            saturationMax - (index * saturationInterval),
            valueMin + (index * valueInterval)
        );
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
        foreach (GameObject i in info.Structures)
        {
            // Creating new nodes
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            temp.layer = LayerMask.NameToLayer("Selectable");
            
            temp.GetComponent<MeshRenderer>().material = i.GetComponent<MeshRenderer>().material;
            
            // Making the collider radius larger so that the nodes can be hit easier
            temp.GetComponent<SphereCollider>().radius *= 1.5f;
            
            Transform tempTransform = temp.transform;
            
            // Positioning the new nodes
            tempTransform.localScale = Vector3.one / 20f;
            tempTransform.localPosition = new BoundsInfo(i).GlobalCentre;
            tempTransform.SetParent(i.transform);
            tempTransform.SetAsFirstSibling();
            
            // Hiding the structures
            i.GetComponent<MeshRenderer>().enabled = false;
            i.GetComponent<MeshCollider>().enabled = false;
        }
    }

    /// <summary>
    /// Method to destroy any of the unnecessary extra mini brain structures
    /// after they have been loaded into the big brain
    /// </summary>
    public void HideExtraStructures()
    {
        for (int i = 0; i < extraStructuresInBrain.Length; i++)
        {
            if (!extraWhitelist.Contains(i))
            {
                Destroy(extraStructuresInBrain[i]);
            }
        }
    }
}
