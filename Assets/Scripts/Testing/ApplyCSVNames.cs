using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyCSVNames : MonoBehaviour
{
    [SerializeField] TextAsset csv;

    void Start()
    {
        string[] lines = csv.text.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            transform.GetChild(i).name = lines[i].Split(',')[1];
        }
    }
}
