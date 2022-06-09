using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class BrainSkybox : MonoBehaviour
{
    [Header("Materials & colours")]
    [Tooltip("The Material into which the new Perlin noise Texture2D will be generated")]
    [SerializeField] Material skyboxMaterial;
    [Tooltip("The lighter and darker colours for the Perlin gradients")]
    [SerializeField] Color lightColour, darkColour;
    
    [Header("Seed")]
    [Tooltip("he seed value for this Perlin generation")]
    [SerializeField] [Range(0, 1000)] int seed;
    [Tooltip("Whether or not to generate the seed randomly")]
    [SerializeField] bool randomSeed = true;

    [Header("Noise parameters")]
    [Tooltip("The width (in pixels) to mirror and blur over along each edge of the Texture2D")]
    [SerializeField] int stitchWidth = 2;
    [Tooltip("The width of the Texture2D")]
    [SerializeField] int width = 500;
    [Tooltip("The height of the Texture2D")]
    [SerializeField] int height = 500;
    [Tooltip("The scale of the Texture2D (the larger the number, the more clusters)")]
    [SerializeField] float scale = 10;
    
    [Header("Advanced")]
    [Tooltip("A blurriness value (very technical)")]
    [SerializeField] int octaves = 10;
    [Tooltip("A blurriness value (very technical)")]
    [SerializeField] float persistance = 0.6f;
    [Tooltip("A blurriness value (very technical)")]
    [SerializeField] float lacunarity = 3;
    [Tooltip("The offset (in pixels) for the origin of the Perlin generation")]
    [SerializeField] Vector2 offset;

    Texture2D skyboxTexture; // The Texture2D to be applied to the Material
    Color[] pix; // The Color array of each pixel in the Texture2D
    
    void Start()
    {
        // Creating the new Texture2D and assigning it to the Material
        skyboxTexture = new Texture2D(width, height);
        skyboxMaterial.mainTexture = skyboxTexture;
        
        pix = new Color[width * height];

        CalculateNoise(); // Calculating the Perlin noise at start
    }
    
    /// <summary>
    /// Method to calculate the noise map, stitch the edges, and apply the colours
    /// </summary>
    void CalculateNoise()
    {
        float[] map = GenerateNoiseMap(); // Initial Perlin noise

        // Adding a horizontal stitch
        map = MakeSeamlessHorizontally(
            Map1DTo2D(
                map,
                width,
                height
            )
        );
        
        // Adding a vertical stitch
        map = MakeSeamlessVertically(
            Map1DTo2D(
                map,
                width,
                height
            )
        );

        // Setting colours based on the Perlin values
        for (int i = 0; i < map.Length; i++)
        {
            pix[i] = Color.Lerp(darkColour, lightColour, map[i]);
        }
        
        // Applying the colours to the Texture2D
        skyboxTexture.SetPixels(pix);
        skyboxTexture.Apply();
    }

    /// <summary>
    /// Method to convert a 1-dimensional array into a 2-dimensional array
    /// </summary>
    /// <param name="arr">The 1-dimensional array to be converted</param>
    /// <param name="arrWidth">The width on the 2-dimensional array (i.e. 2nd dimension)</param>
    /// <param name="arrHeight">The height on the 2-dimensional array (i.e. 1st dimension)</param>
    /// <returns>The newly-converted 2-dimensional array</returns>
    float[,] Map1DTo2D(float[] arr, int arrWidth, int arrHeight)
    {
        float[,] temp = new float[arrHeight, arrWidth]; // Creating a new empty 2D array

        int offset = 0; // The height (i.e. 1st dimension / column) offset
        
        for (int i = 0; i < arrWidth * arrHeight; i++)
        {
            // If the index has hit the end of a width (i.e. 2nd dimension / row), the height offset increases
            if (i != 0 && i % arrWidth == 0)
            {
                offset++;
            }
            
            temp[offset, i - (offset * arrWidth)] = arr[i]; // Copying the value into the correct index
        }

        return temp;
    }
    
    /// <summary>
    /// Method to convert a 2-dimensional array into a 1-dimensional array
    /// </summary>
    /// <param name="arr">The 1-dimensional array to be converted</param>
    /// <param name="arrWidth">The width on the 2-dimensional array (i.e. 2nd dimension)</param>
    /// <param name="arrHeight">The height on the 2-dimensional array (i.e. 1st dimension)</param>
    /// <returns>The newly-converted 2-dimensional array</returns>
    float[] Map2DTo1D(float[,] arr, int arrWidth, int arrHeight)
    {
        float[] temp = new float[arrHeight * arrWidth]; // Creating a new empty 1D array
        
        int offset = 0; // The height (i.e. 1st dimension / column) offset
        
        for (int i = 0; i < arrWidth * arrHeight; i++)
        {
            // If the index has hit the end of a width (i.e. 2nd dimension / row), the height offset increases
            if (i != 0 && i % arrWidth == 0)
            {
                offset++;
            }
            
            temp[i] = arr[offset, i - (offset * arrWidth)]; // Copying the value from the correct index
        }

        return temp;
    }
    
    /// <summary>
    /// Method to create a 1-dimensional array of Perlin noise values
    /// (copied from: https://code2d.wordpress.com/2020/07/21/perlin-noise)
    /// </summary>
    /// <returns>An 1D array of size (width * height) of 0-1 float values</returns>
    float[] GenerateNoiseMap()
    {
        float[] noiseMap = new float[width * height];

        if (randomSeed)
        {
            seed = Random.Range(0, 1000);
        }
        
        var random = new System.Random(seed);
     
        // We need at least one octave
        if (octaves < 1)
        {
            octaves = 1;
        }
     
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = random.Next(-100000, 100000) + offset.x;
            float offsetY = random.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }
     
        if (scale <= 0f)
        {
            scale = 0.0001f;
        }
     
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;
     
        // When changing noise scale, it zooms from top-right corner
        // This will make it zoom from the center
        float halfWidth = width / 2f;
        float halfHeight = height / 2f;
     
        for (int x = 0, y; x < width; x++)
        {
            for (y = 0; y < height; y++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;
     
                    // Use unity's implementation of perlin noise
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
     
                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }
     
                if (noiseHeight > maxNoiseHeight)
                    maxNoiseHeight = noiseHeight;
                else if (noiseHeight < minNoiseHeight)
                    minNoiseHeight = noiseHeight;
     
                noiseMap[y * width + x] = noiseHeight;
            }
        }
     
        for (int x = 0, y; x < width; x++)
        {
            for (y = 0; y < height; y++)
            {
                // Returns a value between 0f and 1f based on noiseMap value
                // minNoiseHeight being 0f, and maxNoiseHeight being 1f
                noiseMap[y * width + x] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[y * width + x]);
            }
        }
        return noiseMap;
    }
    
    /// <summary>
    /// Mirrors and blends one horizontal edge onto another, so the texture joins up horizontally
    /// (copied from: https://medium.com/nerd-for-tech/making-a-seamless-perlin-noise-in-c-4cfc12a90f93)
    /// </summary>
    /// <param name="noiseMap">The 2D Perlin noise array to be modified</param>
    /// <returns>The given noise array with mirrored and blended matching horizontal edges</returns>
    float[] MakeSeamlessHorizontally(float[,] noiseMap)
    {
        int width = noiseMap.GetUpperBound(0) + 1;
        int height = noiseMap.GetUpperBound(1) + 1;

        // iterate on the stitch band (on the left
        // of the noise)
        for (int x = 0; x < stitchWidth; x++)
        {
            // get the transparency value from
            // a linear gradient
            float v = x / (float)stitchWidth;
            for (int y = 0; y < height; y++)
            {
                // compute the "mirrored x position":
                // the far left is copied on the right
                // and the far right on the left
                int o = width - stitchWidth + x;
                // copy the value on the right of the noise
                noiseMap[o, y] = Mathf.Lerp(noiseMap[o, y], noiseMap[stitchWidth - x, y], v);
            }
        }

        return Map2DTo1D(noiseMap, width, height);
    }
    
    /// <summary>
    /// Mirrors and blends one vertical edge onto another, so the texture joins up vertically
    /// (copied from: https://medium.com/nerd-for-tech/making-a-seamless-perlin-noise-in-c-4cfc12a90f93)
    /// </summary>
    /// <param name="noiseMap">The 2D Perlin noise array to be modified</param>
    /// <returns>The given noise array with mirrored and blended matching vertical edges</returns>
    float[] MakeSeamlessVertically(float[,] noiseMap)
    {
        int width = noiseMap.GetUpperBound(0) + 1;
        int height = noiseMap.GetUpperBound(1) + 1;
   
        // iterate through the stitch band (both
        // top and bottom sides are treated
        // simultaneously because its mirrored)
        for (int y = 0; y < stitchWidth; y++)
        {
            // number of neighbour pixels to
            // consider for the average (= kernel size)
            int k = stitchWidth - y;
            // go through the entire row
            for (int x = 0; x < width; x++)
            {
                // compute the sum of pixel values
                // in the top and the bottom bands
                float s1 = 0.0f, s2 = 0.0f;
                int c = 0;
                for (int o = x - k; o < x + k; o++)
                {
                    if (o < 0 || o >= width)
                        continue;
                    s1 += noiseMap[o, y];
                    s2 += noiseMap[o, height - y - 1];
                    c++;
                }
                // compute the means and assign them to
                // the pixels in the top and the bottom
                // rows
                noiseMap[x, y] = s1 / (float)c;
                noiseMap[x, height - y - 1] = s2 / (float)c;
            }
        }

        return Map2DTo1D(noiseMap, width, height);
    }
}