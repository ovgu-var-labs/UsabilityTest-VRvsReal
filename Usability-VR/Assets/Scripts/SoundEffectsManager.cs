using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectsManager : MonoBehaviour
{
    public EventManager eventManager;
    public GameObject soundEffectPrefab;

    public AudioClip defaultNotification;

    private void Start()
    {
        eventManager.OnSoundEffectPlayed += PlaySoundEffectOnce;
    }

    /// <summary>
    /// Play a sound to indicate the disinfection is completed
    /// </summary>
    void PlaySoundEffectOnce(AudioClip clip, Vector3 location, float volume)
    {
        //Debug.Log("Playing notification sound.");
        AudioSource.PlayClipAtPoint(clip, location, volume);
    }
}
