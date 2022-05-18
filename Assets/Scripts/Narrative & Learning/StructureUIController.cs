using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StructureUIController : MonoBehaviour
{
    [SerializeField] string name;
    [SerializeField] string description;

    GameObject canvas;
    TMP_Text nameText, descriptionText;

    void Start()
    {
        if (!name.Equals("") || !description.Equals(""))
        {
            SetUI(name, description);
        }
    }

    void CheckCanvas()
    {
        if (canvas == null)
        {
            canvas = Instantiate(Resources.Load<GameObject>("Structure Canvas"), transform);

            TMP_Text[] temp = canvas.GetComponentsInChildren<TMP_Text>();
            nameText = temp[0];
            descriptionText = temp[1];
        }
    }

    public void SetUI(string n, string d)
    {
        CheckCanvas();
        
        nameText.text = n;
        descriptionText.text = d;
    }
}
