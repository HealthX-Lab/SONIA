using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StructureInformation : MonoBehaviour
{
    [SerializeField, Tooltip("Whether the information is attached to a moving object")]
    bool isMoving;
    
    Transform cam; // The camera's transform
    [HideInInspector] public GameObject canvas, connectionDescription; // The information canvas and the connection description
    MiniBrain miniBrain; // The mini brain script
    CompletionController completion; // The script to manage the completion amounts and displaying
    GameObject descriptionSection, connectionsSection; // The structure's description and connected structure parts in the UI
    bool hasSetNewUIPosition; // Whether or not the UI has been set after the completion stage has been changed
    
    void Start()
    {
        cam = Camera.main.transform;
        
        // Creating a new canvas
        canvas = Instantiate(Resources.Load<GameObject>("Structure Canvas"));
        canvas.SetActive(false);

        // Setting the constant position if it's not going to move
        if (!isMoving)
        {
            canvas.transform.SetParent(transform);
            canvas.transform.localPosition = Vector3.zero;
            canvas.transform.localEulerAngles = Vector3.up * 180;
        }

        TMP_Text[] texts = canvas.GetComponentsInChildren<TMP_Text>();

        // Hiding the connection description at the start
        connectionDescription = texts[3].gameObject;
        connectionDescription.SetActive(false);

        descriptionSection = texts[1].gameObject;
        connectionsSection = texts[2].transform.parent.gameObject;
        
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
    /// <param name="connections">The structures to which the selected structure is connected</param>
    /// <param name="loop">Whether or not to call the function a second time after calling it (true by default)</param>
    public void SetUI(GameObject selected, string description, GameObject[] connections, bool loop = true)
    {
        if ((completion.structureSelectionFirst && completion.hasFinishedStructureSelection) || !completion.structureSelectionFirst)
        {
            // Setting the position of the UI
            if (!hasSetNewUIPosition)
            {
                // Replacing the description with the connections if it's in phases
                if (completion.structureSelectionFirst && completion.hasFinishedStructureSelection)
                {
                    descriptionSection.SetActive(false);
                    connectionsSection.SetActive(true);
                
                    connectionsSection.transform.localPosition += Vector3.right * 1.35f;
                
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

            Transform layout = canvas.GetComponentInChildren<VerticalLayoutGroup>().transform;
        
            // Removing the old connection descriptions
            for (int i = 0; i < layout.childCount; i++)
            {
                Destroy(layout.GetChild(i).gameObject);
            }
            
            GameObject connection = Resources.Load<GameObject>("Connection");
            
            // Adding the new connection descriptions
            foreach (GameObject j in connections)
            {
                GameObject tempConnection = Instantiate(connection, layout);
                tempConnection.GetComponentInChildren<TMP_Text>().text = j.name;

                // Adding colour-coded Subsystem pips for each connected structure
                if (miniBrain.info.Subsystems != null)
                {
                    GameObject tempPips = Instantiate(
                        Resources.Load<GameObject>("ColourPips"),
                        tempConnection.transform
                    );
                
                    tempPips.GetComponent<ColourPips>().AddPips(selected, j, Vector3.right * -0.35f);
                    tempPips.GetComponent<HorizontalLayoutGroup>().reverseArrangement = true;
                }
            }
        }

        canvas.SetActive(true);
        
        TMP_Text[] text = canvas.GetComponentsInChildren<TMP_Text>();

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
            SetUI(selected, description, connections, false);
        }
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