using System;
using UnityEngine;

public class BrainSkybox : MonoBehaviour
{
    [SerializeField] Material skyboxMaterial;
    [SerializeField] int scale;
    [SerializeField] Color lightColour, darkColour;

    Texture2D noiseTexture;
    Color[] pix;
    
    void Start()
    {
        // Set up the texture and a Color array to hold pixels during processing.
        noiseTexture = new Texture2D(scale * 10, scale * 10);
        noiseTexture.wrapMode = TextureWrapMode.Mirror;
        
        pix = new Color[noiseTexture.width * noiseTexture.height];
        skyboxMaterial.mainTexture = noiseTexture;
        
        CalculateNoise();
    }
    
    void Update()
    {
        /*
        if (skyboxMaterial != null && !RenderSettings.skybox.Equals(skyboxMaterial))
        {
            RenderSettings.skybox = skyboxMaterial;
        }
        */
    }

    void CalculateNoise()
    {
        float y = 0;

        while (y < noiseTexture.height)
        {
            float x = 0;
            
            while (x < noiseTexture.width)
            {
                float xCoord = x / noiseTexture.width * scale;
                float yCoord = y / noiseTexture.height * scale;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                
                pix[(int)y * noiseTexture.width + (int)x] = Color.Lerp(darkColour, lightColour, sample);
                
                x++;
            }
            
            y++;
        }
        
        noiseTexture.SetPixels(pix);
        noiseTexture.Apply();
    }
}