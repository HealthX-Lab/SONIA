using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Class to hold arrays of info about loaded atlases
/// </summary>
public class AtlasInfo
{
    /// <summary>
    /// The actual structure GameObjects in the atlas
    /// </summary>
    public GameObject[] Structures { get; set; }
    
    /// <summary>
    /// The structures on the left side of the brain (if they are being ignored elsewhere)
    /// </summary>
    public GameObject[] LeftStructures { get; set; }

    /// <summary>
    /// The (local) centre positions of each structure
    /// </summary>
    public Vector3[] LocalCentres { get; set; }
    
    /// <summary>
    /// The connectivity matrix for each structure
    /// </summary>
    public float[,] Connectivity { get; }
    
    /// <summary>
    /// A list of connected GameObjects for each structure
    /// </summary>
    public List<GameObject>[] ValidConnections { get; set; }
    
    /// <summary>
    /// Descriptions corresponding to each structure
    /// </summary>
    public string[] Descriptions { get; }
    
    /// <summary>
    /// Descriptions corresponding to the connections between structures
    /// </summary>
    public string[,] ConnectionDescriptions { get; }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="basePath">The folder path with in Resources that the structures and connectivity file are located at</param>
    /// <param name="connectivityPath">Name of the connectivity file after the base path</param>
    public AtlasInfo(string basePath, string connectivityPath, string descriptionsPath, string connectionDescriptionsPath)
    {
        Connectivity = LoadConnectivity(Resources.Load<TextAsset>(basePath + "/" + connectivityPath));
        Descriptions = LoadDescriptions(Resources.Load<TextAsset>(basePath + "/" + descriptionsPath));
        ConnectionDescriptions = LoadConnectionDescriptions(Resources.Load<TextAsset>(basePath + "/" + connectionDescriptionsPath));
    }

    /// <summary>
    /// Creates a 2D array connectivity matrix from the supplied file
    /// </summary>
    /// <param name="connectivityFile">The file at Resources/basePath/connectivityFile.csv</param>
    /// <returns>The connectivity matrix as a 2D array</returns>
    float[,] LoadConnectivity(TextAsset connectivityFile)
    {
        string[] split = connectivityFile.text.Split('\n'); // Splitting by line
        float[,] temp = new float[split.Length, split.Length];
        
        for (int i = 0; i < split.Length; i++)
        {
            string[] splitSplit = split[i].Split(','); // Splitting by comma
            
            for (int j = 0; j < splitSplit.Length; j++)
            {
                temp[i, j] = float.Parse(splitSplit[j].Trim()); // Setting the values
            }
        }

        return temp;
    }

    /// <summary>
    /// Creates an array of descriptions from the supplied file
    /// </summary>
    /// <param name="descriptionsFile">The file at Resources/basePath/descriptionsFile.csv</param>
    /// <returns>The description array</returns>
    string[] LoadDescriptions(TextAsset descriptionsFile)
    {
        string[] split = descriptionsFile.text.Split('\n'); // Splitting by line
        
        for (int i = 0; i < split.Length; i++)
        {
            split[i] = split[i].Trim();
        }

        return split;
    }
    
    /// <summary>
    /// Creates a 2D array connection description matrix from the supplied file
    /// </summary>
    /// <param name="descriptionsFile">The file at Resources/basePath/descriptionsFile.csv</param>
    /// <returns>The connection description matrix as a 2D array</returns>
    string[,] LoadConnectionDescriptions(TextAsset descriptionsFile)
    {
        string[] split = descriptionsFile.text.Split('\n'); // Splitting by line
        string[,] temp = new string[split.Length, split.Length];
        
        for (int i = 0; i < split.Length; i++)
        {
            string[] splitSplit = split[i].Split(','); // Splitting by comma
            
            for (int j = 0; j < splitSplit.Length; j++)
            {
                temp[i, j] = splitSplit[j].Trim(); // Setting the values
            }
        }

        return temp;
    }

    /// <summary>
    /// Returns the index of the given object in the structure array
    /// </summary>
    /// <param name="obj">The object being searched for</param>
    /// <returns>The index that the object is at in the array (0-based) (-1 if not found)</returns>
    public int IndexOf(GameObject obj) { return ArrayUtility.IndexOf(Structures, obj); }

    /// <summary>
    /// Returns the GameObject in the structure array with the given name
    /// </summary>
    /// <param name="name">The name of the GameObject being searched for</param>
    /// <returns>The GameObject with the given name (null if not found)</returns>
    public GameObject Find(string name)
    {
        foreach (GameObject i in Structures)
        {
            if (i.name.Equals(name))
            {
                return i;
            }
        }

        return null;
    }
}