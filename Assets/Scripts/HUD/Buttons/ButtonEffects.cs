using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonEffects : MonoBehaviour
{
    public Animator anim;
    public AudioSource audioSource;
    public AudioClip [] soundSFX;
    
    public void OnButtonClick()
    {
        audioSource.PlayOneShot(soundSFX[0]);
        //if (anim != null)
            //anim.SetTrigger("PingPong");
    }
    public void OnButtonEnter()
    {
        audioSource.PlayOneShot(soundSFX[1]);
        if (anim != null)
        {
            anim.SetInteger("Direction", 1);
            anim.SetTrigger("Trigger");          
        }
            
    }
    public void OnButtonExit()
    {
        if (anim != null)
        {
            anim.SetInteger("Direction", 0);
            anim.SetTrigger("Trigger");
        }           
    }
}
