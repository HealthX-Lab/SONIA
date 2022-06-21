using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class SwimmingController : MonoBehaviour
{
    [Header("Guide")]
    [Tooltip("Whether or not to show the guide arrow")]
    [SerializeField] bool showGuide;
    [Tooltip("What material should be applied to the guide arrow")]
    [SerializeField] Material guideMaterial;
    
    [Header("Parameters")]
    [Tooltip("The speed factor of the swimming")]
    [SerializeField] float speed = 0.2f;
    [Tooltip("Minimum safe distance from the triggering hand positions after they have both been triggered before the swimming starts")]
    [SerializeField] float safeDistance = 0.25f;
    [Tooltip("The factor by which the speed should slow down each 1/10th of a second (i.e. higher = less slowdown)")]
    [SerializeField] float slowdownFactor = 10;
    
    [Header("Objects & Triggers")]
    [Tooltip("The SteamVR action to trigger the swimming on both hands")]
    [SerializeField] SteamVR_Action_Boolean trigger;
    [Tooltip("The SteamVR CameraRig and right/left hand Transform")]
    [SerializeField] Transform rig, rightHand, leftHand;

    GameObject guide; // The guide arrow GameObject
    
    bool rightDown, leftDown; // Whether the right or left triggering buttons are being held down
    
    Vector3 forwardAverage; // The normalized average vector between the left and right hands' forward-facing vectors
    Vector3 moveDirection; // The direction the rig is supposed to move in
    Vector3 midPoint; // The (local) midpoint between the right and left hands
    Vector3 rightStartPosition, leftStartPosition; // The (local) position of the right and left hands at the moment that both controllers' trigger buttons are held down
    float midPointStartDistance; // The (global) summed distances from the left and right hands to the midpoint at the moment that both controllers' trigger buttons are held down
    
    bool isSlowingDown; // Whether the rig is in the process of slowing down after a single slide has started
    float startSpeed; // The initial starting speed of the swimming

    void Start()
    {
        if (showGuide)
        {
            // Creating the new guide object
            guide = new GameObject("Guide");
            guide.transform.SetParent(rig);
        
            // Giving it its directional LineRenderer
            LineRenderer guideLine = guide.AddComponent<LineRenderer>();
            guideLine.material = guideMaterial;
            guideLine.useWorldSpace = false;
            guideLine.widthMultiplier = 0.02f;
            guideLine.SetPositions(new []{ Vector3.zero, Vector3.forward * 0.5f });

            // Creating a cosmetic arrow tip object
            GameObject arrow = new GameObject("Arrow");
            arrow.transform.SetParent(guide.transform);

            // Adding the arrow tip LineRenderer
            LineRenderer arrowLine = arrow.AddComponent<LineRenderer>();
            arrowLine.material = guideMaterial;
            arrowLine.useWorldSpace = false;
            arrowLine.widthMultiplier = 0.02f;
            arrowLine.positionCount = 3;
            arrowLine.SetPositions(new []{ new Vector3(-0.1f, 0, 0.4f), Vector3.forward * 0.5f, new Vector3(0.1f, 0, 0.4f) });
        }
        
        // Adding listeners for the controller input
        trigger.AddOnStateDownListener(OnRightDown, SteamVR_Input_Sources.RightHand);
        trigger.AddOnStateUpListener(OnRightUp, SteamVR_Input_Sources.RightHand);
        trigger.AddOnStateDownListener(OnLeftDown, SteamVR_Input_Sources.LeftHand);
        trigger.AddOnStateUpListener(OnLeftUp, SteamVR_Input_Sources.LeftHand);

        startSpeed = speed;
    }

    void FixedUpdate()
    {
        // Getting the forward average and midpoint between the hands
        forwardAverage = ((rightHand.forward + leftHand.forward) / 2f).normalized;
        Vector3 vectorBetween = leftHand.localPosition - rightHand.localPosition;
        midPoint = rightHand.localPosition + (vectorBetween / 2f);
        
        if (showGuide)
        {
            Vector3 tempDirection;

            // If not moving, point it towards the forward average
            if (rightDown && leftDown)
            {
                tempDirection = moveDirection;
            }
            // If currently moving, keep the guide locked in that direction
            else
            {
                tempDirection = forwardAverage;
            }
            
            guide.transform.localPosition = midPoint;
            guide.transform.LookAt(rig.TransformPoint(midPoint + tempDirection));
        }
        
        // If both controllers' triggering buttons are being held down
        if (rightDown && leftDown)
        {
            float rightDistance = Vector3.Distance(rightHand.localPosition, rightStartPosition);
            float leftDistance = Vector3.Distance(leftHand.localPosition, leftStartPosition);
            float midPointDistance = rightDistance + leftDistance; // Calculating the current summed distance from the midpoint

            float midPointDifference = midPointDistance - midPointStartDistance; // Finding the difference between the current distance and the starting distance

            // Making sure to only start moving when the difference is out of the safe distance
            if (midPointDifference > 0 || (midPointDifference < 0 && (-midPointDifference > safeDistance)))
            {
                // Starting to slow down
                if (!isSlowingDown)
                {
                    isSlowingDown = true;
                    Invoke(nameof(SlowSpeed), 0.1f);
                }
            
                rig.transform.position += moveDirection * (speed * midPointDifference); // Moving the rig in the desired direction
            }
        }
    }

    /// <summary>
    /// Recursive Invoke method that incrementally slows down the swimming speed until the swimming stops
    /// </summary>
    void SlowSpeed()
    {
        if (isSlowingDown && speed > 0.01f)
        {
            speed -= startSpeed / slowdownFactor; // Slowing it down by the slowdownFactor of the initial speed
            Invoke(nameof(SlowSpeed), 0.1f);
        }
    }

    /// <summary>
    /// Method to initialize the required variables when the swimming first starts
    /// </summary>
    void StartSwimming()
    {
        // Getting the hand positions at the starting moment
        rightStartPosition = rightHand.localPosition;
        leftStartPosition = leftHand.localPosition;

        midPointStartDistance = Vector3.Distance(rightStartPosition, midPoint) + Vector3.Distance(leftStartPosition, midPoint); // Getting the summed midpoint distance at the starting moment

        moveDirection = forwardAverage; // Locking the move direction to the last gotten forward averge
    }

    /// <summary>
    /// Method to reset required variables when the swimming stops
    /// </summary>
    void StopSwimming()
    {
        // Stops slowing down and resets the current speed
        isSlowingDown = false;
        speed = startSpeed;
    }

    /// <summary>
    /// Triggers when triggering action starts on the right controller
    /// </summary>
    /// <param name="fromAction">Action to be anticipated</param>
    /// <param name="fromSource">Controller/hand that performs the action</param>
    void OnRightDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        rightDown = true;

        if (leftDown)
        {
            StartSwimming();
        }
    }
    
    /// <summary>
    /// Triggers when triggering action stops on the right controller
    /// </summary>
    /// <param name="fromAction">Action to be anticipated</param>
    /// <param name="fromSource">Controller/hand that performs the action</param>
    void OnRightUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        rightDown = false;
        
        StopSwimming();
    }
    
    /// <summary>
    /// Triggers when triggering action starts on the left controller
    /// </summary>
    /// <param name="fromAction">Action to be anticipated</param>
    /// <param name="fromSource">Controller/hand that performs the action</param>
    void OnLeftDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        leftDown = true;

        if (rightDown)
        {
            StartSwimming();
        }
    }
    
    /// <summary>
    /// Triggers when triggering action stops on the left controller
    /// </summary>
    /// <param name="fromAction">Action to be anticipated</param>
    /// <param name="fromSource">Controller/hand that performs the action</param>
    void OnLeftUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        leftDown = false;
        
        StopSwimming();
    }
}
