
using UnityEngine;

public class ButtonFXController : MonoBehaviour
{
    public AudioSource buttonAudioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;
    public AudioClip sliderDrag;
    public void HoverSound()
    {
        buttonAudioSource.PlayOneShot(hoverSound);
    }
    public void ClickSound()
    {
        buttonAudioSource.PlayOneShot(clickSound);
    }
    public void SliderDragSound()
    {
        buttonAudioSource.PlayOneShot(sliderDrag);
    }

}
