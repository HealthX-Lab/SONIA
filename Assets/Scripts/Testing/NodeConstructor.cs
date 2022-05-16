using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class NodeConstructor : MonoBehaviour
{
    [SerializeField] string fileName = "aal_MNI_info.csv";
    
    string path;
    GameObject[] nodes;

    void Start()
    {
        GameObject node = Resources.Load<GameObject>("Node");
        
        string[] lines = LoadLines();
        nodes = new GameObject[lines.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            string[] split = lines[i].Split(',');

            GameObject temp = Instantiate(node, transform);
            temp.name = "[" + split[0] + "] " + split[1];
            temp.GetComponent<MeshRenderer>().material.color = Random.ColorHSV();

            temp.transform.localPosition = new Vector3(
                int.Parse(split[2]),
                int.Parse(split[3]),
                int.Parse(split[4])
            );

            nodes[i] = temp;
        }

        for (int j = 0; j < nodes.Length; j++)
        {
            for (int k = 0; k < 2; k++)
            {
                Debug.DrawLine(nodes[j].transform.position, nodes[Random.Range(0, nodes.Length)].transform.position, Color.cyan, Single.PositiveInfinity);
            }
        }
    }

    void CheckPath()
    {
        if (path == null)
        {
            path = Application.dataPath + "/Resources/";
        }
    }
    
    /*
    public void Empty()
    {
        CheckPath();
        
        File.WriteAllText(path + fileName, "");
    }

    void SaveLine(string line)
    {
        CheckPath();
        
        string temp = "";

        try
        {
            // Making sure to add the new line as a new line
            // instead of just at the end of the last line
            if (File.ReadAllLines(path + fileName).Length != 0)
            {
                temp += "\n";
            }
        }
        catch { }

        temp += line;
        
        File.AppendAllText(path + fileName, temp); // Appending the new line
        
        AssetDatabase.Refresh(); // Allowing the file to be immediately accessible
    }
    */
    
    string[] LoadLines()
    {
        CheckPath();
        
        try
        {
            return File.ReadAllLines(path + fileName);
        }
        catch
        {
            return null;
        }
    }
}
