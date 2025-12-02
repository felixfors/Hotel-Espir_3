using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using DG.Tweening;

public class EndGame : MonoBehaviour
{
    public static EndGame instance;
    public AudioClip creditSound;
    public AudioMixer audioMixer;

    public bool creditsActive;

    public CanvasGroup endScreenUI;


    public ScrollRect scrollRect;
    [Range(0f, 1f)]
    private float scrollValue = 1f; // 1 = toppen, 0 = botten
    public float scrollDuration;

    private Coroutine holdCoroutine;
    private float holdTime = 2.5f;
    public Image fillImage;
    public InputSprite skipInputIcon;

    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (!creditsActive)
            return;

        scrollRect.verticalNormalizedPosition = scrollValue;

        if(PlayerController.instance.menuSkip_static.action.WasPressedThisFrame())
        {
            fillImage.fillAmount = 0;
            holdCoroutine = StartCoroutine(HoldCoroutine());
        }
        else if(PlayerController.instance.menuSkip_static.action.WasReleasedThisFrame())
        {
            if(holdCoroutine != null)
            {
                StopCoroutine(holdCoroutine);
                holdCoroutine = null;
            }                                  
            fillImage.fillAmount = 0;
        }
    }
    IEnumerator HoldCoroutine()
    {
        float startValue = 0f;
        float endValue = 1f;
        float elapsed = 0f;

        while (elapsed < holdTime)
        {
            fillImage.fillAmount = Mathf.Lerp(startValue, endValue, elapsed / holdTime);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        fillImage.fillAmount = endValue;

        OnHoldComplete();
    }
    private void OnHoldComplete()
    {
        PlayerController.instance.MouseController(1);
        SceneManager.LoadScene(0);
    }
    public void EndScene()
    {
        skipInputIcon.IsActive(true);
        creditsActive = true;
        //endScreenUI.alpha = 1;
        endScreenUI.interactable = true;
        endScreenUI.blocksRaycasts = true;

        SaveController.instance.DeleteSave();       
        Time.timeScale = 0;
        PlayerController.instance.canMove = false;
        PlayerController.instance.MouseController(-1);  
        
        StartCoroutine(RollCredits());
        endScreenUI.DOFade(1f, 1f).SetUpdate(true).SetEase(Ease.Linear);
        MuteAllSound();
        BackgroundMusic.instance.ChangeBackgroundmusic(creditSound,true);
        //inputspire.IsActive(true);
    }
    IEnumerator RollCredits()
    {
        float startValue = 1f;
        float endValue = 0f;
        float elapsed = 0f;

        while (elapsed < scrollDuration)
        {
            scrollValue = Mathf.Lerp(startValue, endValue, elapsed / scrollDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        OnHoldComplete();
        scrollValue = endValue; // Se till att det landar exakt på 0
    }
    private void MuteAllSound()
    {

        foreach (SettingsController.AudioSettings audio in SettingsController.instance.audioSettings)
        {
            if(audio.name != "Music_Volume" && audio.name != "Master_Volume")
            {
                audioMixer.SetFloat(audio.name, -80f);
            }            
        }
        
    }
}
