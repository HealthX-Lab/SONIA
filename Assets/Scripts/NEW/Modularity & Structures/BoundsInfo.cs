using UnityEngine;
using UnityEngine.UIElements;

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
    
    public Vector3 Top { get; }
    public Vector3 Bottom { get; }
    public Vector3 Front { get; }
    public Vector3 Back { get; }
    public Vector3 Right { get; }
    public Vector3 Left { get; }
    
    /// <summary>
    /// The spherical size of the mesh
    /// </summary>
    public float Magnitude { get; }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="obj">Given GameObject with the mesh to be extracted</param>
    public BoundsInfo(GameObject obj, bool generateExtremities = false)
    {
        Transform = obj.transform;
        
        MeshFilter[] childFilters = obj.GetComponentsInChildren<MeshFilter>();
        Vector3 tempCentre = Vector3.zero;
        float tempMagnitude = 0;

        Vector3 currentTop = Vector3.zero;
        Vector3 currentBottom = Vector3.zero;
        Vector3 currentFront = Vector3.zero;
        Vector3 currentBack = Vector3.zero;
        Vector3 currentRight = Vector3.zero;
        Vector3 currentLeft = Vector3.zero;

        foreach (MeshFilter i in childFilters)
        {
            Bounds tempBounds = i.mesh.bounds;
            Transform meshTransform = i.gameObject.transform;
            
            tempCentre += meshTransform.TransformPoint(tempBounds.center);
            tempMagnitude += tempBounds.size.magnitude * meshTransform.lossyScale.magnitude;

            if (generateExtremities)
            {
                Vector3 tempScale = meshTransform.localScale;
                float xSize = (tempBounds.size.x / 2f) * tempScale.x;
                float ySize = (tempBounds.size.y / 2f) * tempScale.y;
                float zSize = (tempBounds.size.z / 2f) * tempScale.z;
            
                if (currentTop.y < ySize)
                {
                    currentTop = meshTransform.TransformPoint( meshTransform.up * ySize);
                }
            
                if (currentBottom.y > -ySize)
                {
                    currentBottom = meshTransform.TransformPoint( -meshTransform.up * ySize);
                }
            
                if (currentFront.z < zSize)
                {
                    currentFront = meshTransform.TransformPoint( meshTransform.forward * zSize);
                }
            
                if (currentBack.z > -zSize)
                {
                    currentBack = meshTransform.TransformPoint( -meshTransform.forward * zSize);
                }
            
                if (currentRight.x < xSize)
                {
                    currentRight = meshTransform.TransformPoint( meshTransform.right * xSize);
                }
            
                if (currentLeft.x > -xSize)
                {
                    currentLeft = meshTransform.TransformPoint( -meshTransform.right * xSize);
                }
            }
        }
        
        Transform tempTransform = obj.transform;
            
        while (tempTransform.parent != null)
        {
            tempMagnitude *= tempTransform.lossyScale.magnitude;
                
            tempTransform = tempTransform.parent;
        }
        
        GlobalCentre = tempCentre / childFilters.Length; // Getting the local absolute centre
        Magnitude = tempMagnitude / childFilters.Length; // Getting the magnitude

        if (generateExtremities)
        {
            Top = currentTop;
            Bottom = currentBottom;
            Front = currentFront;
            Back = currentBack;
            Right = currentRight;
            Left = currentLeft;
        }
    }
}
