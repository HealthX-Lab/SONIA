using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniatureBrainController : MonoBehaviour
{
    [Tooltip("The miniature brain object to be created")]
    [SerializeField] GameObject brain;
    [Tooltip("The new scale of the mini brain")]
    [SerializeField] float scale = 1;

    [HideInInspector] public GameObject miniBrain; // The publicly-accessible mini brain

    void Start()
    {
        miniBrain = Instantiate(brain, transform);
        miniBrain.transform.localScale = Vector3.one * scale;
    }
}
