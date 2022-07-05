using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColourBars : MonoBehaviour
{
    [SerializeField, Tooltip("The colours of the bars to be instantiated")]
    Color[] colours;
    [SerializeField, Tooltip("The percentages of each colour in the bar")]
    float[] completionPercentages;
    [SerializeField, Tooltip("Whether or not to automatically generate the bars when the scene starts")]
    bool generateOnStart;

    // Whether or not to completely fill each colour's section if the percentage is > 0
    [HideInInspector] public bool maxFill;

    void Start()
    {
        if (generateOnStart)
        {
            Generate();
        }
    }

    /// <summary>
    /// Creates new bar children objects based on the colours and percentages
    /// </summary>
    void Generate()
    {
        // Creating a new bar parent
        Transform layout = Instantiate(Resources.Load<GameObject>("Completion Bar"), transform)
            .GetComponentInChildren<HorizontalLayoutGroup>().transform;
        GameObject section = Resources.Load<GameObject>("Completion Bar Section");
        
        // Creating new colour sections
        for (int i = 0; i < colours.Length; i++)
        {
            GameObject temp = Instantiate(section, layout);
            temp.GetComponent<Image>().color = colours[i];

            float tempCompletion = completionPercentages[i];

            // Maxing out the percentage if is greater than 0 and set to fill this way
            if (maxFill && tempCompletion > 0)
            {
                tempCompletion = 1;
            }
            
            // Setting the width of teh new section
            RectTransform rect = temp.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(tempCompletion * (10f / colours.Length), rect.rect.height);
        }
    }

    /// <summary>
    /// Adds bars for all the Subsystems that the structure is a part of
    /// </summary>
    /// <param name="selected">The structure to be checked for membership to Subsystems</param>
    /// <param name="offset">The local offset of the bar (should it be needed)</param>
    public void AddBars(GameObject selected, Vector3 offset) { AddBars(selected, selected, offset); }
    
    /// <summary>
    /// Adds bars for all the Subsystems that both structures are a part of
    /// </summary>
    /// <param name="selected">The structure to be checked for membership to Subsystems</param>
    /// <param name="target">A secondary structure to be checked for membership</param>
    /// <param name="offset">The local offset of the bar (should it be needed)</param>
    public void AddBars(GameObject selected, GameObject target, Vector3 offset)
    {
        // Creating new temporary colour and percentage containers
        List<Color> tempColours = new List<Color>();
        List<float> tempPercentages = new List<float>();
        
        MiniBrain miniBrain = FindObjectOfType<MiniBrain>();
        CompletionController completion = FindObjectOfType<CompletionController>();
        
        SubsystemInfo[] temp = miniBrain.info.FindSharedSubsystems(selected, target); // Getting the shared Subsystems
        
        // Adding colours and percentages for each shared Subsystem
        for (int i = 0; i < temp.Length; i++)
        {
            tempColours.Add(temp[i].Colour);

            CompletionController.StructureCompletion tempCompletion =
                completion.structureCompletion[miniBrain.info.IndexOf(selected)];

            // Adding the percentage for each connection
            if (tempCompletion.SubsystemCompletion.Count > 0)
            {
                tempPercentages.Add(tempCompletion.SubsystemCompletion[i]);   
            }
            // If there are no connections, it gets the value from the total completion instead
            else
            {
                tempPercentages.Add(tempCompletion.Completion());
            }
        }
        
        SetValues(tempColours.ToArray(), tempPercentages.ToArray(), offset, true);
    }
    
    /// <summary>
    /// Method to set all the required variables for generation, then generate teh colour bars
    /// </summary>
    /// <param name="col">The colours of the bars to be instantiated</param>
    /// <param name="comp">The percentages of each colour in the bar</param>
    /// <param name="offset">The local offset of the bar (should it be needed)</param>
    /// <param name="maxFill">Whether or not to completely fill each colour's section if the percentage is > 0</param>
    public void SetValues(Color[] col, float[] comp, Vector3 offset, bool maxFill)
    {
        colours = col;
        completionPercentages = comp;
        
        transform.localPosition += offset;

        this.maxFill = maxFill;
        
        Generate();
    }
}