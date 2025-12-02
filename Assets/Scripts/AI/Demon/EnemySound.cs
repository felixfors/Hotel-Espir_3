using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySound : MonoBehaviour
{
    public static EnemySound instance;
    private bool playSound;
    private float voiceLineTimer;
    private Vector2 voiceLineDelay = new Vector2(1,1);
    
    public AudioSource voiceAudio;
    private AudioClip lastSound;
    public AudioClip[] chaseSound;
    public AudioClip[] searchSound;
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }
    private void Update()
    {
        if (EnemyController.instance.banishActive || EnemyController.instance.playerIsCaught)
            return;
        if (voiceLineTimer > 0)
        {
            playSound = false;
            voiceLineTimer -= Time.deltaTime;
        }          
        else
        {
            if(!playSound)
            {
                if (EnemyController.instance.enemyState == EnemyController.States.chase)
                    ChaseSound();
                if (EnemyController.instance.enemyState == EnemyController.States.search)
                    SearchSound();

                playSound = true;
            }
        }
    }
    private void RefreshTimer(AudioClip lastAudioClip)
    {
        float newTimer = lastAudioClip ? lastAudioClip.length : 0 + Random.Range(voiceLineDelay.x, voiceLineDelay.y);
        voiceLineTimer = newTimer;
    }
    public void SearchSound()
    {
        if(voiceAudio.isPlaying) // search körs varje gång ett objet gör ljud, därav denna lösning
        {
            RefreshTimer(null);
            return;
        }
        AudioClip _newSound = searchSound[Random.Range(0, searchSound.Length)];
        while (_newSound == lastSound)
            _newSound = searchSound[Random.Range(0, searchSound.Length)];
        
        lastSound = _newSound;

        voiceAudio.PlayOneShot(_newSound);
        RefreshTimer(_newSound);
    }
    public void ChaseSound()
    {
        AudioClip _newSound = chaseSound[Random.Range(0, chaseSound.Length)];
        while (_newSound == lastSound)
            _newSound = chaseSound[Random.Range(0, chaseSound.Length)];
        
        lastSound = _newSound;

        voiceAudio.PlayOneShot(_newSound);
        RefreshTimer(_newSound);
    }
}
