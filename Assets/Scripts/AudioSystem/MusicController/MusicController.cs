using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    public static MusicController instance;

    public MusicTrigger currentBiom;//senaste music triggern som vi använde, kollar av vilken music zone vi är i

    public bool firstTrack;
    public AudioSource[] audioSource;


    private Coroutine fadeCoroutine;
    private TrackState previousState;
    public TrackState trackState;
    

    public List<Track> track = new();
    [System.Serializable]
    public class Track
    {
        public string trackCategory;
        public AudioClip[] song;
        public float volumeTarget;
    }

    private void Awake()
    {
        instance = this;
        HandleTrackStateChange(trackState);
    }
    private void Update()
    {
        if (trackState != previousState)
        {

            previousState = trackState;
            HandleTrackStateChange(trackState);
        }
    }
    private void HandleTrackStateChange(TrackState newState)
    {
        AudioClip newSong = null;
        switch (newState)
        {
            case TrackState.idle:
                newSong = track[0].song[Random.Range(0, track[0].song.Length)];
                break;
            case TrackState.creepy:
                newSong = track[1].song[Random.Range(0, track[1].song.Length)];
                break;
            case TrackState.scared:
                newSong = track[2].song[Random.Range(0, track[2].song.Length)];
                break;
            case TrackState.mystery:
                newSong = track[3].song[Random.Range(0, track[3].song.Length)];
                break;
            case TrackState.chased:
                newSong = track[4].song[Random.Range(0, track[4].song.Length)];
                break;
        }
        if(newSong)
        {
            NewMusicTrack(newSong);
        }
        
    }
    private void NewMusicTrack(AudioClip song, float fadeDuration = 1f)
    {
        firstTrack = !firstTrack;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        if (firstTrack)
        {
            audioSource[0].clip = song;
            audioSource[0].Play();
            fadeCoroutine = StartCoroutine(FadeTracks(audioSource[0], audioSource[1], fadeDuration, track[(int)trackState].volumeTarget));
        }
        else
        {
            audioSource[1].clip = song;
            audioSource[1].Play();
            fadeCoroutine = StartCoroutine(FadeTracks(audioSource[1], audioSource[0], fadeDuration, track[(int)trackState].volumeTarget));
        }
    }

    private IEnumerator FadeTracks(AudioSource fadeIn, AudioSource fadeOut, float duration, float targetVolume = 1f)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            fadeIn.volume = Mathf.Lerp(0f, targetVolume, t);
            fadeOut.volume = Mathf.Lerp(targetVolume, 0f, t);

            yield return null;
        }

        fadeIn.volume = targetVolume;
        fadeOut.volume = 0f;
        fadeOut.Stop();
    }

    public void SwitchTrackState(TrackState newTrackState)
    {
        //if(newTrackState != trackState)
        trackState = newTrackState;
    }
}
