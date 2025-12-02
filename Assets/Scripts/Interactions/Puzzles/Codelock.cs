using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
public class Codelock : MonoBehaviour
{
    public bool unlocked;
    private string id; // used for storing data/saved files
    //[HideInInspector]
    private bool scrollReset = true;
    private bool scrollHolding;
    private float scrollSpeed;

    public bool interacting;
    public NoteData noteData;
    public NewTextFeed hintText;

    private AudioSource audioSource;
    public AudioClip [] soundClick;
    public AudioClip soundOpen;

    public UnityEvent correctPasswordEvent;
    private int currentNumber; // what number is the wheel currently on
    private int previousNumber;
    private bool streak; // only true if the current combination is correct

    [Header("How many digits does the wheel have")]
    public int NumberAmount;
    [Header("Rotating wheel target")]
    public Transform lockCylinder;
    [Header("Text to display current digit")]
    public TextMeshProUGUI digitText;

    [Space(20)]
    public List<Password> password = new();
    [System.Serializable]
    public class Password
    {
        public int digit;
        public bool correct;
    }
    private float angle;
    private int lastDirection; // -1 left 1 right, what direction are we rotating towards
    private float currentRotation; // the transform rotation
    private float rotationAmount; // how many degrees do we rotate for each number

    //[HideInInspector]
    private List<Digit> digitData = new();

    [System.Serializable]
    public class Digit
    {
        public int representingNumber;
        public float rotationAngle;
    }

    // Start is called before the first frame update
    void Start()
    {
        if(gameObject.GetComponent<AudioSource>())
            audioSource = GetComponent<AudioSource>();
        rotationAmount = 360f / NumberAmount;
        AssignDigits();
        scrollReset = true;
    }
    private void Update()
    {
        if (digitText.text != currentNumber.ToString())
            digitText.text = currentNumber.ToString();
        if (!unlocked && interacting && !MenuController.instance.paused)
        {
            float input = PlayerController.instance.scrollWheel.action.ReadValue<float>();
            scrollHolding = PlayerController.instance.scrollWheel.action.ReadValue<float>() != 0;

            if(DetectController.instance.inputType == DetectController.InputType.gamepad)
            {
                if (scrollReset)
                {
                    if (input > 0.5f)
                    {
                        Scrolling(1);
                        StartCoroutine(ScrollCooldown());
                    }
                    else if (input < -0.5f)
                    {
                        Scrolling(-1);
                        StartCoroutine(ScrollCooldown());
                    }
                }
                if (scrollHolding)
                {
                    scrollSpeed = Mathf.MoveTowards(scrollSpeed, 0.01f, 2 * Time.deltaTime);
                }
                scrollSpeed = 0.2f;
            }
            
            else
            {
                if (input == 0)
                    return;
                if (input > 0.5f)
                {
                    Scrolling(1);
                }
                else if (input < -0.5f)
                {
                    Scrolling(-1);
                }
            }          
        }
       
            
    }
    private IEnumerator ScrollCooldown()
    {
        scrollReset = false;
        yield return new WaitForSeconds(scrollSpeed);
        scrollReset = true;
    }
    private void Scrolling(float direction)
    {

        lockCylinder.eulerAngles += new Vector3(0, 0, +rotationAmount*direction);
        currentRotation = 360-lockCylinder.localEulerAngles.z;
        currentRotation = Mathf.Round(currentRotation * 10.0f) * 0.1f;

        GetCurrentNumber((int)direction);
    }
    private void GetCurrentNumber(int direction)
    {

        for(int i = 0; i <NumberAmount; i ++)
        {
            float currentDigit = Mathf.Round(digitData[i].rotationAngle * 10.0f) * 0.1f;
            if (currentRotation == 360)
                currentRotation = 0;

            if (currentRotation - currentDigit == 0)
            {                
                currentNumber = digitData[i].representingNumber;
                if(audioSource)
                {
                    audioSource.pitch = Random.Range(0.9f,1f);
                    string _currentNumberString = currentNumber.ToString();
                    if (_currentNumberString.Contains("5") || _currentNumberString.Contains("0"))
                    {
                        previousNumber = 0;
                        audioSource.PlayOneShot(soundClick[1]);
                    }
                    else
                    {
                        audioSource.PlayOneShot(soundClick[0]);
                    }
                }             
            }
        }
        for(int i = 0; i < password.Count; i ++)
        {
            if(!password[i].correct)
            {
                if (password[i].digit == currentNumber)
                {    
                    if (direction != lastDirection)
                    {
                        streak = true;
                        password[i].correct = true;
                        if (i == password.Count-1)
                            CorrectPassword();

                        lastDirection = direction;
                        break;
                    }
                    else
                    {
                        ResetPassword();
                        break;
                    }                                                            
                }
                else
                {
                    if(streak && direction == lastDirection)
                    {
                        ResetPassword();
                        streak = false;
                    }
                    break;
                }
            }
        }
    }
    private void OpenSound()
    {
        audioSource.pitch = 1;
        audioSource.PlayOneShot(soundOpen);
    }
    private void CorrectPassword()
    {
        if(audioSource)
        {
            Invoke("OpenSound",0.5f);
        }
        
        correctPasswordEvent.Invoke();
        EndInteract();
        unlocked = true;       
    }
    private void ResetPassword()
    {
        for (int i = 0; i < password.Count; i++)
        {
            password[i].correct = false;
        }
    }
    private void AssignDigits()
    {       
        for(int i = 0; i < NumberAmount; i ++)
        {
            Digit temp = new();            
            temp.representingNumber = i;
            temp.rotationAngle = i * rotationAmount;
            digitData.Add(temp);
        }
    }
    public void StartInteract()
    {
        interacting = true;
    }
    public void EndInteract()
    {
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
        for(int i = 0; i <password.Count; i ++)
        {
            int _NewDigit = Random.Range(0, NumberAmount);
            newPassword +=_NewDigit+"_";
        }
        SetPassword(newPassword);
        PlayerPrefs.SetString(ID, newPassword);
    }
    private void SetPassword(string _password)
    {
        string[] _splitPassword = _password.Split(char.Parse("_"));
        for(int i = 0; i < password.Count; i ++)
        {
            password[i].digit = int.Parse(_splitPassword[i]);
        }
    }
    public void GetPassword(string ID)
    {
        id = ID;
        if (PlayerPrefs.HasKey(ID))
        {
            SetPassword(PlayerPrefs.GetString(ID));           
        }
        else
        {
            GeneratePassword(ID);
        }
        SetHintText();
        string lockedState = ID += "_unlocked";
        if(PlayerPrefs.HasKey(lockedState))
        {
            CorrectPassword(); 
        }
        else
        {
            unlocked = false;
        }
    }
    private void SetHintText()
    {
        string _hintText = "";
        for (int i = 0; i < password.Count; i++)
        {
            _hintText += password[i].digit + " ";
        }

        if (noteData)
        {
            NoteController.instance.GetNoteTextCodesText(noteData.noteID, _hintText);
        }
        if (!hintText)
            return;
        
        hintText.inGameText.text = _hintText;
    }
}
