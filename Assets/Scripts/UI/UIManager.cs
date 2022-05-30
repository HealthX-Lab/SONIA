using System;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] PathwaySelectionManager pathwayManager;
    
    public void TogglePathwayManager()
    {
        pathwayManager.gameObject.SetActive(!pathwayManager.gameObject.activeSelf);
    }
}