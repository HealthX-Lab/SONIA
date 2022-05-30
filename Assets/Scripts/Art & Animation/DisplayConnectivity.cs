using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DisplayConnectivity : MonoBehaviour
{
    [Header("Files")]
    [Tooltip("The names and positions of the structures")]
    [SerializeField] TextAsset positions;
    [Tooltip("The connectivity matrix between structures")]
    [SerializeField] TextAsset connectivity;
    
    [Header("Validation")]
    [Tooltip("The highest connectivity between structures in the whole matrix (should be pre-calculated)")]
    [SerializeField] float highestValue = 0.5332578f;
    [Tooltip("The minimum strength that a connection can have for it to be displayed")]
    [SerializeField] float thresholdPercentage = 0.1f;
    [Tooltip("The indices in the connectivity matrix that should be ignored (0-based)")]
    [SerializeField] int[] ignoreIndices;
    
    [Header("Visualization")]
    [Tooltip("The material to be applied to the connection visualizations")]
    [SerializeField] Material connectionMaterial;
    [Tooltip("The width of the connection visualizations")]
    [SerializeField] float connectionWidth = 0.001f;

    GameObject[] nodeArray; // The array of all the nodes that are connected
    
    void Start()
    {
        // Initializing the array and threshold
        nodeArray = new GameObject[118];
        float thresholdValue = highestValue * thresholdPercentage;
        
        GameObject node = Resources.Load<GameObject>("Node");
        
        string[] positionsSplit = positions.text.Split('\n'); // Splitting the position file by line

        for (int i = 0; i < positionsSplit.Length; i++)
        {
            // Making sure that the index is not to be ignored
            if (!ignoreIndices.Contains(i))
            {
                string[] positionsSplitSplit = positionsSplit[i].Split(','); // Splitting each line by comma

                nodeArray[i] = Instantiate(node, transform); // Creating a new node for each position
            
                nodeArray[i].transform.localPosition = new Vector3(
                    -float.Parse(positionsSplitSplit[2].Trim()),
                    float.Parse(positionsSplitSplit[3].Trim()),
                    float.Parse(positionsSplitSplit[4].Trim())
                );
            }
        }
        
        string[] connectivitySplit = connectivity.text.Split('\n'); // Splitting the connectivity matrix file by line

        for (int j = 0; j < connectivitySplit.Length; j++)
        {
            // Making sure that the index is not to be ignored
            if (!ignoreIndices.Contains(j))
            {
                string[] connectivitySplitSplit = connectivitySplit[j].Split(','); // Splitting each line by comma

                for (int k = 0; k < connectivitySplitSplit.Length; k++)
                {
                    // Making sure that the index is not to be ignored and that it is >= to the threshold
                    if (!ignoreIndices.Contains(k) && float.Parse(connectivitySplitSplit[k].Trim()) >= thresholdValue)
                    {
                        // Creating a new connection visualization for each valid connection
                        GameObject line = new GameObject("Non-Narrative Edge");
                        line.transform.SetParent(nodeArray[j].transform);
                    
                        // Setting the LineRenderer values
                        LineRenderer temp = line.AddComponent<LineRenderer>();
                        temp.material = connectionMaterial;
                        temp.SetPositions(new []{ nodeArray[j].transform.position, nodeArray[k].transform.position });
                        temp.widthMultiplier = connectionWidth;
                        temp.useWorldSpace = false;
                    }
                }
            }
        }
    }
}
