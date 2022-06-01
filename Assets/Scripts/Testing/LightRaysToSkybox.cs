using System;
using UnityEngine;

public class LightRaysToSkybox : MonoBehaviour
{
    [SerializeField] MeshRenderer source;

    void Start()
    {
        Skybox temp = gameObject.AddComponent<Skybox>();
        temp.material = source.material;
    }
}