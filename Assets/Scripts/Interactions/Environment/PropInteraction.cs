using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PropInteraction : MonoBehaviour
{
    public bool interacting;
    public bool on;
    private bool coolDown;
    public float cooldownTimer;

    [Space(20)]
    public Animator anim;
    public string animTriggerName;

    [Space(20)]
    public AudioSource audioSource;
    public AudioClip soundInteract;
    public AudioClip soundEnd;


    public UnityEvent InteractEvent;

    public UnityEvent onEvent;
    public UnityEvent offEvent;

    // Start is called before the first frame update
    void Start()
    {
        coolDown = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(interacting && PlayerController.instance.itemInteractInput.action.WasPerformedThisFrame() && !coolDown)
        {
            on = !on;

            Invoke("CooldownTimer", cooldownTimer);
            coolDown = true;
            if (audioSource && soundInteract)
                audioSource.PlayOneShot(soundInteract);
           
            if (anim)
            {
                if (animTriggerName != "")
                    anim.SetTrigger(animTriggerName);
            }
            InteractEvent.Invoke();

            if (on)
                onEvent.Invoke();
            else
                offEvent.Invoke();
        }
    }
    private void CooldownTimer()
    {
        if (audioSource && soundEnd)
            audioSource.PlayOneShot(soundEnd);
        coolDown = false;
    }


    public void StartInteract()
    {
        interacting = true;
    }
    public void EndInteract()
    {
        interacting = false;
    }
}
