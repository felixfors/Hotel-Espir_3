using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicTrigger : MonoBehaviour
{
    public TrackState trackCategory;
    private bool insideBiom;
    private bool biomQue;



    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            
            if(MusicController.instance.trackState != TrackState.scared && MusicController.instance.trackState != TrackState.chased)
            {
                MusicController.instance.SwitchTrackState(trackCategory);
            }
                     
            MusicController.instance.currentBiom = this;

            insideBiom = true;
        }      
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            insideBiom = false;
        }
    }
    public void resetMusicToBiom()
    {
        MusicController.instance.SwitchTrackState(trackCategory);
    }
}
