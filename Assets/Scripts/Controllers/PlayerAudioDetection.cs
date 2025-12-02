using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAudioDetection : MonoBehaviour
{
    public static PlayerAudioDetection instance;
    public Image awarnessHUD;

    private float currentSound;



    private float volumeClamped;

    public float currentVolume;
    public float targetVolume;
    
    public float resetTime;
    public float maxVolume;

    public bool debugGizmo;
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {        

        //currentSound = Mathf.Lerp(currentSound, targetVolume, Time.deltaTime * resetTime*2);
        targetVolume -= Time.deltaTime * resetTime;
        targetVolume = Mathf.Clamp01(targetVolume);
        awarnessHUD.fillAmount = targetVolume;
    }
    public void SoundImpact(float _loudness) // för när objekt kastas osv, loudness är antal meter
    {
        float converter = (1f / 25f) * _loudness;
        
        if (converter > targetVolume) // vi ska bara plussa på mellan skillnaden, annars kan visaren bli misledande om vi bara plussar på hela tiden
        {
            converter -= targetVolume;
            targetVolume += converter;
        }

    }
    public void VolumeIncrease(float _loudness, float multiplier) // för när spelaren rör på sig
    {
        //targetVolume += loudness * multiplier;
        float loudness = _loudness * multiplier;
        float converter = (1f / 25f) * loudness;
        //targetVolume += converter;
        if(converter > targetVolume)
        {   
            converter -= targetVolume;
            targetVolume += converter;
        }


        //Debug.Log("så här mycket " + targetVolume);
        EnemyController.instance.SoundImpact(loudness, transform);
    }
}
