using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
public class InterActiveButton : MonoBehaviour
{
    private string id; // used for storing data/saved files
    public NewTextFeed hintText;
    public NoteData noteData;
    private bool unlocked;
    [Header("Interact settings")]
    [Header("Automaticly accept the password if it is correct")]
    public bool autoCompletePassword;
    public float maxDistance;
    public KeybindType keybindType;
    public enum KeybindType
    {
        mouse_LeftClick,mouse_RightClick, mouse_Scrollwheel, keyboard
    }
    public KeyCode keyboard_keyBind;

    public Camera playerCamera;
    [Space(50)]
    [Header("True = interact from center of screen.")]
    [Header("False = interact from mouse position")]
    public bool rayFromCenter; 


    [Space(50)]
    private bool inRange;
    
    
    
    [Header("Password settings")]
    public string password;
    private string temporaryText;
    public int maxPasswordLetters;
    private Canvas textCanvas;
    public TextMeshProUGUI textGUI;

    [Space(50)]
    
    public bool coolDown;
    [Header("Lock the actions when the password is correct")]
    public bool dontResetOnCorrect;
    [Header("Restart when the combination is not correct")]
    public bool restartOnWrong;

    [Header("CoolDown when the password is correct")]
    public float delayActionOnCorrect;
    [Header("CoolDown when the password is wrong")]
    public float delayActionOnFalse;
    
    [Space(50)]
    [Header("SFX")]
    public AudioSource audioSorce;
    public AudioClip buttonClickSound;
    public AudioClip correctPasswordSound;
    public AudioClip wrongPasswordSound;
    [Space(50)]
    [Header("VFX")]
    public MeshRenderer [] emissionMaterial;
    [ColorUsage(true,true)]
    public Color correctColor;
    [ColorUsage(true, true)]
    public Color wrongColor;
    [ColorUsage(true, true)]
    public Color regularColor;

    [Space(50)]
    [Header("All functions that should be triggered by the correct Password")]
    [SerializeField]
    private UnityEvent onCorrectPassword;
    [Header("All functions that should be triggered by the wrong password")]
    [SerializeField]
    private UnityEvent onFalsePassword;


    [Space(50)]
    [Header("All interactive buttons data")]
    public List<Buttons> buttonData = new();

    
    [System.Serializable]
    public class Buttons
    {
        public string representingText;
        public GameObject buttonTarget;
        [Header("unique Event for this button")]
        [SerializeField]
        public UnityEvent uniqueEvent;
        public bool onlyEventOnCorrect;
    }
    private void Awake()
    {
        if(textGUI)
        {
            textCanvas = textGUI.GetComponentInParent<Canvas>();
            textCanvas.worldCamera = playerCamera;
        }       
        EnableButtons();
        ResetCoolDown();
    }
    private void Update()
    {
       
        
        if (keybindType == KeybindType.keyboard && Input.GetKeyDown(keyboard_keyBind) || 
           keybindType == KeybindType.mouse_LeftClick && Input.GetMouseButtonDown(0) ||
           keybindType == KeybindType.mouse_RightClick && Input.GetMouseButtonDown(1) ||
           keybindType == KeybindType.mouse_Scrollwheel && Input.GetMouseButtonDown(2))// trying to interact with button
        {
            inRange = Vector3.Distance(transform.position, playerCamera.transform.position) <= maxDistance; // checking if we are in range for interacting
            if (inRange && !coolDown)
            {
                ButtonAction(); // we are in range, see if we hit something that is a button               
            }
        }
        if(textGUI)
        {
            if (textGUI.text != temporaryText) // update the password GUI 
            {
                textGUI.text = temporaryText;
            }
        }      
    }
    private void EnableButtons()
    {
        for(int  i = 0; i < buttonData.Count; i ++)
        {
            if(buttonData[i].buttonTarget.GetComponent<BoxCollider>())
                buttonData[i].buttonTarget.GetComponent<BoxCollider>().enabled = true;
        }
    }

    public void ButtonAction() 
    {
        RaycastHit hit;
        Ray ray = rayFromCenter? playerCamera.ViewportPointToRay(new Vector3(0.5f,0.5f,0)): playerCamera.ScreenPointToRay(Input.mousePosition); // cast ray from mouse position or center of screen

        if (Physics.Raycast(ray, out hit))
        {
            for(int i = 0; i <buttonData.Count; i ++) // going through the different buttons that we cant interact with and see if we hit any of them
            {
                if (hit.transform.gameObject == buttonData[i].buttonTarget) // we hit a button
                {
                    if (buttonData[i].buttonTarget.GetComponent<Animator>()) // check if the button has any animations that we can play
                        buttonData[i].buttonTarget.GetComponent<Animator>().SetTrigger("Play");
                    
                    temporaryText += buttonData[i].representingText; // add letter to temp password

                    
                   

                    if (audioSorce && buttonClickSound)
                        audioSorce.PlayOneShot(buttonClickSound);
                    
                    
                    if (restartOnWrong)
                    {
                        string checkTempPassword = password.Substring(0, temporaryText.Length);
                        if (temporaryText != checkTempPassword)
                        {
                            WrongPassword();
                            coolDown = true;
                            Invoke("ResetCoolDown", delayActionOnFalse);
                            break;
                        }    
                    }
                    if(buttonData[i].onlyEventOnCorrect)
                    {
                        string checkTempPassword = password.Substring(0, temporaryText.Length);
                        if (temporaryText == checkTempPassword)
                        {
                            buttonData[i].uniqueEvent?.Invoke();
                        }                                                    
                    }
                    else
                    {
                        buttonData[i].uniqueEvent?.Invoke();
                    }
                   


                    if (autoCompletePassword)
                        TryPassword();
                    else if(!autoCompletePassword && temporaryText.Length >= maxPasswordLetters && temporaryText != password)
                    {
                        TryPassword();
                    }
                }
            }
        }
    }
    public void TryPassword() // Try the current password
    {
        if (temporaryText == password) // we have the correct password
        {
            CorrectPassword();
            coolDown = true;
            if (!dontResetOnCorrect)
                Invoke("ResetCoolDown", delayActionOnCorrect);
        }
        else if (temporaryText.Length >= maxPasswordLetters || !autoCompletePassword) // we have the wrong password
        {
            WrongPassword();
            coolDown = true;
            Invoke("ResetCoolDown", delayActionOnFalse);
        }
    }
    public void ReverseButtonAction() // Remove last letter
    {
        if(temporaryText.Length >0)
        {
            var text = temporaryText.Substring(0, temporaryText.Length - 1);
            temporaryText = text;
        }
    }
    public void Restart()
    {
        temporaryText = "";
    }
    private void ResetCoolDown() // Reset the keypad
    {
        coolDown = false;
        temporaryText = "";

        if(emissionMaterial.Length >0)
        {
            for(int i = 0; i < emissionMaterial.Length; i ++)
            {
                Material tempEmission = emissionMaterial[i].material;// change Emission color
                tempEmission.SetColor("_EmissionColor", regularColor);
                emissionMaterial[i].material = tempEmission;
            }          
        }
        
    }
    private void CorrectPassword()
    {
        unlocked = true;
        if(audioSorce && correctPasswordSound)
            audioSorce.PlayOneShot(correctPasswordSound);

        onCorrectPassword?.Invoke(); // call all our events for correct password

        if (emissionMaterial.Length > 0)
        {
            for (int i = 0; i < emissionMaterial.Length; i++)
            {
                Material tempEmission = emissionMaterial[i].material;// change Emission color
                tempEmission.SetColor("_EmissionColor", correctColor);
                emissionMaterial[i].material = tempEmission;
            }
        }
    }
    private void WrongPassword()
    {
        if (audioSorce && wrongPasswordSound)
            audioSorce.PlayOneShot(wrongPasswordSound);

        onFalsePassword?.Invoke(); // call all our events for wrong password

        if (emissionMaterial.Length > 0)
        {
            for (int i = 0; i < emissionMaterial.Length; i++)
            {
                Material tempEmission = emissionMaterial[i].material;// change Emission color
                tempEmission.SetColor("_EmissionColor", wrongColor);
                emissionMaterial[i].material = tempEmission;
            }
        }
    }
    public void SaveLockState(string ID)
    {
        string lockedState = ID += "_unlocked";
        if (unlocked)
        {
            PlayerPrefs.SetString(lockedState, "true");
        }
    }
    public void GeneratePassword(string ID)
    {
        string newPassword = "";
        for (int i = 0; i < maxPasswordLetters; i++)
        {
            newPassword += Random.Range(0, 9);
        }
        password = newPassword;
        PlayerPrefs.SetString(ID, newPassword);
    }
    public void GetPassword(string ID)
    {
        id = ID;
        if (PlayerPrefs.HasKey(ID))
        {
            password = PlayerPrefs.GetString(ID);
            textGUI.text = password;
        }
        else
        {
            GeneratePassword(ID);
        }
        SetHintText();
        string lockedState = ID += "_unlocked";
        if (PlayerPrefs.HasKey(lockedState))
        {
            temporaryText = password;
            TryPassword();
            unlocked = true;
        }
        else
        {
            unlocked = false;
        }
    }
    private void SetHintText()
    {
        if (noteData)
        {
            NoteController.instance.GetNoteTextCodesText(noteData.noteID, password);
        }
        hintText.inGameText.text = password;
    }
}
