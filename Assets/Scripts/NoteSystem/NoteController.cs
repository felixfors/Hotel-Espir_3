using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NoteController : MonoBehaviour
{
    
    private bool pauseSate;
    private float triggerTimer = 0;
    private float triggerCoolDown = 0.5f; // så vi inte kan spamma tab hela tiden
    [Space(20)]
    public static NoteController instance;
    [Space(20)]
    public InputSprite journalCategoryButtons;
    private int categorySelection;
    public TextMeshProUGUI categoryTitleText;

    [Space(20)]
    public AudioSource audioSource;
    public AudioClip note_PickupSound;
    public AudioClip note_DiscardSound;

    public CanvasGroup canvasGroupButtons;
    public CanvasGroup canvasGroupPapper;

    [Space(20)]
    public GameObject buttonPrefab;
    public Transform noteHierarchy;
    

    public bool gotJournal;
    public bool journalOpen;
    public bool readingNote;
    //[HideInInspector]
    public bool readingNoteDelay; // används i inventory controller så vi inte slänger items när vi slutar läsa
    private bool cancelNoteDelay; // annars stänger vi noten direkt när vi läser den

    public RawImage bookCover;
    public List<Note> note = new();
    [System.Serializable]
    public class Note
    {
        public int id;
        public GameObject noteGraphic;
        public TextMeshProUGUI codeText;
        public NoteData noteData;
        public bool saved;
        public bool read;
        public NoteHierarchyButton noteHierarchyButton;
    }

    public List<Category> category = new();
    [System.Serializable]
    public class Category
    {
        public string categoryName;
        public List<GameObject> buttons = new();
        public List<NoteData> noteData = new();  

    }
    public event Action ClosedNoteEvent;
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
    }
    public void JournalOpenDelay()
    {
        if (!journalOpen)
            return;

        audioSource.PlayOneShot(note_PickupSound);

        canvasGroupPapper.alpha = 1;
        canvasGroupButtons.alpha = 1;
        journalCategoryButtons.IsActive(true);
        bookCover.enabled = true;
    }
    // Update is called once per frame
    void Update()
    {
        if(pauseSate != MenuController.instance.paused)
        {
            if(MenuController.instance.paused)
            {                
                pauseSate = true;

                if (readingNote || journalOpen)
                    CloseJournal(true);
            }
            else
            {
                pauseSate = false;
            }
        }
        if (MenuController.instance.paused) // Om spelet är pausat så ska inte några inputs funka
            return;
        triggerTimer += Time.deltaTime;
        if (PlayerController.instance.journalInput.action.WasPressedThisFrame() && gotJournal && !EnemyAttack.instance.jumpScareActive && triggerTimer >=triggerCoolDown)
        {
            journalOpen = !journalOpen;
            canvasGroupButtons.interactable = journalOpen;
            canvasGroupButtons.blocksRaycasts = journalOpen;

            if (!journalOpen) // om vi stänger så ska all hud försvinna på engång
            {
                if(InventoryController.instance.currentItemData)
                {
                    GameObject target = InventoryController.instance.inventory[InventoryController.instance.currentItem].physicalObject;
                    InventoryController.instance.StartCoroutine(InventoryController.instance.DelayedItemVisibleSwap(target, true,1));
                }
                CancelInvoke("JournalOpenDelay");
                canvasGroupPapper.alpha = 0;
                canvasGroupButtons.alpha = 0;
            }
            else // om vi stänger så ska all hud öppnas med en liten delay, för animationens skull
            {
                Invoke(nameof(JournalOpenDelay), 0.9f);
                if (InventoryController.instance.currentItemData)
                {
                    GameObject target = InventoryController.instance.inventory[InventoryController.instance.currentItem].physicalObject;
                    InventoryController.instance.StartCoroutine(InventoryController.instance.DelayedItemVisibleSwap(target, false,0.5f));
                }
            }

            if (journalOpen)
            {
                PlayerAnimator.instance.anim.ResetTrigger("Withdraw");
                PlayerAnimator.instance.PlayPlayerAnimation(1,false, "play");

               
                
                //PlayerController.instance.canMove = false;
                InventoryController.instance.canUseInventory = false;
                PlayerController.instance.MouseController(1);
                NextCategorySelector(0);
            }
            else
            {
                CloseJournal(false);
            }
            triggerTimer = 0;
        }
        if (journalOpen)
        {          
            if(PlayerController.instance.journalCategory.action.WasPerformedThisFrame() && PlayerController.instance.journalCategory.action.ReadValue<float>() < 0)
            {
                audioSource.PlayOneShot(note_DiscardSound);
                NextCategorySelector(-1);
                //journalHud.PressedCategoryButton(true);
            }
            else if (PlayerController.instance.journalCategory.action.WasPerformedThisFrame() && PlayerController.instance.journalCategory.action.ReadValue<float>() > 0)
            {
                audioSource.PlayOneShot(note_PickupSound);
                NextCategorySelector(1);
                //journalHud.PressedCategoryButton(false);
            }

            //bool moving = PlayerController.instance.moveInput.action.ReadValue<Vector2>().x != 0 || PlayerController.instance.moveInput.action.ReadValue<Vector2>().y != 0;

            if (PlayerController.instance.jumpInput.action.WasPressedThisFrame()) // om vi står stilla gör ingenting
                CloseJournal(false); // false för vi stänger utanför döden
        }
        else if(!journalOpen && readingNote && !cancelNoteDelay)
        {
            bool moving = PlayerController.instance.moveInput.action.ReadValue<Vector2>().x != 0 || PlayerController.instance.moveInput.action.ReadValue<Vector2>().y != 0;
            if (PlayerController.instance.cancelAction.action.WasPressedThisFrame() || moving)                
                CloseJournal(false); // false för vi stänger utanför döden
        }
    }
  

    public void NextCategorySelector(int nextSelection)
    {
        categorySelection += nextSelection;
        if (category.Count > 0)
        {
            if (categorySelection < -1)
                categorySelection = category.Count-1;
            else if (categorySelection > category.Count-1)
                categorySelection = -1;
        }
        else
            categorySelection = -1;

        if (categorySelection == -1)
            categoryTitleText.text = "All";
        else
            categoryTitleText.text = category[categorySelection].categoryName;


        foreach (Category _category in category)
        {
            foreach(GameObject _button in _category.buttons)
            {
                if(categorySelection == -1) //Vi ska visa alla kategorier
                {
                    _button.SetActive(true);
                    //return;
                }               
                else
                {
                    _button.SetActive(false);
                }                
            }
        }
        if(categorySelection >=0 && category.Count >0) // vi ska visa en särskild kategori
        {
            foreach (GameObject _button in category[categorySelection].buttons)
            {
                _button.SetActive(true);
            }
        }
    }
    private void CancelNoteDelay()
    {
        cancelNoteDelay = false;
    }
    private void ReadingNoteDelay()
    {
        readingNoteDelay = false;
    }
    public void DisplayNote(NoteData noteData)
    {
        foreach (Note _note in note)// stäng av alla andra notes
        {
            if (_note.noteGraphic.activeInHierarchy)
                _note.noteGraphic.SetActive(false);
        }

        for (int i = 0; i < note.Count; i++) // Hitta pappret som vi letar efter
        {
            if (note[i].id == noteData.noteID) // vi har hittat pappret
            {
                audioSource.PlayOneShot(note_PickupSound);
                readingNote = true;
                readingNoteDelay = true;
                cancelNoteDelay = true;
                Invoke("CancelNoteDelay",0.1f);

                //PlayerController.instance.canMove = false;
                PlayerController.instance.MouseController(1);
                InventoryController.instance.canUseInventory = false;
                canvasGroupPapper.alpha = 1;

                note[i].noteGraphic.SetActive(true);
                //journalOpen = true;

                if(journalOpen)
                {
                    bookCover.enabled = true;
                    note[i].noteGraphic.GetComponent<RawImage>().enabled = false;                                   
                }
                else
                {
                    bookCover.enabled = false;
                    note[i].noteGraphic.GetComponent<RawImage>().enabled = true;
                }

                if (!note[i].saved && gotJournal) // spara pappret
                {
                    note[i].saved = true;

                    GameObject _buttonPrefab = Instantiate(buttonPrefab, noteHierarchy);
                    note[i].noteHierarchyButton = _buttonPrefab.GetComponent<NoteHierarchyButton>();
                    note[i].noteHierarchyButton.name_Text.text = note[i].noteData.noteTitle;
                    note[i].noteHierarchyButton.new_Text.text = "NEW";

                    CreateCategy(note[i]);

                    _buttonPrefab.GetComponent<Button>().onClick.RemoveAllListeners();
                    _buttonPrefab.GetComponent<Button>().onClick.AddListener(() => DisplayNote(note[i].noteData));

                    Color itemColor = NotificationController.instance.colorPallet[0];
                    NotificationController.instance.NewNotification("<b><color=#" + ColorUtility.ToHtmlStringRGBA(itemColor) + ">" + noteData.noteTitle + "</color></b>" + " added to journal");
                }
                else if(note[i].saved && gotJournal && !note[i].read) // Om pappret redan är sparat och vi öppnar det så ska det markeras som "läst"
                {
                    note[i].noteHierarchyButton.new_Text.text = "";
                    note[i].read = true;
                }
                break;
            }
        }
    }
    public void CloseJournal(bool fromDeath)
    {
        
        StartCoroutine(WithdrawAnimation(fromDeath));
        if(journalOpen)
        {
           
            PlayerAnimator.instance.PlayPlayerAnimation(0, false, "Withdraw");
        }
        
        ClosedNoteEvent?.Invoke();
        
        

        journalCategoryButtons.IsActive(false);
        canvasGroupPapper.alpha = 0;
        canvasGroupButtons.alpha = 0;
        canvasGroupButtons.interactable = false;
        canvasGroupButtons.blocksRaycasts = false;
        bookCover.enabled = false;
        audioSource.PlayOneShot(note_DiscardSound);
        readingNote = false;
        Invoke("ReadingNoteDelay", 1f);
        journalOpen = false;
        foreach (Note _note in note)// stäng av alla notes
        {
            if (_note.noteGraphic.activeInHierarchy)
                _note.noteGraphic.SetActive(false);
        }

        
        
    }
    private IEnumerator WithdrawAnimation(bool _fromDeath) // gör så att world item syns igen efter vi kastar bort den
    {
        

        PlayerController.instance.MouseController(-1);

        InventoryController.instance.canUseInventory = true;

        PlayerAnimator.instance.PlayPlayerAnimation(0, false, "Withdraw");

        yield return new WaitForSeconds(1);        
        if (!_fromDeath)
        {
            PlayerAnimator.instance.anim.ResetTrigger("Withdraw");
            InventoryController.instance.SwapToItem(InventoryController.instance.currentItem);
            InventoryController.instance.canUseInventory = true;
            
        }

    }
    public void GetNoteTextCodesText(int id, string codeText)
    {
        for (int i = 0; i < note.Count; i++) // Hitta pappret som vi letar efter
        {
            if (note[i].id == id) // vi har hittat pappret
            {
                note[i].codeText.text = codeText;
            }
        }
    }

    public void GetAllNoteButtons()//Används för att lägga till alla knappar vid en sparning
    {       
        for(int i = 0; i < note.Count; i ++)
        {
            if(note[i].noteHierarchyButton == null && note[i].saved)
            {
                GameObject _buttonPrefab = Instantiate(buttonPrefab, noteHierarchy);
                note[i].noteHierarchyButton = _buttonPrefab.GetComponent<NoteHierarchyButton>();
                note[i].noteHierarchyButton.name_Text.text = note[i].noteData.noteTitle;
                note[i].noteHierarchyButton.new_Text.text = note[i].read ? "" : "NEW";
                
                CreateCategy(note[i]);
                             
                _buttonPrefab.GetComponent<Button>().onClick.RemoveAllListeners();
                NoteData _noteData = note[i].noteData;
                _buttonPrefab.GetComponent<Button>().onClick.AddListener(() => DisplayNote(_noteData));
            }                      
        }
    }

    public void CreateCategy(Note note)
    {
        if (category.Count == 0) // Ingen kategori finns än, skapa den första
        {
            Category newCategory = new Category();
            newCategory.categoryName = note.noteData.noteCategory.ToString();
            newCategory.buttons.Add(note.noteHierarchyButton.gameObject);
            newCategory.noteData.Add(note.noteData);
            category.Add(newCategory);
        }
        else // Vi har redan kategorier
        {
            for (int i = 0; i < category.Count; i++)
            {
                Debug.Log(category[i].categoryName); 
                if (category[i].categoryName == note.noteData.noteCategory.ToString())// vi har redan denna kategori, lägg påöka befintlig kategori information
                {
                    category[i].buttons.Add(note.noteHierarchyButton.gameObject);
                    category[i].noteData.Add(note.noteData);
                    break;
                }                   
                else if(category[i].categoryName != note.noteData.noteCategory.ToString())// Vi har inte denna categori, skapa en ny
                {
                    if (i == category.Count-1)
                    {
                        Category newCategory = new Category();
                        newCategory.categoryName = note.noteData.noteCategory.ToString();
                        newCategory.buttons.Add(note.noteHierarchyButton.gameObject);
                        newCategory.noteData.Add(note.noteData);
                        category.Add(newCategory);
                        break;
                    }                  
                }

            }
            
        }
    }

}
