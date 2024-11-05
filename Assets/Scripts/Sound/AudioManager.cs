using UnityEngine;
using System;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    public static AudioManager instance;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    void Awake()
    {
        // Singleton pattern to keep AudioManager across scenes
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        // Initialize each sound and assign the correct audio source
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

            s.source.outputAudioMixerGroup = s.isMusic ? musicGroup : sfxGroup;
        }
    }

    private void Start()
    {
        // Play Main Menu Music
        Play("RockandRace");
    }

    public void Play(string name)
    {
        Sound soundData = Array.Find(sounds, sound => sound.name == name);
        if (soundData == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        soundData.source.Play();
    }

    public void Stop(string name)
    {
        Sound soundData = Array.Find(sounds, sound => sound.name == name);
        if (soundData == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        soundData.source.Stop();
    }
}
