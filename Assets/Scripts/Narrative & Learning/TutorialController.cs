using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Valve.VR;

public class TutorialController : MonoBehaviour
{
    [Tooltip("Whether or not to run the tutorial when the experience starts")]
    public bool showTutorial = true;
    
    [Header("Tutorial file")]
    [Tooltip("The text file to read from")]
    [SerializeField] TextAsset tutorialText;
    [Tooltip("The speed at which to display the new lines")]
    [SerializeField] float speed = 1;
    [Tooltip("The line that the tutorial should start at (for debugging purposes)")]
    [SerializeField] int startAtLine;
    
    [Header("UI")]
    [Tooltip("The controller trigger boolean")]
    [SerializeField] SteamVR_Action_Boolean trigger;
    [Tooltip("The outline colour for the tutorial UI")]
    [SerializeField] Color outlineColour;
    [Tooltip("The UI button for the tutorial")]
    [SerializeField] GameObject tutorialButton;

    [HideInInspector] public int progress; // How many lines have been completed so far
    Transform canvas; // The subtitle canvas
    TMP_Text subtitle; // The subtitle text

    List<string> text; // List of each line in the tutorial
    List<bool> wait; // Corresponding list of booleans for whether to wait at each line
    int waitsCompleted; // How many line waits have already been completed
    
    ControllerLaser laser; // The right hand's laser script
    GameObject lastHitObject; // The last object hit with the laser script

    UIManager manager; // The main UI manager script

    void Start()
    {
        // Creating and initializing the subtitle variables
        canvas = Instantiate(Resources.Load<GameObject>("Subtitle Canvas"), transform).transform;
        subtitle = canvas.GetComponentInChildren<TMP_Text>();
        
        laser = FindObjectOfType<ControllerLaser>();
        manager = FindObjectOfType<UIManager>();

        text = new List<string>();
        wait = new List<bool>();
        
        string[] split = SplitAndTrim(tutorialText.text, '\n'); // Splitting the lines in the tutorial file

        foreach (string i in split)
        {
            // Making sure the line isn't empty or a comment
            if (i.Length > 0 && !i.Substring(0, 2).Equals("//"))
            {
                // Checking to see if the line might have a wait marker
                if (i.Length >= 8)
                {
                    string temp = i.Substring(0, 6);

                    // If the line has a wait marker, the boolean list is noted as such
                    if (temp.Equals("[WAIT]"))
                    {
                        text.Add(i.Substring(7));
                        wait.Add(true);
                    }
                    else
                    {
                        text.Add(i);
                        wait.Add(false);
                    }
                }
                else
                {
                    text.Add(i);
                    wait.Add(false);
                }
            }
        }

        // Skipping to the starting debug line
        
        progress = startAtLine;

        for (int j = 0; j < progress; j++)
        {
            if (wait[j])
            {
                waitsCompleted++;
            }
        }

        // Hiding itself if the tutorial isn't happening
        if (showTutorial)
        {
            GoToNext();
        }
        else
        {
            canvas.gameObject.SetActive(false);
        }
        
        tutorialButton.SetActive(false); // Hiding the tutorial button
        
        trigger.AddOnStateDownListener(OnTriggerDown, SteamVR_Input_Sources.RightHand); // Adding listeners for the controller input
    }

    void FixedUpdate()
    {
        // Making sure that the laser is hitting one of the options
        if (laser.hitObject != null && laser.hitObject.CompareTag("Tutorial Option"))
        {
            if (lastHitObject != laser.hitObject)
            {
                ResetHitting(); // Resetting the last options' outline
                
                Outline tempOutline = laser.hitObject.GetComponent<Outline>();
                
                if (tempOutline != null)
                {
                    // Enabling the outline if hitting the option
                    tempOutline.enabled = true;
                    lastHitObject = laser.hitObject;
                }
                else
                {
                    Outline newOutline = laser.hitObject.AddComponent<Outline>();
                    newOutline.OutlineWidth = 10;
                    newOutline.OutlineColor = outlineColour;
                }
            }
        }
        // Resetting the last option if not hitting anything
        else
        {
            ResetHitting();
        }
    }

    /// <summary>
    /// Method to continue down the tutorial lines, displaying only if valid, and halting otherwise
    /// </summary>
    public void GoToNext()
    {
        // Upping the waits completed if the wait is being by passed by this method's call
        if (wait[progress])
        {
            waitsCompleted++;
        }

        // Hiding the button after clicking it
        if (progress == 13)
        {
            tutorialButton.SetActive(true);
        }
        // Showing the pathway UI when the Introduction and Controls sections are done
        else if (progress == 15)
        {
            manager.GoToPathwayUI();
        }
        
        subtitle.text = text[progress]; // Setting the subtitle text for this line
        
        // TODO: trigger voiceover at each line
        
        // Gett the proportional time this line should display (in seconds)
        float seconds = text[progress].Length / (10f * speed);
        float min = 4f / speed;
        
        // Capping the minimum time
        if (seconds < min)
        {
            seconds = min;
        }

        // Making sure this isn't the last line
        if (progress < text.Count - 1)
        {
            // Only proceeding to the next line if it doesn't have a wait marker
            if (!wait[progress + 1])
            {
                Invoke(nameof(GoToNext), seconds);
            }

            progress++;
        }
        // Hiding the tutorial after the last line has been shown
        else
        {
            Invoke(nameof(HideTutorial), seconds);
        }
    }
    
    /// <summary>
    /// Splits a string into an array based on a given delimiter, then trims each value
    /// </summary>
    /// <param name="text">The string to be split</param>
    /// <param name="delim">The delimiter to partition the string</param>
    /// <returns>A newly split and individually trimmed array</returns>
    string[] SplitAndTrim(string text, char delim)
    {
        string[] temp = text.Split(delim);

        for (int i = 0; i < temp.Length;i++)
        {
            temp[i] = temp[i].Trim();
        }

        return temp;
    }

    /// <summary>
    /// Quick method to hide the tutorial subtitles after they are finished
    /// </summary>
    void HideTutorial()
    {
        canvas.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Quick method to reset the last hit object
    /// </summary>
    void ResetHitting()
    {
        if (lastHitObject != null)
        {
            lastHitObject.GetComponent<Outline>().enabled = false;
            lastHitObject = null;
        }
    }
    
    /// <summary>
    /// Triggers when given action starts on the given controller
    /// </summary>
    /// <param name="fromAction">Action to be anticipated</param>
    /// <param name="fromSource">Controller/hand that performs the action</param>
    void OnTriggerDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        // Making sure that the tutorial is happening and that it is currently waiting
        if (showTutorial && wait[progress])
        {
            // Clicking the trigger
            if (waitsCompleted == 0 && laser.hitObject == null)
            {
                GoToNext();
            }
            else if (laser.hitObject != null)
            {
                // Clicking a button
                if (waitsCompleted == 1 && laser.hitObject.CompareTag("Tutorial Option"))
                {
                    GoToNext();
                    
                    tutorialButton.SetActive(false);
                }
                // Selecting a connection
                else if (waitsCompleted == 2 && laser.hitObject.CompareTag("Pathway Option"))
                {
                    GoToNext();
                }
                // Cycling through the structures
                else if (waitsCompleted == 3 && laser.hitObject.CompareTag("Structure Option"))
                {
                    GoToNext();
                }
                // Returning to the connections menu
                else if (waitsCompleted == 4 && laser.hitObject.CompareTag("Return Option"))
                {
                    GoToNext();
                } 
            }
        }
    }
}