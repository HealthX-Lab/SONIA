using UnityEngine;

/// <summary>
/// Custom class used to easily get exact position and size information from a mesh
/// </summary>
/// <organization>Health-X Lab</organization>
/// <project>Insideout (May-August 2022)</project>
/// <author>Owen Hellum</author>
public class BoundsInfo
{
    /// <summary>
    /// The Transform component of the associated GameObject
    /// </summary>
    public Transform Transform { get; }
    
    /// <summary>
    /// The absolute centre of the mesh bounds
    /// </summary>
    public Vector3 GlobalCentre { get; }
    
    /// <summary>
    /// The spherical size of the mesh
    /// </summary>
    public float Magnitude { get; }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="obj">Given GameObject with the mesh to be extracted</param>
    public BoundsInfo(GameObject obj)
    {
        Transform = obj.transform;
        
        MeshFilter[] childFilters = obj.GetComponentsInChildren<MeshFilter>();
        Vector3 tempCentre = Vector3.zero;
        float tempMagnitude = 0;

        foreach (MeshFilter i in childFilters)
        {
            Bounds tempBounds = i.mesh.bounds;
            
            tempCentre += i.gameObject.transform.TransformPoint(tempBounds.center);
            tempMagnitude += tempBounds.size.magnitude * i.gameObject.transform.lossyScale.magnitude;
        }
        
        Transform tempTransform = obj.transform;
            
        while (tempTransform.parent != null)
        {
            tempMagnitude *= tempTransform.lossyScale.magnitude;
                
            tempTransform = tempTransform.parent;
        }
        
        GlobalCentre = tempCentre / childFilters.Length; // Getting the local absolute centre
        Magnitude = tempMagnitude / childFilters.Length; // Getting the magnitude
    }
}
