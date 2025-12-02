using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Suitcase : MonoBehaviour
{

    private string id; // used for storing data/saved files
    public NoteData noteData;
    private bool unlocked;
    [HideInInspector]
    public bool interacting;
    public NewTextFeed hintText; //används för att visa lösenordet på object vid mouse hover
    public GameObject buttonsHUD;
    public string password;
    public UnityEvent correctPasswordEvent;

    public List<Wheels> wheel = new();
    [System.Serializable]
    public class Wheels
    {
        public int currentWheelNumber;
        public Transform physicalObject;
        public int maxCombinations;
        public float degreePerCombination;
        public float currentRotation;
    }

    private AudioSource audioSource;
    public AudioClip[] tumblerSound;
    public AudioClip unlockSound;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        GetSetDegreeValue();
        buttonsHUD.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void GetSetDegreeValue()
    {
        for(int i = 0; i < wheel.Count; i ++)
        {
            wheel[i].degreePerCombination = 360 / wheel[i].maxCombinations;
        }
    }

    public void ScrollUp(int wheelTarget)
    {
        audioSource.PlayOneShot(tumblerSound[Random.Range(0,tumblerSound.Length)]);
        var target = wheel[wheelTarget];
        Vector3 newAngle = new Vector3(target.degreePerCombination, 0, 0);
        target.physicalObject.Rotate(newAngle);
        target.currentWheelNumber--;
        if (target.currentWheelNumber < 0)
            target.currentWheelNumber = target.maxCombinations - 1;

        string targetPassword = password[wheelTarget].ToString();

        if (target.currentWheelNumber.ToString() == targetPassword) // prova koden om vi har rätt siffra på nuvarande hjul
            TryCombination();
    }
    public void ScrollDown(int wheelTarget)
    {
        audioSource.PlayOneShot(tumblerSound[Random.Range(0, tumblerSound.Length)]);
        var target = wheel[wheelTarget];
        Vector3 newAngle = new Vector3(-target.degreePerCombination, 0, 0);
        target.physicalObject.Rotate(newAngle);
        target.currentWheelNumber++;
        if (target.currentWheelNumber > target.maxCombinations - 1)
            target.currentWheelNumber = 0;

        string targetPassword = password[wheelTarget].ToString();

        if(target.currentWheelNumber.ToString() == targetPassword) // prova koden om vi har rätt siffra på nuvarande hjul
            TryCombination();
    }
    private void TryCombination()
    {
        string tempPassword = "";
        for(int i = 0; i < wheel.Count; i ++)
        {
            string currentCombination = wheel[i].currentWheelNumber.ToString();
            tempPassword += currentCombination;
        }
        if(tempPassword == password)
        {
            correctPasswordEvent.Invoke();
            audioSource.PlayOneShot(unlockSound);            
            EndInteract();
            unlocked = true;
        }
    }
    public void StartInteract()
    {
        PlayerController.instance.MouseController(1);
        buttonsHUD.SetActive(true);
        interacting = true;
    }
    public void EndInteract()
    {
        PlayerController.instance.MouseController(-1);
        buttonsHUD.SetActive(false);
        interacting = false;
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
        for(int i = 0; i <wheel.Count; i ++)
        {
            newPassword += Random.Range(0,9);
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
        }            
        else
        {
            GeneratePassword(ID);
        }
        SetHintText();
        string lockedState = ID += "_unlocked";
        if (PlayerPrefs.HasKey(lockedState))
        {
            correctPasswordEvent.Invoke();
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
        if (!hintText)
            return;

        hintText.inGameText.text = password;
    }
}
