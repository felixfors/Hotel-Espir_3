using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentAudio : MonoBehaviour
{
    public static EnvironmentAudio instance;
    public List<OutsideSoundMuffer> nearbyWindows = new();
    public OutsideSoundMuffer nearestWindow;
    

    private float nearestWindowDistance;
    
    public AudioSource outsideAudioSource;
    private float volumeTarget;
    public float volume; // 0–1 beroende på avstånd
    public float minDistance = 5f;
    public float maxDistance = 10f;

    public float thunderMuffleDistance;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        
    }
    private void Start()
    {
        outsideAudioSource = ThunderController.instance.thunderAudio;
    }

    // Update is called once per frame
    private void Update()
    {
        if (nearestWindow != null)
        {
            float nearestWindowDistance = Vector3.Distance(PlayerController.instance.transform.position,nearestWindow.transform.position);

            // volym = 1 nära fönstret, 0 längre bort
            volume = 1f - Mathf.InverseLerp(minDistance, maxDistance, nearestWindowDistance);

            // applicera direkt på AudioSource
            volumeTarget = volume;
        }
        else
        {
            volume = 0f;
            volumeTarget = 0.6f;
        }
        ThunderController.instance.thunderAudio.volume = Mathf.Lerp(ThunderController.instance.thunderAudio.volume, volumeTarget, Time.deltaTime*2);
    }

    public void AddWindow(OutsideSoundMuffer newWindow, float distance)
    {
        nearbyWindows.Add(newWindow);
        if (nearestWindow == null)
            nearestWindow = newWindow;
        else
        {
            float nearestWindowDistance = Vector3.Distance(PlayerController.instance.transform.position, nearestWindow.transform.position);

            if (distance < nearestWindowDistance)
                nearestWindow = newWindow;
        }

        

        
    }
    public void RemoveWindow(OutsideSoundMuffer oldWindow)
    {
        // Ta bort fönstret ur listan först
        nearbyWindows.Remove(oldWindow);

        // Om vi tog bort närmsta, måste vi hitta ett nytt
        if (oldWindow == nearestWindow)
        {
            nearestWindow = null;
            float nearestDist = float.MaxValue;

            foreach (var window in nearbyWindows)
            {
                float dist = Vector3.Distance(PlayerController.instance.transform.position, window.transform.position);

                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestWindow = window;
                }
            }
        }
    }
}
