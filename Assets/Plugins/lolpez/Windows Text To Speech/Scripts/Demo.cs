using UnityEngine;
using TextToSpeechApi;
using UnityEngine.UI;
using System.Collections;
using System;

public class Demo : MonoBehaviour
{
    private TextToSpeech textToSpeech = new TextToSpeech();
    private VoiceToken[] voices;
    public Dropdown voiceDropdown;
    public InputField textField;

    // Start is called before the first frame update
    void Start()
    {
        textToSpeech.Init();
        PopulateAvailableVoices();
    }

    private void OnDestroy()
    {
        textToSpeech.Stop();
    }

    /// <summary>
    /// Populates the "voice" dropdown with the available text to speech voices.
    /// </summary>
    public void PopulateAvailableVoices()
    {
        voices = textToSpeech.GetSpeechVoices();
        for (int i = 0; i < voices.Length; i++)
        {
            voiceDropdown.options.Add(new Dropdown.OptionData { text = voices[i].Name });
        }
        voiceDropdown.value = 0;
        voiceDropdown.RefreshShownValue();
        // Setting the first voice as default
        VoicesDropdownOnUpdate(0);
    }

    /// <summary>
    /// Speech action executed by the "Speech it!" button.
    /// </summary>
    public void Speech()
    {
        Debug.Log("Processing speech...");
        textToSpeech.SpeechText(textField.text).OnSuccess((audioData) =>
        {
            Debug.Log($"Speech completed, speaking now {textField.text}");
            // Create a new game component with an audio source component attached to it
            GameObject audioGameObject = new GameObject();
            audioGameObject.name = $"voice: {voices[voiceDropdown.value].Name}";
            AudioSource audioSource = audioGameObject.AddComponent(typeof(AudioSource)) as AudioSource;
            AudioClip audioClip = AudioClip.Create(audioGameObject.name, audioData.value.Length, 1, textToSpeech.samplerate, false);
            audioClip.SetData(audioData.value, 0);
            audioSource.clip = audioClip;
            // Play the text to speech audio!
            audioSource.Play();
            // Once the audio is finished, destroy the game object.
            StartCoroutine(WaitForTextToSpeechSoundToFinish(audioSource));
        });
    }

    IEnumerator WaitForTextToSpeechSoundToFinish(AudioSource audioSource)
    {
        while (audioSource.isPlaying)
        {
            yield return null;
        }
        Destroy(audioSource.gameObject);
    }

    /// <summary>
    /// Updates speech speed, action executed by the "Speed" slider on change.
    /// </summary>
    /// <param name="newSpeed">New speed to set</param>
    public void SpeedSliderOnUpdate(float newSpeed)
    {
        Debug.Log($"Setting {Convert.ToInt32(newSpeed)} as speech speed");
        textToSpeech.SetNewSpeechSpeed(Convert.ToInt32(newSpeed));
    }

    /// <summary>
    /// Updates speech voice, action executed by the "Voice" dropdown on change.
    /// </summary>
    /// <param name="voiceTokenId">New voice id to set</param>
    public void VoicesDropdownOnUpdate(int voiceTokenId)
    {
        VoiceToken voiceToken = voices[voiceTokenId];
        Debug.Log($"Setting {voiceToken.Name} as speech voice (ID: {voiceToken.Id})");
        textToSpeech.SetNewSpeechVoice(voiceToken.Id);
    }

}
