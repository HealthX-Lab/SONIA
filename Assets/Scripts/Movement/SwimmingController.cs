using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class SwimmingController : MonoBehaviour
{
    [SerializeField] bool showGuide;
    [SerializeField] float speed = 0.1f;
    
    [SerializeField] Transform rig;
    [SerializeField] Transform rightHand, leftHand;
    
    [Tooltip("The controller trigger boolean")]
    [SerializeField] SteamVR_Action_Boolean trigger;
    [Tooltip("Which hand's input")]
    [SerializeField] SteamVR_Input_Sources rightInput, leftInput;

    Vector3 midPoint, moveDirection;
    Vector3 startPosition;
    
    GameObject guide;
    bool rightGrip, leftGrip;

    void Start()
    {
        if (showGuide)
        {
            guide = new GameObject("Guide");
        
            LineRenderer temp = guide.AddComponent<LineRenderer>();
            temp.useWorldSpace = false;
            temp.widthMultiplier = 0.02f;
            temp.SetPositions(new []{ Vector3.zero, Vector3.forward * 0.5f });
        }
        
        // Adding listeners for the controller input
        trigger.AddOnStateDownListener(OnRightGripDown, rightInput);
        trigger.AddOnStateUpListener(OnRightGripUp, rightInput);
        trigger.AddOnStateDownListener(OnLeftGripDown, leftInput);
        trigger.AddOnStateUpListener(OnLeftGripUp, leftInput);
    }

    void FixedUpdate()
    {
        Vector3 vectorBetween = leftHand.position - rightHand.position;
        midPoint = rightHand.position + (vectorBetween / 2f);

        if (showGuide)
        {
            guide.transform.position = midPoint;
        }

        Vector3 forwardAverage = ((rightHand.forward + leftHand.forward) / 2f).normalized;
        moveDirection = forwardAverage - midPoint;

        if (showGuide)
        {
            guide.transform.LookAt(midPoint + forwardAverage);
        }

        if (rightGrip && leftGrip)
        {
            print(moveDirection.magnitude);
            rig.transform.position += moveDirection * speed;
        }
    }
    
    /// <summary>
    /// Triggers when grip action starts on the right controller
    /// </summary>
    /// <param name="fromAction">Action to be anticipated</param>
    /// <param name="fromSource">Controller/hand that performs the action</param>
    void OnRightGripDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        rightGrip = true;

        if (leftGrip)
        {
            startPosition = rig.transform.position;
        }
    }
    
    /// <summary>
    /// Triggers when grip action stops on the right controller
    /// </summary>
    /// <param name="fromAction">Action to be anticipated</param>
    /// <param name="fromSource">Controller/hand that performs the action</param>
    void OnRightGripUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        rightGrip = false;
    }
    
    /// <summary>
    /// Triggers when grip action starts on the left controller
    /// </summary>
    /// <param name="fromAction">Action to be anticipated</param>
    /// <param name="fromSource">Controller/hand that performs the action</param>
    void OnLeftGripDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        leftGrip = true;

        if (rightGrip)
        {
            startPosition = rig.transform.position;
        }
    }
    
    /// <summary>
    /// Triggers when grip action stops on the left controller
    /// </summary>
    /// <param name="fromAction">Action to be anticipated</param>
    /// <param name="fromSource">Controller/hand that performs the action</param>
    void OnLeftGripUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        leftGrip = false;
    }
}
