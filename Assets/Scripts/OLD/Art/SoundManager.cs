using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    List<AudioSource> sources; // A list of AudioSources created by this script

    void Start()
    {
        sources = new List<AudioSource>();
    }

    /// <summary>
    /// Public method to add a new AudioSource child
    /// </summary>
    /// <param name="clip">The AudioClip to be played when the AudioSource plays</param>
    /// <param name="volume">The volume of the AudioSource</param>
    /// <param name="loop">Whether or not to loop the AudioSource's clip</param>
    /// <returns>The newly instantiated AudioSource</returns>
    public AudioSource AddSource(AudioClip clip, float volume, bool loop)
    {
        // Creating a child object
        GameObject temp = new GameObject("Source");
        temp.transform.SetParent(transform);

        // Adding the AudioSource and setting its values
        AudioSource source = temp.AddComponent<AudioSource>();
        source.volume = volume;
        source.clip = clip;
        source.loop = loop;

        return source;
    }
}