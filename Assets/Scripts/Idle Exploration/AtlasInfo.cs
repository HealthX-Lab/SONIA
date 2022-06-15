using System.Collections.Generic;
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
    /// Default constructor
    /// </summary>
    /// <param name="basePath">The folder path with in Resources that the structures and connectivity file are located at</param>
    /// <param name="connectivityPath">Name of the connectivity file after the base path</param>
    public AtlasInfo(string basePath, string connectivityPath)
    {
        Connectivity = LoadConnectivity(Resources.Load<TextAsset>(basePath + "/" + connectivityPath));
    }

    /// <summary>
    /// Creates a 2D array connectivity matrix from the supplied file
    /// </summary>
    /// <param name="connectivityFile">The file within the Resources/basePath/connectivityPath.csv file</param>
    /// <returns>The connectivity matrix as a 2D array</returns>
    float[,] LoadConnectivity(TextAsset connectivityFile)
    {
        string[] split = connectivityFile.text.Split('\n'); // Splitting by line
        float[,] temp = new float[split.Length, split.Length];
        
        for (int i = 0; i < split.Length; i++)
        {
            string[] splitSplit = split[i].Split(','); // Splitting by comma
            
            for (int j = 0; j < split.Length; j++)
            {
                temp[i, j] = float.Parse(splitSplit[j].Trim()); // Setting the values
            }
        }

        return temp;
    }
}