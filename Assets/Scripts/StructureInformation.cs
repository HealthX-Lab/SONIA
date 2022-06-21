using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StructureInformation : MonoBehaviour
{
    Transform cam; // The camera's transform
    [HideInInspector] public GameObject canvas, connectionDescription; // The information canvas and the connection description
    MiniBrain miniBrain;
    CompletionController completion; // The script to manage the completion amounts and displaying
    
    void Start()
    {
        cam = Camera.main.transform;
        
        // Creating a new canvas
        canvas = Instantiate(Resources.Load<GameObject>("Structure Canvas"));
        canvas.SetActive(false);

        // Hiding the connection description at the start
        connectionDescription = canvas.GetComponentsInChildren<TMP_Text>()[3].gameObject;
        connectionDescription.SetActive(false);

        miniBrain = FindObjectOfType<MiniBrain>();
        completion = FindObjectOfType<CompletionController>();
    }

    void FixedUpdate()
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

    /// <summary>
    /// Method to quickly set the name, description, and populate the connection descriptions of the selected structure
    /// </summary>
    /// <param name="name">The name of the selected structure</param>
    /// <param name="description">The description of the selected structure</param>
    /// <param name="connections">The structures to which the selected structure is connected</param>
    public void SetUI(string name, string description, GameObject[] connections)
    {
        Transform layout = canvas.GetComponentInChildren<VerticalLayoutGroup>().transform;
        
        // Removing the old connection descriptions
        for (int i = 0; i < layout.childCount; i++)
        {
            Destroy(layout.GetChild(i).gameObject);
        }
        
        canvas.SetActive(true);
        
        TMP_Text[] text = canvas.GetComponentsInChildren<TMP_Text>();

        text[0].text = name;
        text[1].text = description;

        GameObject connection = Resources.Load<GameObject>("Connection");

        // Adding the new connection descriptions
        foreach (GameObject j in connections)
        {
            Instantiate(connection, layout).GetComponentInChildren<TMP_Text>().text = j.name;
        }
        
        // Updating the completion when a structure is selected
        completion.UpdateStructureCompletion(miniBrain.info.IndexOf(miniBrain.info.Find(name)));
        completion.GenerateCompletionInfo();
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
