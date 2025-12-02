using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
public class MenuController : MonoBehaviour
{
    public bool paused;
    [HideInInspector]
    public bool settingsMenuOpen;

    public SettingsMenu settingsMenu;
    public static MenuController instance;
    public CanvasGroup menuHUD;
    public CanvasGroup playerHudParent;
    public CanvasGroup[] playerHUD;
    public CanvasGroup playerCursorHUD;
    private int playerHudCounter;
    private int cursorHUDCounter;

    public CanvasGroup deathHUD;
    public Volume vfxBlur;
    public AudioSource menuAudio;
    public AudioClip pausSound;

    [Space(50)]
    public GameObject button_Continue;

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
        if (SceneManager.GetActiveScene().buildIndex == 1)
            PauseState(false);
        else
        {
            EnemyController.instance.FreezeGhost(false);
            UpdateButtons();
        }
            
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
            return;
       if (PlayerController.instance.menuInput.action.WasPerformedThisFrame() || Input.GetKeyDown(KeyCode.F1))
       {
            if (EndGame.instance.creditsActive)
                return;

                PauseState(paused? false : true);
       }

    }
    public void PauseState(bool _pause)
    {
        if (_pause)
        {
            UpdateButtons();
            menuAudio.PlayOneShot(pausSound);
        }
        else
            menuAudio.Stop();

        settingsMenu.CloseSettingTabs();
        settingsMenu.SettingButtonsVisible(false);

        Time.timeScale = _pause ? 0 : 1;
        vfxBlur.weight = _pause ? 1 : 0;
        menuHUD.alpha = _pause ? 1 : 0;
        menuHUD.blocksRaycasts = _pause ? true : false;
        menuHUD.interactable = _pause ? true : false;
        showHUD(_pause ? false : true);
        PlayerController.instance.canMove = _pause ? false : true;
        PlayerController.instance.MouseController(_pause ? 1 : -1);
        paused = _pause ? true : false;
    }
    public void UpdateButtons()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        bool gotSave = PlayerPrefs.HasKey("ItemData");
        if (currentScene == 0)
        {
            button_Continue.SetActive(gotSave);
        }
        
    }
    public void ContinueGame()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        
        if(currentScene == 0) // main menu
            SceneManager.LoadScene(1);
        else                    
            PauseState(false);// in game
    }
    public void LoadGame()
    {
        SaveController.instance.LoadSave();
        PauseState(false);
    }
    public void RestartGame()
    {
        SaveController.instance.DeleteSave();
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(1);
        Debug.Log("Vi Startade om");
    }
    public void settingsButton()
    {
        if(!settingsMenuOpen)
        {
            settingsMenu.SettingButtonsVisible(true);
            settingsMenu.OpenSettingsTab(0);
            settingsMenuOpen = true;
        }
        else
        {
            settingsMenu.SettingButtonsVisible(false);
            settingsMenu.CloseSettingTabs();
            settingsMenuOpen = false;
        }
        
    }
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
        Debug.Log("Vi laddade huvudmenyn");
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public void DeathScreen()
    {
        deathHUD.alpha = 1;
        deathHUD.blocksRaycasts = true;
        deathHUD.interactable = true;
        PlayerController.instance.MouseController(1);
        //menuAudio.PlayOneShot(deathSound);
        SaveController.instance.DeleteSave();
    }
    public void showHUD(bool showHud)
    {
        playerHudParent.alpha = showHud ? 1 : 0;
    }
    public void ShowHUDSettings(bool showHud)
    {
        if (playerHUD == null)
            return;
        foreach(CanvasGroup canvas in playerHUD)
        {
            canvas.alpha = showHud ? 1 : 0;
        }       
    }
    public void showCursorSettings(bool ShowCursor)
    {   
        if(playerCursorHUD)
        playerCursorHUD.alpha = ShowCursor ? 1 : 0;
    }
}
