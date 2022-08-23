using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MonoBehaviour to manage the updating of the structure information UI
/// </summary>
/// <organization>Health-X Lab</organization>
/// <project>Insideout (May-August 2022)</project>
/// <author>Owen Hellum</author>
public class StructureInformation : MonoBehaviour
{
    [SerializeField, Tooltip("Whether the information is attached to a moving object")]
    bool isMoving;
    [SerializeField, Tooltip("Whether or not to have the 'connected from' UI appear")]
    bool showConnectedFrom;
    
    Transform cam; // The camera's transform
    [HideInInspector] public GameObject canvas, connectionDescription; // The information canvas and the connection description
    MiniBrain miniBrain; // The mini brain script
    CompletionController completion; // The script to manage the completion amounts and displaying
    GameObject descriptionSection, connectionsSection; // The structure's description and connected structure parts in the UI
    bool hasSetNewUIPosition; // Whether or not the UI has been set after the completion stage has been changed
    // The Transforms of the verticalLayoutGroups holding the structures
    // that the selected structure is connected to and from, respectively
    Transform connectedToLayout, connectedFromLayout;
    GameObject[] lastConnectionsTo, lastConnectionsFrom; // The set of connected to/from structures in the UI
    GameObject lastSelected; // The last selected structure in the mini brain
    
    /// <summary>
    /// External, manually-called Start method
    /// </summary>
    public void InformationStart()
    {
        cam = Camera.main.transform;
        
        // Creating a new canvas
        canvas = Instantiate(Resources.Load<GameObject>("Structure Canvas"));
        canvas.SetActive(false);

        // Setting the constant position if it's not going to move
        if (!isMoving)
        {
            canvas.transform.SetParent(transform);
            canvas.transform.localPosition = Vector3.up * 0.2f;
            canvas.transform.localEulerAngles = Vector3.up * 180;
        }
        
        // Getting the main sections of the information UI
        descriptionSection = canvas.transform.GetChild(1).gameObject;
        connectionsSection = canvas.transform.GetChild(2).gameObject;
        
        // Getting the layout group sections
        connectedToLayout = connectionsSection.transform.GetChild(0)
            .GetComponentInChildren<VerticalLayoutGroup>().transform;
        connectedFromLayout = connectionsSection.transform.GetChild(1)
            .GetComponentInChildren<VerticalLayoutGroup>().transform;
        
        connectedFromLayout.parent.gameObject.SetActive(showConnectedFrom);
        
        // Hiding the connection description at the start
        connectionDescription = connectionsSection.transform.GetChild(2).gameObject;
        connectionDescription.SetActive(false);

        completion = FindObjectOfType<CompletionController>();

        // Hiding the connected structures UI at first if the completion is being done in stages
        if (completion.structureSelectionFirst)
        {
            connectionsSection.SetActive(false);
        }

        miniBrain = FindObjectOfType<MiniBrain>();
    }

    void FixedUpdate()
    {
        // Only looking at the user if it's following a moving object
        if (isMoving)
        {
            Transform canvasTransform = canvas.transform;

            // Positioning and rotating the canvas
            canvasTransform.position = transform.position + (transform.forward * 0.2f) + (Vector3.up * 0.2f);
            canvasTransform.LookAt(cam);
        
            // Looking at the user, with some minor extra tilting
            canvasTransform.localRotation = Quaternion.Euler(new Vector3(
                -10,
                canvasTransform.localRotation.eulerAngles.y + 20,
                canvasTransform.localRotation.eulerAngles.z
            ));   
        }
    }

    /// <summary>
    /// Method to quickly set the name, description, and populate the connection descriptions of the selected structure
    /// </summary>
    /// <param name="selected">The selected structure</param>
    /// <param name="description">The description of the selected structure</param>
    /// <param name="connectionsTo">The structures to which the selected structure is connected</param>
    /// <param name="connectionsFrom">The structures from which the selected structure is connected</param>
    /// <param name="loop">Whether or not to call the function a second time after calling it (true by default)</param>
    public void SetUI(
        GameObject selected,
        string description,
        GameObject[] connectionsTo,
        GameObject[] connectionsFrom,
        bool loop = true)
    {
        if ((completion.structureSelectionFirst && completion.hasFinishedStructureSelection) || !completion.structureSelectionFirst)
        {
            // Setting the position of the UI
            if (!hasSetNewUIPosition)
            {
                // Replacing the description with the connections if it's in phases
                if (completion.structureSelectionFirst && completion.hasFinishedStructureSelection)
                {
                    Invoke(nameof(WaitSetUIPosition), 7);
                    
                    hasSetNewUIPosition = true;
                }
                // Otherwise just moving everything over
                else if (!completion.structureSelectionFirst)
                {
                    descriptionSection.transform.localPosition += Vector3.right * 0.7f;
                    connectionsSection.transform.localPosition += Vector3.right * 0.7f;
                
                    hasSetNewUIPosition = true;
                }
            }

            SetConnections(connectionsTo, connectionsFrom, selected);
        }

        TMP_Text[] text = canvas.GetComponentsInChildren<TMP_Text>();

        canvas.SetActive(true);

        text[0].text = selected.name;

        if ((completion.structureSelectionFirst && !completion.hasFinishedStructureSelection) || !completion.structureSelectionFirst)
        {
            text[1].text = description;
        }

        // Adding colour-coded Subsystem pips for the selected structure
        if (miniBrain.info.Subsystems != null)
        {
            if (text[0].gameObject.transform.childCount > 1)
            {
                for (int k = 1; k < text[0].gameObject.transform.childCount; k++)
                {
                    Destroy(text[0].gameObject.transform.GetChild(k).gameObject);
                }  
            }
            
            Instantiate(Resources.Load<GameObject>("ColourPips"), text[0].gameObject.transform)
                .GetComponent<ColourPips>().AddPips(selected, Vector3.up * 0.1f);
        }

        // Updating the completion when a structure is selected
        completion.UpdateStructureCompletion(miniBrain.info.IndexOf(selected));
        completion.GenerateCompletionInfo();

        // Calling the method again if it's set to loop
        if (completion.hasFinishedStructureSelection && loop)
        {
            SetUI(selected, description, connectionsTo, connectionsFrom, false);
        }
    }

    /// <summary>
    /// Quick method to load the connections from the last loaded ones (to keep visual changes up to date)
    /// </summary>
    public void ResetConnections()
    {
        if (lastConnectionsTo != null && lastConnectionsFrom != null && lastSelected != null)
        {
            SetConnections(lastConnectionsTo, lastConnectionsFrom, lastSelected);   
        }
    }
    
    /// <summary>
    /// Loads all the connections with the given variables in the information UI
    /// </summary>
    /// <param name="to">The structures that the selected structure is connected to</param>
    /// <param name="from">The structures that the selected structure is connected from</param>
    /// <param name="selected">The selected structure</param>
    void SetConnections(GameObject[] to, GameObject[] from, GameObject selected)
    {
        // Removing the old 'connection to' options
        for (int i = 0; i < connectedToLayout.childCount; i++)
        {
            Destroy(connectedToLayout.GetChild(i).gameObject);
        }

        // Removing the old 'connection from' options
        for (int j = 0; j < connectedFromLayout.childCount; j++)
        {
            Destroy(connectedFromLayout.GetChild(j).gameObject);
        }

        // Creating both sets of connections
        AddConnections(to, connectedToLayout, selected, false);
        AddConnections(from, connectedFromLayout, selected, true);

        // Resetting the 'last' variables
        lastConnectionsTo = to;
        lastConnectionsFrom = from;
        lastSelected = selected;
    }

    /// <summary>
    /// Adds a set of connections into the information UI
    /// </summary>
    /// <param name="connections">The GameObjects tha the selected one is connected to/from</param>
    /// <param name="layout">The place to instantiate the connections in the UI</param>
    /// <param name="selected">The selected GameObject in the mini brain</param>
    /// <param name="isFrom">Whether or not the connections are coming from the other structures (rather than to)</param>
    void AddConnections(GameObject[] connections, Transform layout, GameObject selected, bool isFrom)
    {
        GameObject connection = Resources.Load<GameObject>("Connection");
        
        // Adding the new connection names
        foreach (GameObject i in connections)
        {
            // Instantiating the connection
            GameObject tempConnection = Instantiate(connection, layout);
            tempConnection.GetComponentInChildren<TMP_Text>().text = i.name;

            GameObject selectedObject = selected;
            GameObject otherObject = i;
            
            // Swapping the targets if it's connected 'from'
            if (isFrom)
            {
                (selectedObject, otherObject) = (otherObject, selectedObject);
            }

            // Getting the respective relative indices in the atlas
            int selectedIndex = miniBrain.info.IndexOf(selectedObject);
            int otherIndex = miniBrain.info.ValidConnections[selectedIndex].IndexOf(otherObject);
            
            MeshRenderer tempRenderer = tempConnection.GetComponentInChildren<MeshRenderer>();
            tempRenderer.gameObject.layer = LayerMask.NameToLayer("Selectable");
            
            // Checking if the connection has been viewed
            if (completion.structureCompletion[selectedIndex].ViewedConnections[otherIndex])
            {
                // Making the material transparent if it has been viewed
                StandardShaderUtils.ChangeRenderMode(tempRenderer.material, StandardShaderUtils.BlendMode.Transparent);

                Color tempColour = tempRenderer.material.color;
                tempRenderer.material.color = new Color(
                    tempColour.r,
                    tempColour.g,
                    tempColour.b,
                    0.5f
                );
            }

            // Adding colour-coded Subsystem pips for each connected structure
            if (miniBrain.info.Subsystems != null)
            {
                GameObject tempPips = Instantiate(
                    Resources.Load<GameObject>("ColourPips"),
                    tempConnection.transform
                );
                
                tempPips.GetComponent<ColourPips>().AddPips(selected, i, Vector3.right * -0.35f);
                tempPips.GetComponent<HorizontalLayoutGroup>().reverseArrangement = true;
            }
        }
    }

    /// <summary>
    /// Method to be called with Invoke to make the UI switch to connection selection after a certain waiting period
    /// </summary>
    void WaitSetUIPosition()
    {
        descriptionSection.SetActive(false);
        connectionsSection.SetActive(true);

        float setAmount = 1.35f;

        if (showConnectedFrom)
        {
            setAmount = 0.9f;
        }
        
        connectionsSection.transform.localPosition += Vector3.right * setAmount;
                    
        FindObjectOfType<StructureSelection>().ToggleTutorial();
    }

    /// <summary>
    /// Method to quickly set the text of the connection description
    /// </summary>
    /// <param name="selectedIndex">The index in the AtlasInfo of the currently selected object</param>
    /// <param name="otherIndex">The index in the AtlasInfo of the other object connected to the current object</param>
    public void SetConnectionDescription(int selectedIndex, int otherIndex)
    {
        connectionDescription.GetComponent<TMP_Text>().text = miniBrain.info.SubsystemConnectionDescriptions[
            selectedIndex,
            otherIndex
        ];
        
        // Updating the completion when a connection is selected
        completion.UpdateStructureCompletion(selectedIndex, otherIndex);
        completion.GenerateCompletionInfo();
    }
}