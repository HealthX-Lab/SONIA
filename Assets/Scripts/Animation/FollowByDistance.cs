using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowByDistance : MonoBehaviour
{
    [Tooltip("The Transform of the hand's miniature offset")]
    [SerializeField] Transform handTarget;
    [Tooltip("The maximum distance that the miniature brain will start following the hand when approaching the default target")]
    [SerializeField] float distance = 1.5f;

    Transform defaultTarget, leftHand; // The default starting target of the mini brain and the left hand's Trandform, respectively
    LazyFollow follow; // The script to make the mini brain follow a target

    void Start()
    {
        follow = GetComponent<LazyFollow>();
        defaultTarget = follow.target;
        leftHand = handTarget.parent;
    }

    void FixedUpdate()
    {
        // If the hand's target is close enough to the default target, the brain starts following it
        if (leftHand.position.x < 0 && Vector3.Distance(handTarget.position, defaultTarget.position) <= distance)
        {
            follow.target = handTarget;
        }
        // Otherwise, it floats back to the default position
        else
        {
            follow.target = defaultTarget;
        }
    }
}
