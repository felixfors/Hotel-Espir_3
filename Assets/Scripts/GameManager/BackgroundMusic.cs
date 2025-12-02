using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic instance;
    public AudioSource audioSource;
    public AudioClip mainBackgrounSound;

    // Start is called before the first frame update
    private void Awake()
    {
        instance = this;

    }

    public void ChangeBackgroundmusic(AudioClip newSound, bool loop)
    {
        if (!newSound)
        {
            audioSource.Stop();
            return;
        }
            

        if (loop == false && mainBackgrounSound !=null)
            DOVirtual.DelayedCall(newSound.length, () => ChangeBackgroundmusic(mainBackgrounSound, true));
        
        audioSource.loop = loop;
        audioSource.PlayOneShot(newSound);
    }
}
