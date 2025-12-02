using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SaveController : MonoBehaviour
{
    public static SaveController instance;
    public Transform spawnPoint;
    public Transform player;
    private void Awake()
    {
        instance = this;
    }
    //All combination locks in the game and their states
    public List<PasswordData> passwordData = new();
    [System.Serializable]
    public class PasswordData
    {
        public string passwordID;
        public GameObject passwordObjects;

    }

    public List<DoorData> doorsData = new();
    [System.Serializable]
    public class DoorData
    {
        public string doorID;
        public Doors doorObjects;
        public Drawers drawerObject;
    }
    public List<InventoryData> inventoryData = new();
    [System.Serializable]
    public class InventoryData
    {
        public int slotID;
        public Item itemObjects;
        public bool active;
    }

    public List<ItemData> itemData = new();
    [System.Serializable]
    public class ItemData
    {
        public string itemID;
        public Item itemObjects;
    }

    public List<NoteItem> noteItem = new();



    public List<EventsData> eventData = new();
    [System.Serializable]
    public class EventsData
    {
        public string eventID;
        public EventData eventObject;
    }

    public List<GameObject> physicObject = new();

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
        LoadSave();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) // Save game
        {
            SaveGame();
        }
        if (Input.GetKeyDown(KeyCode.G)) // Load Save
        {
            LoadSave();   
        }
    }
    public void DeleteSave()
    {
     
        for (int i = 0; i < passwordData.Count; i++)
        {
            PlayerPrefs.DeleteKey(passwordData[i].passwordID + "_unlocked");
            PlayerPrefs.DeleteKey(passwordData[i].passwordID);
        }
        PlayerPrefs.DeleteKey("DoorData");
        PlayerPrefs.DeleteKey("PlayerTransform");
        PlayerPrefs.DeleteKey("GhostTransform");
        PlayerPrefs.DeleteKey("ItemData");
        PlayerPrefs.DeleteKey("PlayerStats");
        PlayerPrefs.DeleteKey("EventsData");
        PlayerPrefs.DeleteKey("NoteData");
        PlayerPrefs.DeleteKey("JournalData");
        PlayerPrefs.DeleteKey("PhysicObjectData");
    }
    public void LoadSave()
    {       
        GetDoorsData(); // get and set Doors data or generate new
        GetPasswords(); // get and set Password data or generate new
        GetPlayerTransform(); // Get Player Transform
        GetGhostTransform(); // Get Ghost transform + state
        GetPlayerStats();
        GetItemsData();      
        GetEventsData();
        GetJournalData();
        GetPhysicObject();
        if (PlayerPrefs.HasKey("PlayerTransform"))
        NotificationController.instance.NewNotification("Save loaded");
    }
    public void SaveGame()
    {
        NotificationController.instance.NewNotification("Game Saved");

        for (int i = 0; i < passwordData.Count; i++)
        {
            passwordData[i].passwordObjects.SendMessage("SaveLockState", passwordData[i].passwordID);
        }
        SaveDoorData();
        SavePlayerTransform();
        SaveGhostTransform();
        SaveItems();
        SavePlayerStats();
        SaveEvents();
        SavejournalData();
        SavePhysicObject();
    }
    [ContextMenu("GetAllPasswordTargets")] // Måste köras om vid varje nytt tillägg
    public void GetPasswordTargets()
    {
        passwordData.Clear();
        GameObject[] allPasswords = GameObject.FindGameObjectsWithTag("Password");
        for (int i = 0; i < allPasswords.Length; i++)
        {
            PasswordData newPassword = new PasswordData();
            newPassword.passwordID = "Password_" + i;
            newPassword.passwordObjects = allPasswords[i].gameObject;
            passwordData.Add(newPassword);
        }
    }
    public void SavejournalData()
    {
        string _noteString = "";
        for(int i = 0; i < noteItem.Count; i ++)
        {
            _noteString += (noteItem[i].pickedUp ? "true" : "false");
            if (i != noteItem.Count)
                _noteString += "&";
        }
        PlayerPrefs.SetString("NoteData", _noteString);

        string _journalDataString = "";
        for(int i = 0; i < NoteController.instance.note.Count; i ++)
        {
            _journalDataString += (NoteController.instance.note[i].saved ? "true" : "false") + ","; // 1
            _journalDataString += (NoteController.instance.note[i].read ? "true" : "false");        // 2
            if (i < NoteController.instance.note.Count-1)
                _journalDataString += "&";
        }
        PlayerPrefs.SetString("JournalData", _journalDataString);
    }
    public void GetJournalData()
    {
        if (PlayerPrefs.HasKey("NoteData"))
        {
            string _noteData = PlayerPrefs.GetString("NoteData");
            string[] _noteDataSplit = _noteData.Split(char.Parse("&"));
            for(int i = 0; i < noteItem.Count; i ++)
            {
                bool _pickedUp = (_noteDataSplit[i] == "true" ? true : false);
                if(_pickedUp)
                noteItem[i].PickedUp();
            }
        }

        if (PlayerPrefs.HasKey("JournalData"))
        {
            string _journalData = PlayerPrefs.GetString("JournalData");
            string[] _journalDataSplit = _journalData.Split(char.Parse("&"));
            for (int i = 0; i < NoteController.instance.note.Count; i++)
            {
                string[] _thisData = _journalDataSplit[i].Split(",");
                
                bool _saved = (_thisData[0] == "true" ? true : false);
                NoteController.instance.note[i].saved = _saved;

                bool _read = (_thisData[1] == "true" ? true : false);
                NoteController.instance.note[i].read = _read;
            }
                
                NoteController.instance.GetAllNoteButtons();
        }
    }
    public void GetPasswords() // Om passwords inte finns så genereras nya automatiskts för enskild enhet
    {
        for (int i = 0; i < passwordData.Count; i++)
        {
            //Debug.Log(i);
            passwordData[i].passwordObjects.SendMessage("GetPassword", passwordData[i].passwordID);
        }
    }
    
    [ContextMenu("GetAllInteractible")] // Måste köras om vid varje nytt tillägg
    private void GetAllInteractible()
    {
        doorsData.Clear();
        itemData.Clear();
        noteItem.Clear();
        physicObject.Clear();

        GameObject[] allInteractible = GameObject.FindGameObjectsWithTag("Interactable");
        for(int i = 0; i < allInteractible.Length; i ++)
        {           
            if (allInteractible[i].GetComponent<Doors>() || allInteractible[i].GetComponent<Drawers>()) // check if it is a door or an drawer
            {
                DoorData newDoor = new DoorData();
                newDoor.doorID = "DoorID_" + i;
                if (allInteractible[i].GetComponent<Doors>())
                    newDoor.doorObjects = allInteractible[i].GetComponent<Doors>();
                else if (allInteractible[i].GetComponent<Drawers>())
                    newDoor.drawerObject = allInteractible[i].GetComponent<Drawers>();

                doorsData.Add(newDoor);
            }
            if(allInteractible[i].GetComponent<Item>())
            {
                ItemData newItem = new ItemData();
                newItem.itemID = "ItemID_" + i;
                newItem.itemObjects = allInteractible[i].GetComponent<Item>();
                itemData.Add(newItem);
            }
            if(allInteractible[i].GetComponent<NoteItem>())
            {
                NoteItem newNote = allInteractible[i].GetComponent<NoteItem>();
                noteItem.Add(newNote);
            }
            if(allInteractible[i].GetComponent<Rigidbody>() && allInteractible[i].GetComponent<Item>() == null)
            {
                physicObject.Add(allInteractible[i]);
            }
        }
    }
    [ContextMenu("GetAllEvents")] // Måste köras om vid varje nytt tillägg
    private void GetAllEvents()
    {
        eventData.Clear();
        GameObject[] allEvents = GameObject.FindGameObjectsWithTag("Event");
        for(int i = 0; i <allEvents.Length; i ++)
        {
            EventsData _newEvent = new EventsData();
            _newEvent.eventID = "EventID_" + i.ToString();
            _newEvent.eventObject = allEvents[i].gameObject.GetComponent<EventData>();
            eventData.Add(_newEvent);
        }
    }
    public void SaveDoorData()
    {
        string _doorData = "";
        for(int i = 0; i < doorsData.Count; i ++)
        {
            if(doorsData[i].doorObjects)
            {
                _doorData += doorsData[i].doorObjects.rotation + ",";
                _doorData += doorsData[i].doorObjects.locked.ToString() + "&";
            }
            else if(doorsData[i].drawerObject)
            {
                _doorData += doorsData[i].drawerObject.drawerValue + ",";
                _doorData += doorsData[i].drawerObject.locked.ToString() + "&";
            }           
        }
        PlayerPrefs.SetString("DoorData", _doorData);
    }
    public void GetDoorsData()
    {

        if(PlayerPrefs.HasKey("DoorData"))
        {
            string _doorData = PlayerPrefs.GetString("DoorData");
            string[] _doorDataSplit = _doorData.Split(char.Parse("&")); // (0,true &) 0,true & 0,true etc
            //Debug.Log();
            for(int i = 0; i <doorsData.Count; i ++)
            {
                string[] thisDoorData = _doorDataSplit[i].Split(char.Parse(",")); //(0) (true)
                if (doorsData[i].doorObjects)
                    doorsData[i].doorObjects.rotation = float.Parse(thisDoorData[0]);
                else if (doorsData[i].drawerObject)
                    doorsData[i].drawerObject.SetSavedValue(float.Parse(thisDoorData[0]));

                bool _locked = (thisDoorData[1] == "True" ? true : false);
                if (doorsData[i].doorObjects)
                    doorsData[i].doorObjects.locked = _locked;
                else if (doorsData[i].drawerObject)
                    doorsData[i].drawerObject.locked = _locked;
            }
        }
    }


    //Player Data
    public void SavePlayerTransform()
    {
        string _playerTransform = "";
        _playerTransform += PlayerController.instance.transform.position.ToString();        
        _playerTransform += ","+PlayerController.instance.transform.eulerAngles.y.ToString();
        _playerTransform = _playerTransform.Replace("(", ""); _playerTransform = _playerTransform.Replace(")", "");
        PlayerPrefs.SetString("PlayerTransform",_playerTransform);       
    }
    public void GetPlayerTransform()
    {
        if(PlayerPrefs.HasKey("PlayerTransform"))
        {
            string _playerTransform = PlayerPrefs.GetString("PlayerTransform");
            string[] _playerTransformSplit = _playerTransform.Split(char.Parse(","));
          
            Vector3 savedPosition = new Vector3(float.Parse(_playerTransformSplit[0]), float.Parse(_playerTransformSplit[1]), float.Parse(_playerTransformSplit[2]));
            Vector3 savedRotation = new Vector3(0, float.Parse(_playerTransformSplit[3]), 0);
            PlayerController.instance.transform.position = savedPosition;
            PlayerController.instance.transform.eulerAngles = savedRotation;

        }
    }
    public void SavePlayerStats()
    {
        string _playerStats = "";
        _playerStats += PlayerController.instance.health.ToString(); // 0 = hälsa
        _playerStats += "," + InventoryController.instance.banishmentBoxDiscs.ToString(); // 1 = skivor i inventory
       
        bool gotFlashlight = InventoryController.instance.flashLight.isInInventory;
        _playerStats += gotFlashlight? ",true" : ",false"; // 2 = ficklampa i inventory
        
        bool gotJournal = NoteController.instance.gotJournal;
        _playerStats += gotJournal ? ",true" : ",false"; // 3 = journal i inventory

        

        _playerStats += "," + InventoryController.instance.currentItem.ToString(); // 4 = senaste item som hölls i

        bool isFlashLightOn = InventoryController.instance.flashLight.on;
        _playerStats += isFlashLightOn ? ",true" : ",false"; // 5 = är flashlight på när vi sparade
        PlayerPrefs.SetString("PlayerStats", _playerStats);
    }
    public void GetPlayerStats()
    {
        if (PlayerPrefs.HasKey("PlayerStats"))
        {
            string _PlayerStats = PlayerPrefs.GetString("PlayerStats");
            string[] _PlayerStatsSplit = _PlayerStats.Split(char.Parse(","));
            PlayerController.instance.health = int.Parse(_PlayerStatsSplit[0]);
            InventoryController.instance.banishmentBoxDiscs = int.Parse(_PlayerStatsSplit[1]);
            
            bool gotFlashlight = (_PlayerStatsSplit[2] == "true");
            if (gotFlashlight)
            {
               
                bool flashlightOn = (_PlayerStatsSplit[5] == "true");
                InventoryController.instance.flashLight.GetFlashLightData(flashlightOn);
            }
                
            
            bool gotJournal = (_PlayerStatsSplit[3] == "true");
            if (gotJournal)
                NoteController.instance.gotJournal = true;

        }
        else
        {
            PlayerController.instance.health = 1;
            InventoryController.instance.banishmentBoxDiscs = 0;
        }
    }
    public void SaveGhostTransform()
    {
        string _GhostTransform = "";
        _GhostTransform += EnemyController.instance.transform.position.ToString(); // save ghost position
        _GhostTransform += "," + EnemyController.instance.transform.eulerAngles.y.ToString();// save ghost rotation
        _GhostTransform += "," + EnemyController.instance.target.position.ToString(); // save ghost target position
        _GhostTransform += "," + (int)EnemyController.instance.enemyState; // save ghost state
        
        bool _freezePosition = EnemyController.instance.freezeGhost; //  Kolla om spöket är fryst
        _GhostTransform += _freezePosition ? ",true" : ",false"; // spara det frysta läget
        
        _GhostTransform = _GhostTransform.Replace("(", ""); _GhostTransform = _GhostTransform.Replace(")", "");
        PlayerPrefs.SetString("GhostTransform", _GhostTransform);
    }
    public void GetGhostTransform()
    {
        if (PlayerPrefs.HasKey("GhostTransform"))
        {
            string _GhostTransform = PlayerPrefs.GetString("GhostTransform");
            string[] _GhostTransformSplit = _GhostTransform.Split(char.Parse(","));

            Vector3 savedPosition = new Vector3(float.Parse(_GhostTransformSplit[0]), float.Parse(_GhostTransformSplit[1]), float.Parse(_GhostTransformSplit[2]));
            Vector3 savedRotation = new Vector3(0, float.Parse(_GhostTransformSplit[3]), 0);
            Vector3 savedTargetPosition = new Vector3(float.Parse(_GhostTransformSplit[4]), float.Parse(_GhostTransformSplit[5]), float.Parse(_GhostTransformSplit[6]));
            
            EnemyController.instance.agent.Warp(savedPosition);
            EnemyController.instance.transform.eulerAngles = savedRotation;
            EnemyController.instance.target.position = savedTargetPosition;
            EnemyController.States _states = (EnemyController.States)int.Parse(_GhostTransformSplit[7]);
            bool _freeze = (_GhostTransformSplit[8] == "true");
            //Debug.Log("Freeeze läget ska vara " + _freeze);
            EnemyController.instance.freezeGhost = _freeze;
            EnemyController.instance.SwitchState(_states);
        }
    }

    private void GetItemsData()
    {
 
        if (PlayerPrefs.HasKey("ItemData"))
        {
            int lastHeldItem = InventoryController.instance.currentItem;
            if (InventoryController.instance.currentItem != 0 && InventoryController.instance.inventory[lastHeldItem].item.itemData.usable)// Om vi intergrerar med itemet så vill vi sluta intergrera med det när vi återställer
                InventoryController.instance.inventory[lastHeldItem].physicalObject.SendMessage("UnUseItem");
 

            inventoryData.Clear();
            if (InventoryController.instance.inventory.Count > 0)
                InventoryController.instance.inventory.RemoveRange(1, InventoryController.instance.inventory.Count - 1); // clear the current inventory preventing duping


            string _itemData = PlayerPrefs.GetString("ItemData");
            string[] _itemDataSplit = _itemData.Split(char.Parse("&"));

            //InteractManager.instance.currentPriority =  Detta måste ändras till neutralt



            InventoryController.instance.currentItem = 0;
            InventoryController.instance.currentItemData = null;
            PlayerAnimator.instance.PlayPlayerAnimation(0,false,"Idle"); // återställ animation stadiet till neutralt

            for (int i = 0; i <itemData.Count; i ++)
            {

                
                    string[] _thisData = _itemDataSplit[i].Split(",");
                    Vector3 _newPosition = new Vector3(float.Parse(_thisData[0]), float.Parse(_thisData[1]), float.Parse(_thisData[2]));
                    Vector3 _newRotation = new Vector3(float.Parse(_thisData[3]), float.Parse(_thisData[4]), float.Parse(_thisData[5]));
                    itemData[i].itemObjects.transform.position = _newPosition;
                    itemData[i].itemObjects.transform.eulerAngles = _newRotation;
                    bool _active = (_thisData[6] == "true" ? true : false);
                    itemData[i].itemObjects.gameObject.SetActive(_active);
                    bool _inInventory = (_thisData[7] == "true" ? true : false);
                    if (_inInventory)
                    {
                        //InventoryController.instance.AddNewItem(itemData[i].itemObjects);
                        InventoryData _newInventoryData = new InventoryData(); // add all Inventory data to a temp inventory to sort it
                        _newInventoryData.itemObjects = itemData[i].itemObjects;
                        _newInventoryData.slotID = int.Parse(_thisData[8]);
                        _newInventoryData.active = _active;
                        inventoryData.Add(_newInventoryData);
                    }
                    else
                    {
                        itemData[i].itemObjects.transform.parent = InventoryController.instance.worldItemParent;
                        if (itemData[i].itemObjects.meshObject.layer == 17) // Detta betyder att vi hade detta item i handen när vi ska ladda en save där itemet inte ligger i handen
                        {

                            itemData[i].itemObjects.meshObject.layer = 9; // Item Layer
                            itemData[i].itemObjects.gameObject.layer = 9;
                            itemData[i].itemObjects.transform.localScale *= 2f;
                    }
                        

                }
            }
            if(inventoryData.Count >0)
            {
                inventoryData.Sort(SortInventory); // Sort the temp data
                int _primaryItem = 0;
                for(int i = 0; i < inventoryData.Count; i ++)
                {
                    if (inventoryData[i].active)
                        _primaryItem = inventoryData[i].slotID;
                    InventoryController.instance.AddNewItem(inventoryData[i].itemObjects); // add all Items in a sorted order into the real Inventory
                }
                string _PlayerStats = PlayerPrefs.GetString("PlayerStats");
                string[] _PlayerStatsSplit = _PlayerStats.Split(char.Parse(","));
                int _activeItem = int.Parse(_PlayerStatsSplit[4]);

                InventoryController.instance.SwapToItem(_activeItem); // swap to our saved primary
                NotificationController.instance.ClearNotifications(); // Remove all the items added notifications
            }           
        }
    }
    private int SortInventory(InventoryData a, InventoryData b)
    {
        if (a.slotID < b.slotID)
            return -1;
        else if (a.slotID > b.slotID)
            return 1;
        return 0;
    }
    public void SaveItems()
    {
        string itemDataString = "";
        for (int i = 0; i < itemData.Count; i++)
        {
            itemDataString += itemData[i].itemObjects.transform.position.ToString()+","; //0,1,2 - position
            itemDataString = itemDataString.Replace("(", ""); itemDataString = itemDataString.Replace(")", "");
            itemDataString += itemData[i].itemObjects.transform.eulerAngles.x.ToString()+","; // 3 - rotation x
            itemDataString += itemData[i].itemObjects.transform.eulerAngles.y.ToString() +","; // 4 - rotation y
            itemDataString += itemData[i].itemObjects.transform.eulerAngles.z.ToString() +","; // 5 - rotation z
            itemDataString += (itemData[i].itemObjects.gameObject.activeInHierarchy ? "true" : "false") + ","; // 6 - active or not
            bool inInventory = itemData[i].itemObjects.transform.parent == InventoryController.instance.inventoryParent;
            itemDataString += (inInventory ? "true" : "false") + ","; // 7 - in inventory or not
            itemDataString += (inInventory ? itemData[i].itemObjects.transform.GetSiblingIndex().ToString() : "0") + ","; // 8 - Inventory Slot
            if (i != itemData.Count)
                itemDataString += "&";           
        }
        PlayerPrefs.SetString("ItemData", itemDataString);
    }

    public void SaveEvents()
    {
        string eventDataString = "";
        for(int i = 0; i <eventData.Count; i ++)
        {
            eventDataString += (eventData[i].eventObject.loop ? "true" : "false") +":"; // 0 kan eventet loopas
            eventDataString += (eventData[i].eventObject.triggered ? "true" : "false") + ":"; // 1 har eventet triggats
            eventDataString += eventData[i].eventObject.triggeredCount + ":"; // 2 hur många gånger har eventet triggats
            eventDataString += eventData[i].eventObject.floatValue; // 3 float värde som sparas
            if (i != eventData.Count - 1)
                eventDataString += "&";
        }
        Debug.Log(eventDataString + " Detta sparade vi");
        PlayerPrefs.SetString("EventsData", eventDataString);
    }
    public void GetEventsData()
    {
        if (PlayerPrefs.HasKey("EventsData"))
        {
            string _eventsData = PlayerPrefs.GetString("EventsData");
            string[] _eventsDataSplit = _eventsData.Split(char.Parse("&")); // splittar up alla events
            for (int i = 0; i < eventData.Count; i++)
            {

                string[] _thisData = _eventsDataSplit[i].Split(":"); // splittar upp varje variable för enskilt event
                
                bool _loop = (_thisData[0] == "true" ? true : false);
                eventData[i].eventObject.loop = _loop;
                
                bool _triggered = (_thisData[1] == "true" ? true : false);
                eventData[i].eventObject.triggered = _triggered;

                eventData[i].eventObject.triggeredCount = int.Parse(_thisData[2]);

                eventData[i].eventObject.floatValue = float.Parse(_thisData[3]);
            }
        }
    }
    public void SavePhysicObject()
    {
        string objectDataString = "";
        for (int i = 0; i < physicObject.Count; i++)
        {
            objectDataString += physicObject[i].transform.position.ToString() + ","; //0,1,2 - position
            objectDataString = objectDataString.Replace("(", ""); objectDataString = objectDataString.Replace(")", "");
            objectDataString += physicObject[i].transform.eulerAngles.x.ToString() + ","; // 3 - rotation x
            objectDataString += physicObject[i].transform.eulerAngles.y.ToString() + ","; // 4 - rotation y
            objectDataString += physicObject[i].transform.eulerAngles.z.ToString() + ","; // 5 - rotation z
            objectDataString += (physicObject[i].activeInHierarchy ? "true" : "false"); // 6 - active or not
            if (i != physicObject.Count)
                objectDataString += "&";
        }
        PlayerPrefs.SetString("PhysicObjectData", objectDataString);
    }

    public void GetPhysicObject()
    {

        if (PlayerPrefs.HasKey("PhysicObjectData"))
        {           
            string _physicObjectData = PlayerPrefs.GetString("PhysicObjectData");
            string[] _physicObjectSplit = _physicObjectData.Split(char.Parse("&"));
            Debug.Log(_physicObjectData);
            for (int i = 0; i < physicObject.Count; i++)
            {
                Rigidbody thisRB = physicObject[i].GetComponent<Rigidbody>();

                string[] _thisData = _physicObjectSplit[i].Split(",");
                Debug.Log(_thisData);
                Vector3 _newPosition = new Vector3(float.Parse(_thisData[0]), float.Parse(_thisData[1]), float.Parse(_thisData[2]));
                Vector3 _newRotation = new Vector3(float.Parse(_thisData[3]), float.Parse(_thisData[4]), float.Parse(_thisData[5]));
                thisRB.position = _newPosition;
                thisRB.rotation = Quaternion.Euler(_newRotation);
                Debug.Log(physicObject[i].transform.position);
                bool _active = (_thisData[6] == "true" ? true : false);
                physicObject[i].SetActive(_active);           
            }        
        }
    }
}
