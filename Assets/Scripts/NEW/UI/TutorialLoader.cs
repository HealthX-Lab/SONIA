using System;
using System.Collections.Generic;
using TMPro;
using Unity.Tutorials.Core.Editor;
using UnityEngine;

/// <summary>
/// MonoBehaviour to generate the tutorial text and present it in the scene as needed
/// </summary>
/// <organization>Health-X Lab</organization>
/// <project>Insideout (May-August 2022)</project>
/// <author>Owen Hellum</author>
public class TutorialLoader : MonoBehaviour
{
    [SerializeField, Tooltip(
         "The base folder name within the Resources folder" +
         " and the corresponding local file name for the tutorial text"
    )]
    string path, tutorialPath;

    GameObject button; // The button GameObject to be hidden and shown each rest
    string[] tutorialSections; // The array of tutorial texts to be shown sequentially
    [HideInInspector] public int current; // The index of the currently/last shown tutorial section

    void Start()
    {
        GenerateSections();

        current = -1;
        Reset();
    }

    /// <summary>
    /// Method to go to the next tutorial section, show its text, and show the button
    /// </summary>
    public void Reset()
    {
        current++;
        GetComponentInChildren<TMP_Text>().text = tutorialSections[current];

        button = GetComponentInChildren<Collider>().transform.parent.gameObject;
        button.SetActive(false);
        Invoke(nameof(ShowButton), 3);
    }

    /// <summary>
    /// Method to loop through the tutorial text file, and extract/format the text within
    /// </summary>
    void GenerateSections()
    {
        // Splitting the file's lines
        string[] split = Resources.Load<TextAsset>(path + "/" + tutorialPath).text.Split('\n');
        
        List<string> temp = new List<string>();
        
        bool isHeader = true; // Whether the next line to be gotten is a header line

        foreach (string i in split)
        {
            // Making sure that the current line isn't empty
            if (i.Trim().IsNotNullOrEmpty())
            {
                // Formatting the header and adding it to the list
                if (isHeader)
                {
                    temp.Add("<style=\"H1\">" + i + "</style>");
                    isHeader = false;
                }
                // Otherwise, appending the regular tutorial text to the previous header
                else
                {
                    temp[^1] += "\n" + i;
                    isHeader = true;
                }
            }
        }

        tutorialSections = temp.ToArray();
    }

    /// <summary>
    /// Quick method to set the button gameObject active (called with Invoke in this case)
    /// </summary>
    void ShowButton()
    {
        button.SetActive(true);
    }
}