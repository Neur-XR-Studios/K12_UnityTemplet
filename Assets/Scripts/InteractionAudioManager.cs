using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionAudioManager : MonoBehaviour
{
    public static InteractionAudioManager Instance;

    [SerializeField] AudioSource audioSource;

    private void Awake()
    {
        Instance = this; 
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayAudio(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }
}
