using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundTrigger : MonoBehaviour
{
    [Tooltip("The array of possible sounds this trigger can make")]
    [SerializeField] AudioClip[] sounds;
    [Tooltip("The volume of this sound")]
    [SerializeField] float volume = 0.5f;
    [Tooltip("Whether or not to play this sound when the system starts")]
    [SerializeField] bool onStart;
    [Tooltip("Whether or not to loop the sound")]
    [SerializeField] bool loop;

    SoundManager manager; // The manager where the AudioSources are stored
    AudioSource source; // The AudioSource corresponding to this trigger in the SoundManager

    void Start()
    {
        manager = FindObjectOfType<SoundManager>();
        
        if (onStart)
        {
            PlaySound();
        }
    }
    
    /// <summary>
    /// Plays the corresponding sound
    /// (creates a new AudioSource child in the SoundManager if one doesn't exist for this trigger yet)
    /// </summary>
    public void PlaySound()
    {
        if (source == null)
        {
            source = manager.AddSource(GetClip(), volume, loop);
        }
        
        source.Play();
    }
    
    /// <summary>
    /// Stops the corresponding sound
    /// (creates a new AudioSource child in the SoundManager if one doesn't exist for this trigger yet)
    /// </summary>
    public void StopSound()
    {
        if (source == null)
        {
            source = manager.AddSource(GetClip(), volume, loop);
        }
        
        source.Stop();
    }

    /// <summary>
    /// Gets a random clip from the possible clips variable
    /// (selects the only available of if there is in fact only one)
    /// </summary>
    /// <returns></returns>
    AudioClip GetClip()
    {
        // Making sure there are clips
        if (sounds.Length > 0)
        {
            // Getting the only one
            if (sounds.Length == 1)
            {
                return sounds[0];
            }
            
            return sounds[Random.Range(0, sounds.Length)]; // Getting a random one
        }

        return null;
    }
}