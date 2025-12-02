using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class InventoryController : MonoBehaviour
{
    
    public static InventoryController instance;
    public InventorySway inventorySway;
    public Transform inventoryHolder;
    public Transform handBone;
    [HideInInspector]
    public bool canUseInventory; // används vid paus, när man är död etc
    public bool usingItem; // Gör så vi inte loopar användning av ett item
    public Flashlights flashLight;
    public int currentItem;
    public int inventorySize;

    public Transform inventoryParent;
    public Transform worldItemParent;

    public AudioSource inventoryAudio;
    public AudioClip[] swapItemSound;
    
    //Throw Variables;
    public float throwStrength;
    private bool chargingThrow;

    private bool scrollReset;
    private float scrollTimer = 0;
    private float scrollCoolDown = 1;
    [Space(50)]
    [Header("Items in inventory")]
    public List<Inventory> inventory = new();
    [System.Serializable]
    public class Inventory
    {
        public string itemName;
        public Item item;
        public GameObject physicalObject;
    }
    public Item currentItemData;

    public int max_BanishmentBoxDiscs;
    public int banishmentBoxDiscs;

    public Animator itemAnim;
    public Animator armsAnim;
    private void Awake()
    {
        instance = this;
    }
    private void Update()
    {
        if (canUseInventory )
            InputManager();
        else
            usingItem = false;

    }
    private void InputManager()
    {
        if (MenuController.instance.paused)
            return;

        if (inventory.Count > 1 && !usingItem) // vi ska inte kunna swappa inventory om vi aktivt använder ett item, som att snurra på banboxen
        {
            scrollTimer +=Time.deltaTime;

            float mouseWheel = PlayerController.instance.scrollWheel.action.ReadValue<float>();
            if (mouseWheel != 0 && scrollTimer >=scrollCoolDown && !NoteController.instance.journalOpen)
            {
                if (mouseWheel > 0)
                    SwapItem(1);
                else if (mouseWheel < 0)
                    SwapItem(-1);

                scrollTimer = 0;
            }

            
        }

        if (PlayerController.instance.throwInput.action.IsPressed() && currentItem != 0 && !NoteController.instance.readingNoteDelay && !usingItem)
        {
            chargingThrow = true;
            if (throwStrength < 700)
                throwStrength += Time.deltaTime * 600;            
        }
        if (PlayerController.instance.throwInput.action.WasReleasedThisFrame() && chargingThrow && currentItem != 0)
        {
            RemoveItem(currentItem);
            chargingThrow = false;
        }
        if (PlayerController.instance.useItemInput.action.WasPressedThisFrame() && currentItem != 0 && inventory[currentItem].item.itemData.usable && !usingItem && inventory[currentItem].physicalObject.activeInHierarchy) // prova att använda item
        {       
            inventory[currentItem].physicalObject.SendMessage("UseItem",inventory[currentItem].item.itemData);
            usingItem = true;
        }
        //if (PlayerController.instance.useItemInput.action.WasReleasedThisFrame() && usingItem)
            //usingItem = false;
    }
    private void RemoveItem(int ID)
    {
        PlayerAnimator.instance.PlayPlayerAnimation(0, false, "Withdraw"); // Kasta animationen ska vara här
        

        Rigidbody objectToThrow = inventory[ID].physicalObject.GetComponent<Rigidbody>();
        inventory[ID].physicalObject.transform.parent = worldItemParent? worldItemParent : null;
        inventory[ID].item.meshObject.layer = 9; // Item Layer
        inventory[ID].item.gameObject.layer = 9;
        inventory[ID].physicalObject.transform.localScale *= 2f; // dubbla storleken till sin ursprungliga storlek
        if (inventory[ID].item.itemData.usable)
            inventory[ID].physicalObject.SendMessage("UnUseItem"); // Om vi intergrerar med itemet så vill vi sluta intergrera med det när vi kastar bort
        inventory.Remove(inventory[ID]);
        
       

        objectToThrow.isKinematic = false;
        objectToThrow.AddForce(PlayerController.instance.playerCamera.forward * throwStrength*2);
        SwapItem(ID - 1);
        throwStrength = 0;       
    }
    public void DestroyItem()
    {
        GameObject trashItem = inventory[currentItem].physicalObject;
        trashItem.GetComponent<Collider>().isTrigger = true;
        trashItem.GetComponent<Animator>().enabled = true;
        inventory.Remove(inventory[currentItem]);
        SwapItem(currentItem - 1);
    }
 
    public void SwapToItem(int slotID)
    {
        currentItem = slotID;
        SwapItem(0);
        //for (int i = 0; i < inventory.Count; i++)
        //{
            //if (i == currentItem)
            //{
               // inventory[i].physicalObject.SetActive(true);
               // currentItemData = inventory[i].item;
            //}
           // else
            //{
               // if (inventory[i].physicalObject.activeInHierarchy && inventory[i].item && inventory[i].item.itemData.usable)// senaste aktiva itemet
               // {
                //    currentItemData = null;
                //    inventory[i].physicalObject.SendMessage("UnUseItem");
               // }
               // inventory[i].physicalObject.SetActive(false);
            //}
        //}
    }
    public IEnumerator DelayedItemVisibleSwap(GameObject obj, bool active, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(active);
        if(active && inventory[currentItem].item && inventory[currentItem].item == currentItemData)
        {
            if(inventory[currentItem].physicalObject.activeInHierarchy)
                inventory[currentItem].physicalObject.SendMessage("PickedUp");
            else
            {
                // Vänta tills objektet blir aktivt
                yield return new WaitUntil(() => inventory[currentItem].physicalObject.activeInHierarchy);
                Debug.Log("Vi väntade på att " + inventory[currentItem].physicalObject + " skulle bli aktivt");
                // När det blir aktivt, kör SendMessage en gång och avsluta
                inventory[currentItem].physicalObject.SendMessage("PickedUp");
            }
        }
            
    }
    public void SwapItem(int ID)
    {
        

        inventoryAudio.pitch = Random.Range(0.9f,1.2f);
        inventoryAudio.PlayOneShot(swapItemSound[Random.Range(0,1)]);
        currentItem += ID;
        if (currentItem >= inventory.Count)
            currentItem = 0;
        else if (currentItem < 0)
            currentItem = inventory.Count-1;

        for(int i = 0; i <inventory.Count; i ++)
        {
            if(i == currentItem)
            {
                currentItemData = inventory[i].item;
                StartCoroutine(DelayedItemVisibleSwap(inventory[i].physicalObject, true, 0.2f));

                if (inventory[i].item)
                {                   
                    PlayerAnimator.instance.PlayPlayerAnimation(inventory[i].item.itemData.animationID, currentItemData.itemData.twoHanded, "play");
                }                  
                else
                    PlayerAnimator.instance.PlayPlayerAnimation(0, false, "play");
            }
            else
            {
                if(inventory[i].physicalObject.activeInHierarchy && inventory[i].item && inventory[i].item.itemData.usable)// senaste aktiva itemet
                {
                    currentItemData = null;
                    inventory[i].physicalObject.SendMessage("UnUseItem");
                }
                StartCoroutine(DelayedItemVisibleSwap(inventory[i].physicalObject, false, 0.2f));
                //inventory[i].physicalObject.SetActive(false);
            }
        }
    }

    public void AddNewItem(Item newItem)
    {
        if (inventory.Count >= inventorySize)// Inventory full
        {
            NotificationController.instance.NewNotification("<b>I can’t carry anymore<b>");
            return;
        }
        // Inventory inte full
        //PlayerAnimator.instance.PlayPlayerAnimation(0,false,"PickUp");
        Inventory addToInventory = new Inventory();
        addToInventory.item = newItem;
        addToInventory.physicalObject = newItem.gameObject;
        addToInventory.itemName = newItem.itemName;
        if (newItem.itemData.pickUpSound.Length >0)
        {
            inventoryAudio.pitch = Random.Range(0.9f,1.2f);
            inventoryAudio.PlayOneShot(newItem.itemData.pickUpSound[Random.Range(0, newItem.itemData.pickUpSound.Length)]);
        }
        newItem.gameObject.layer = 17;// inventory Layer  
        newItem.meshObject.layer = 17; // inventory Layer          
        newItem.canInteract = false;
        newItem.gameObject.SetActive(false);
        newItem.GetComponent<Rigidbody>().isKinematic = true;
        newItem.transform.parent = inventoryParent;
        newItem.transform.position = inventoryParent.position;
        newItem.transform.rotation = inventoryParent.rotation;
        newItem.transform.localScale *=0.5f; // skala objektet till 50%
        inventory.Add(addToInventory);
        Color itemColor = NotificationController.instance.colorPallet[0];
        NotificationController.instance.NewNotification("<b><color=#"+ ColorUtility.ToHtmlStringRGBA(itemColor)+">"+ newItem.itemName +"</color></b>" + " added to equipment");
        SwapToItem(inventory.Count-1);
    }

    public void AddDisc(Item newItem)
    {
        if(banishmentBoxDiscs < max_BanishmentBoxDiscs)
        {
            if (newItem.itemData.pickUpSound.Length > 0)
            {
                inventoryAudio.pitch = Random.Range(0.9f, 1.2f);
                inventoryAudio.PlayOneShot(newItem.itemData.pickUpSound[Random.Range(0, newItem.itemData.pickUpSound.Length)]);
            }
            newItem.gameObject.SetActive(false);
            Color itemColor = NotificationController.instance.colorPallet[0];
            NotificationController.instance.NewNotification("<b><color=#" + ColorUtility.ToHtmlStringRGBA(itemColor) + ">" + newItem.itemName + "</color></b>" + " added to equipment");
            banishmentBoxDiscs++;
        }
        else
        {
            Color itemColor = NotificationController.instance.colorPallet[0];
            NotificationController.instance.NewNotification("<b><color=#" + "I can’t carry anymore" + ColorUtility.ToHtmlStringRGBA(itemColor) + ">" + newItem.itemName + "</color></b>");
        }
    }
}
