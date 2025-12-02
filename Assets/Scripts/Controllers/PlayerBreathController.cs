using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Cinemachine;

public class PlayerBreathController : MonoBehaviour
{
    public static PlayerBreathController instance;
    private float breathTimer = 0f;  // håller koll på tiden
    private float breathInterval = 2f; // 2 sekunder
    public CinemachineImpulseSource exhaleImpulse;

    public bool holdingBreath;
    public float maxBreathHold;
    private bool rebreathCooldown;
    public float rebreathCoolDownTimer = 5;
    
    private float breathVelocity = 0f;

    public Slider maxBreathSlider;
    private float maxBreathLeft;    
    public Slider currentBreathSlider;
    private float currentBreath;
 
    public CanvasGroup BreathBarUI;

    public AudioSource audioSource;
    public AudioClip[] normalBreathing;//0
    public AudioClip[] inhaleSound;//1
    public AudioClip[] exhaleSound;//2
    public AudioClip[] gaspSound;//3
    public AudioClip[] heavyBreathingSound;//4
    private AudioClip lastSound;
    

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {

        ResetBreath();
        UpdateSliders();
        ShowUI(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (MenuController.instance.paused)
            return;
        if(PlayerController.instance.holdBreathInput.action.WasPressedThisFrame() && !rebreathCooldown)
        {
            if (!holdingBreath)// om vi inte håller andan börja håll andan
            {
                PlaySoundEffect(1);
                RestartBreath();
                
                holdingBreath = true;
            }               
            else// om håller andan och trycker så ska timern börja
            {
                PlaySoundEffect(3); //gasp sound 
                //Uppdatera Maxbreath
                bool belowMinimumBreath = currentBreath <= 0.07f;
                float amountOfBreathToRemove = belowMinimumBreath? 0.07f : currentBreath;

                maxBreathLeft -= amountOfBreathToRemove;

                //Flytta currentbreaht till nya maxbreath
                currentBreath = maxBreathLeft;
            }
        }        
        if(holdingBreath)
        {
            if(currentBreath >0)
            {
                HoldingBreath();
                UpdateSliders();
            }
            else
            {
                rebreathCooldown = true;
                PlaySoundEffect(2); //exhale sound
                exhaleImpulse.GenerateImpulse(transform.forward);
                StartCoroutine(BreathCooldown(rebreathCoolDownTimer));
                ShowUI(false);
                holdingBreath = false;               
            }          
        }
        else
        {
            breathTimer += Time.deltaTime;
           
            if (!audioSource.isPlaying && breathTimer >= breathInterval)
            {
                PlaySoundEffect(rebreathCooldown? 4 : 0);   // spela ljudet
                breathTimer = 0f;     // återställ timern
            }
        }
    }
    private void ShowUI(bool show)
    {
        BreathBarUI.alpha = show? 1 : 0;
    }
    private void HoldingBreath()
    {
        //currentBreath = Mathf.MoveTowards(currentBreath, 0, Time.deltaTime / maxBreathHold); // linjärt
        currentBreath = Mathf.SmoothDamp(currentBreath, 0f,ref breathVelocity, maxBreathHold);
        if (currentBreath < 0.001f)// threshold
        {
            currentBreath = 0;          
        }            
    }
    private void RestartBreath()
    {
        ShowUI(true);
        maxBreathLeft = 1;
        currentBreath = 1;
    }
    IEnumerator BreathCooldown(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        rebreathCooldown = false;
    
    }
    private void UpdateSliders()
    {
        if (currentBreathSlider.value != currentBreath)
            currentBreathSlider.value = currentBreath;
        
        if (maxBreathSlider.value != maxBreathLeft)
            maxBreathSlider.value = maxBreathLeft;
    }
    public void ResetBreath() // körs när spelet bland annat startar om, uttifall vi blir tagna medans vi håller handan etc
    {
        ShowUI(false);
        StopAllCoroutines(); // avsluta cooldownTimer
        rebreathCooldown = false;
        maxBreathLeft = 1;
        currentBreath = 1;
    }
    private void PlaySoundEffect(int soundID) // 0 normal breathing, 1 inhale, 2 exhale, 3 gasp
    {
        AudioClip[] clips = null;
        float loudnessLevel = 0;
        audioSource.volume = (soundID == 0 ? 0.5f : 1);

        switch (soundID)
        {
            case 0:
                clips = normalBreathing;
                //Debug.Log("normal breahting");
                loudnessLevel = 6; //6 meters
                break;

            case 1:
                clips = inhaleSound;
                loudnessLevel = 0; // 0 meters
                break;

            case 2:
                clips = exhaleSound;
                loudnessLevel = 10; // 10 meters
                break;

            case 3:
                clips = gaspSound;
                loudnessLevel = 0; // 0 meters
                break;
            case 4:
                clips = heavyBreathingSound;
                loudnessLevel = 6; // 6 meters
                break;
        }

        EnemyController.instance.SoundImpact(loudnessLevel, transform);
        PlayerAudioDetection.instance.SoundImpact(loudnessLevel);

        List<AudioClip> noDouble = new();
        for(int i = 0; i <clips.Length; i ++) // gå igenom alla ljud som ska spelas
        {
            if (clips[i] != lastSound) // kolla så vi inte spelar samma ljud som innan
                noDouble.Add(clips[i]); // lägg till alla ljud som är unika
        }

        if (noDouble.Count > 0)
        {
            int randomIndex = Random.Range(0, noDouble.Count);
            audioSource.PlayOneShot(noDouble[randomIndex]);
            
            lastSound = noDouble[randomIndex];
        }
    }
}
