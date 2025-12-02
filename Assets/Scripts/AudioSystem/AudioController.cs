using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public List<SoundboardPlayer> soundboardData = new();
    [System.Serializable]
    public class SoundboardPlayer
    {
        public AudioSource audioSource;
        public AudioClip sound;
        public float volume;
        public string description;
    }

    public void PlaySoundEffect(int id)
    {
        if (id > soundboardData.Count)
            return;
        
        SoundboardPlayer newSound = soundboardData[id];

        if(newSound.audioSource && newSound.sound)
        newSound.audioSource.PlayOneShot(newSound.sound,newSound.volume);
    }
}
