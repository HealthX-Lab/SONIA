using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Class to generate and store info about the loaded atlas
/// </summary>
public class AtlasInfo
{
    /// <summary>
    /// The base path within Resources that the files for the atlas reside
    /// </summary>
    readonly string basePath;
    
    /// <summary>
    /// The names of each GameObject in the atlas
    /// </summary>
    public string[] Names { get; }
    
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
    public string[,] SubsystemConnectionDescriptions { get; }

    /// <summary>
    /// Classes corresponding to subsystems within the structures
    /// </summary>
    public SubsystemInfo[] Subsystems { get; }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="basePath">The folder path with in Resources that the structures and connectivity file are located at</param>
    /// <param name="infoPath">Name of the structure name/description file after the base path</param>
    /// <param name="connectivityPath">Name of the structure connectivity file after the base path</param>
    /// <param name="subsystemsInfoPath">Name of the subsystem name/description file after the base path</param>
    /// <param name="subsystemsConnectivityPath">Name of the subsystem's included structures file after the base path</param>
    /// <param name="subsystemsConnectionDescriptionsPath">Name of the structure connection descriptions file after the base path</param>
    public AtlasInfo(
        string basePath,
        string infoPath,
        string connectivityPath,
        string subsystemsInfoPath,
        string subsystemsConnectivityPath,
        string subsystemsConnectionDescriptionsPath)
    {
        this.basePath = basePath;
        
        // Setting the structure names/descriptions, connectivity, and connection descriptions
        Names = LoadStringColumn(infoPath, 0, '|', true, false);
        Descriptions = LoadStringColumn(infoPath, 1, '|', false, true);
        Connectivity = LoadFloatMatrix(connectivityPath, ',');
        SubsystemConnectionDescriptions = LoadStringMatrix(subsystemsConnectionDescriptionsPath, '|');

        string[] subsystemNames = LoadStringColumn(subsystemsInfoPath, 0, '|', false, false); // Getting the subsystem names (if they exist)

        if (subsystemNames != null)
        {
            // Loading the descriptions and initializing the Subsystems
            string[] subsystemDescriptions = LoadStringColumn(subsystemsInfoPath, 1, '|', false, false);
            Subsystems = new SubsystemInfo[subsystemNames.Length];

            for (int i = 0; i < subsystemNames.Length; i++)
            {
                float[] subsystemFloatConnectivity = LoadFloatRow(subsystemsConnectivityPath, i, ',');
                bool[] subsystemBoolConnectivity = new bool[subsystemFloatConnectivity.Length];

                // Setting the included structures for each Subsystem
                for (int j = 0; j < subsystemFloatConnectivity.Length; j++)
                {
                    if (subsystemFloatConnectivity[j] > 0)
                    {
                        subsystemBoolConnectivity[j] = true;
                    }
                }
            
                // Creating the new Subsystem
                Subsystems[i] = new SubsystemInfo(
                    this,
                    subsystemNames[i], 
                    subsystemDescriptions[i],
                    Color.HSVToRGB(i * (1f / subsystemNames.Length), 1, 1),
                    subsystemBoolConnectivity
                );
            }   
        }
    }

    /// <summary>
    /// Method to check and load a TextAsset (.csv) file after the base path
    /// </summary>
    /// <param name="fileName">The file at Resources/basePath/fileName.csv</param>
    /// <returns>The TextAsset file at the given location</returns>
    TextAsset LoadFile(string fileName) { return Resources.Load<TextAsset>(basePath + "/" + fileName); }

    /// <summary>
    /// Creates an array of string values from a column in a file
    /// </summary>
    /// <param name="fileName">The file at Resources/basePath/fileName.csv</param>
    /// <param name="columnIndex">The index of the column to be read from</param>
    /// <param name="delim">The delimiter character for the rows in the file</param>
    /// <param name="format">Whether or not to format the values of the column</param>
    /// <param name="ignoreLeft">Whether or not to ignore every other structure in the list (starting with the first)</param>
    /// <returns>A string array of the values in the column (null if the file doesn't exist)</returns>
    string[] LoadStringColumn(string fileName, int columnIndex, char delim, bool format, bool ignoreLeft)
    {
        TextAsset file = LoadFile(fileName);

        // Making sure the file exists
        if (file != null)
        {
            string[] split = file.text.Split('\n'); // Splitting by line

            int splitLength = split.Length;
            int rightIndex = 0;

            // Splitting the returned array's length in half if the left is being ignored
            if (ignoreLeft)
            {
                splitLength /= 2;
            }
            
            string[] temp = new string[splitLength];

            for (int i = 0; i < split.Length; i++)
            {
                // Adding the value if the left isn't being ignored, or if it is, grabbing every other index
                if (!ignoreLeft || i % 2 == 1)
                {
                    int index = i;

                    if (ignoreLeft)
                    {
                        index = rightIndex;
                    }
                    
                    temp[index] = split[i].Split(delim)[columnIndex].Trim(); // Getting the value in the column

                    // Formatting the string (if requested)
                    if (format)
                    {
                        temp[index] = FormatName(temp[index]);
                    }
                    
                    rightIndex++; // Increasing the index for the smaller version of the array if the left is being ignored
                }
            }

            return temp;
        }

        return null;
    }

    /// <summary>
    /// Creates an array of float values from a row in a file
    /// </summary>
    /// <param name="fileName">The file at Resources/basePath/fileName.csv</param>
    /// <param name="rowIndex">The index of the row to be read from</param>
    /// <param name="delim">The delimiter character for the rows in the file</param>
    /// <returns>A string array of the values in the row (null if the file doesn't exist)</returns>
    float[] LoadFloatRow(string fileName, int rowIndex, char delim)
    {
        TextAsset file = LoadFile(fileName);

        // Making sure the file exists
        if (file != null)
        {
            string[] split = file.text.Split('\n')[rowIndex].Split(delim); // Getting the array of values for the row
            float[] temp = new float[split.Length];

            for (int i = 0; i < split.Length; i++)
            {
                temp[i] = float.Parse(split[i].Trim()); // Getting the value in each index of the row
            }

            return temp;
        }

        return null;
    }

    /// <summary>
    /// Creates a 2D array float matrix from the supplied file
    /// </summary>
    /// <param name="fileName">The file at Resources/basePath/fileName.csv</param>
    /// <param name="delim">The delimiter character for the rows in the file</param>
    /// <returns>The float matrix as a 2D array</returns>
    float[,] LoadFloatMatrix(string fileName, char delim)
    {
        TextAsset file = LoadFile(fileName);

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
    /// Creates a 2D array string matrix from the supplied file
    /// </summary>
    /// <param name="fileName">The file at Resources/basePath/fileName.csv</param>
    /// <param name="delim">The delimiter character for the rows in the file</param>
    /// <returns>The string matrix as a 2D array</returns>
    string[,] LoadStringMatrix(string fileName, char delim)
    {
        TextAsset file = LoadFile(fileName);

        if (file != null)
        {
            string[] split = file.text.Split('\n'); // Splitting by line
            string[,] temp = new string[split.Length, split.Length];
        
            for (int i = 0; i < split.Length; i++)
            {
                string[] splitSplit = split[i].Split(delim); // Splitting by comma
            
                for (int j = 0; j < splitSplit.Length; j++)
                {
                    temp[i, j] = splitSplit[j].Trim(); // Setting the values
                }
            }

            return temp;
        }

        return null;
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