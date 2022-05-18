using UnityEngine;

/// <summary>
/// Class that uses QuickSort to get the nearest Transform
/// </summary>
public class FindClosest
{
    /// <summary>
    /// The sorted list of Transforms from closest to furthest away (null at first)
    /// </summary>
    public Transform[] To { get; private set; }

    /// <summary>
    /// The origin Transform
    /// </summary>
    Transform from;

    /// <summary>
    /// Default constructor (nothing happens)
    /// </summary>
    public FindClosest() { }

    /// <summary>
    /// Sets the From and To, then QuickSorts the To with respect to the From
    /// </summary>
    /// <param name="f">New from value</param>
    /// <param name="t">New to values</param>
    public void SetFromAndTo(Transform f, Transform[] t)
    {
        To = t;
        from = f;
        QuickSort(0, To.Length - 1);
    }

    /// <summary>
    /// QuickSort algorithm for organizing Transforms relative to their distance from an origin
    /// </summary>
    /// <param name="low">Starting index in the To array</param>
    /// <param name="high">Ending index in the To array</param>
    void QuickSort(int low, int high)
    {
        // Partitioning until unable to
        if (low < high)
        {
            int pi = Partition(low, high);

            // Recursively sorting the halves of the array
            QuickSort(low, pi - 1);
            QuickSort(pi + 1, high);
        }
    }
    
    /// <summary>
    /// QuickSort helper method to swap necessary values and return a new partition value
    /// </summary>
    /// <param name="low">Starting index in the To array</param>
    /// <param name="high">Ending index in the To array</param>
    /// <returns> The next index where the partition will happen</returns>
    int Partition(int low, int high)
    {
        float pivot = Vector3.Distance(from.position, To[high].position);

        int i = low - 1;

        // Making the i and j indices inward, swapping as necessary
        for (int j = low; j <= high- 1; j++)
        {
            if (Vector3.Distance(from.position, To[j].position) < pivot)
            {
                i++;
                (To[i], To[j]) = (To[j], To[i]);
            }
        }
        
        (To[i+1], To[high]) = (To[high], To[i+1]);
        return i + 1;
    }
}
