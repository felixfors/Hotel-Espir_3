using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderController : MonoBehaviour
{
    public static ThunderController instance;
    public static event Action onThunderPlayed;

    public AudioSource thunderAudio;
    public Vector2 thunderIntensityVariation;
    //[HideInInspector]
    public float thunderIntensity;
    //[HideInInspector]
    public float thunderIntensityOrignal;
    [Space(50)]
    public AudioClip [] thunderSound;


    private float nextThunder;
    public Vector2 thunderIntervalls;

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        thunderIntensityOrignal = thunderIntensity;
        GetNextThunder();
    }

    // Update is called once per frame
    void Update()
    {
        if (nextThunder > 0)
            nextThunder -= Time.deltaTime;
        else
        {
            PlayThunderSound();
            GetNextThunder();
        }
    }
    private void GetNextThunder()
    {
        nextThunder = UnityEngine.Random.Range(thunderIntervalls.x, thunderIntervalls.y);
    }
    private void PlayThunderSound()
    {
        
        thunderIntensity = UnityEngine.Random.Range(thunderIntensityVariation.x, thunderIntensityVariation.y);      
        thunderAudio.pitch = UnityEngine.Random.Range(0.9f,1.2f);
        thunderAudio.PlayOneShot(thunderSound[UnityEngine.Random.Range(0, thunderSound.Length)]);
        onThunderPlayed?.Invoke();
        Invoke("ResetLightning", 0.1f);
    }
    private void ResetLightning()
    {      
        thunderIntensity = 0;
        onThunderPlayed?.Invoke();
    }
}
