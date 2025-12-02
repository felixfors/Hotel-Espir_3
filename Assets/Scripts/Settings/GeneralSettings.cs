using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class GeneralSettings : MonoBehaviour
{
    public static GeneralSettings instance;

    [Header("HudScale")]
    public CanvasScaler hud_canvasScaler;

    [Header("Lock Mouse On drag")]
    public int cameraFollowDrag_Settings;
    public bool cameraFollowDrag;
    public TextMeshProUGUI cameraFollowDrag_UI;

    [Header("Display FPS")]
    public int displayFPS_Settings;
    public TextMeshProUGUI displayFPS_UI;
    public FPSCounter fpsCounter;

    [Header("Display HUD")]
    public int displayHUD_Settings;
    public TextMeshProUGUI displayHUD_UI;


    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {      
        CameraFollowDrag(PlayerPrefs.GetInt("CameraFollowDrag"));
        DisplayFPS(PlayerPrefs.GetInt("DisplayFPS"));
        DisplayHUD(PlayerPrefs.GetInt("DisplayHUD"));
    }
    public void newHUDScale(float newValue)
    {
        float original = newValue;
        float inverted = 1f - original;
        if(hud_canvasScaler != null)
            hud_canvasScaler.matchWidthOrHeight = inverted;
    }
    public void CameraFollowDrag(int qualitySetting)
    {
        cameraFollowDrag_Settings += qualitySetting;
        int maxSetting = 1;
        if (cameraFollowDrag_Settings > maxSetting)
            cameraFollowDrag_Settings = 0;
        else if (cameraFollowDrag_Settings < 0)
            cameraFollowDrag_Settings = maxSetting;
        PlayerPrefs.SetInt("CameraFollowDrag", cameraFollowDrag_Settings);


        switch (cameraFollowDrag_Settings)
        {
            case 0:
                cameraFollowDrag = true;
                cameraFollowDrag_UI.text = "on";
                break;
            case 1:
                cameraFollowDrag = false;
                cameraFollowDrag_UI.text = "off";
                break;
        }
    }

    public void DisplayFPS(int qualitySetting)
    {
        displayFPS_Settings += qualitySetting;
        int maxSetting = 1;
        if (displayFPS_Settings > maxSetting)
            displayFPS_Settings = 0;
        else if (displayFPS_Settings < 0)
            displayFPS_Settings = maxSetting;
        PlayerPrefs.SetInt("DisplayFPS", displayFPS_Settings);
        

        switch (displayFPS_Settings)
        {
            case 0:
                fpsCounter.DisplayFPS_Status(false);
                displayFPS_UI.text = "off";               
                break;
            case 1:
                fpsCounter.DisplayFPS_Status(true);
                displayFPS_UI.text = "on";
                break;
        }
    }
    public void DisplayHUD(int qualitySetting)
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
            return;

        displayHUD_Settings += qualitySetting;
        int maxSetting = 2;
        if (displayHUD_Settings > maxSetting)
            displayHUD_Settings = 0;
        else if (displayHUD_Settings < 0)
            displayHUD_Settings = maxSetting;
        PlayerPrefs.SetInt("DisplayHUD", displayHUD_Settings);

        switch (displayHUD_Settings)
        {
            case 0:
                MenuController.instance.ShowHUDSettings(true);
                MenuController.instance.showCursorSettings(true);
                displayHUD_UI.text = "on";
                break;
            case 1:
                MenuController.instance.ShowHUDSettings(false);
                MenuController.instance.showCursorSettings(false);
                displayHUD_UI.text = "off";
                break;
            case 2:
                MenuController.instance.ShowHUDSettings(false);
                MenuController.instance.showCursorSettings(true);
                displayHUD_UI.text = "only cursor";
                break;
        }
    }
}
