using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

namespace Match3
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager: MonoBehaviour 
    {
        [SerializeField] AudioClip click;
        [SerializeField] AudioClip deselect;
        [SerializeField] AudioClip match;
        [SerializeField] AudioClip noMatch;
        [SerializeField] AudioClip woosh;
        [SerializeField] AudioClip pop;

        AudioSource audioSource;

        public void PlayClick() => audioSource.PlayOneShot(click);
        public void PlayDeselect() => audioSource.PlayOneShot(deselect);
        public void PlayMatch() => audioSource.PlayOneShot(match);
        public void PlayNoMatch() => audioSource.PlayOneShot(noMatch);
        public void PlayWoosh() => audioSource.PlayOneShot(woosh);
        public void PlayPop() => audioSource.PlayOneShot(pop);
        private void OnValidate()
        {
            if(audioSource == null) audioSource = GetComponent<AudioSource>();
        }

        void PlayRandomPitch(AudioClip audioClip)
        {
            audioSource.pitch = Random.Range(0.9f,1.1f);
            audioSource.PlayOneShot(audioClip);
            audioSource.pitch = 1f;
        }

    }
}