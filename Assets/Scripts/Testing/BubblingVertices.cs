using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubblingVertices : MonoBehaviour
{
    [SerializeField] float amplitude = 1;
    
    MeshFilter filter;
    Vector3[] startPositions;

    void Start()
    {
        filter = GetComponent<MeshFilter>();
        startPositions = filter.mesh.vertices;
    }

    void FixedUpdate()
    {
        Vector3[] verts = filter.mesh.vertices;
        
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = startPositions[i] + filter.mesh.normals[i] * Random.Range(-0.01f, 0.01f) * amplitude;
        }

        filter.mesh.vertices = verts;
    }
}
