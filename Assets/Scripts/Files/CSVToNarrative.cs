using System;
using UnityEngine;

public class CSVToNarrative : MonoBehaviour
{
    [SerializeField] TextAsset file;

    void Start()
    {
        print(Load().Name);
    }

    Narrative Load()
    {
        string[] split = file.text.Split('\n');
        
        Narrative temp = new Narrative(
            split[0].Trim(),
            split[1].Trim()
        );

        return temp;
    }
}