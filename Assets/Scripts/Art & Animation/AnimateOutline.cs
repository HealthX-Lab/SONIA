using System;
using UnityEngine;

public class AnimateOutline : MonoBehaviour
{
    [Tooltip("The speed at which the outline pulses")]
    [SerializeField] float speed = 0.2f;
    [Tooltip("The maximum width that the outline can grow to")]
    [SerializeField] float maxWidth = 15;

    Outline outline; // The outline Component of the object
    bool increasing = true; // Whether the outline is growing or shrinking
    
    void Start()
    {
        outline = GetComponent<Outline>();
    }

    void FixedUpdate()
    {
        if (increasing)
        {
            outline.OutlineWidth += speed; // Growing the outline

            // Switching the direction if too large
            if (outline.OutlineWidth >= maxWidth)
            {
                increasing = false;
            }
        }
        else
        {
            outline.OutlineWidth -= speed; // Shrinking the direction

            // Switching the direction if too small
            if (outline.OutlineWidth <= 5)
            {
                increasing = true;
            }
        }
    }

    /// <summary>
    /// Public method to quickly reset the outline's width
    /// </summary>
    public void Reset()
    {
        outline.OutlineWidth = 5;
    }
}