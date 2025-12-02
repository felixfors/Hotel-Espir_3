using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public bool canInteract;
    public int priority;
    public ItemData itemData;
    public GameObject meshObject;
    [HideInInspector]
    public string itemName;
    [HideInInspector]
    public bool destroyOnUse;

    public Texture tempText;
    private Item item;
    // Start is called before the first frame update
    void Start()
    {
        item = GetComponent<Item>();           
    }
    private void Awake()
    {
    itemName = itemData.itemName;
    destroyOnUse = itemData.destroyOnUse;
    }
    // Update is called once per frame
    void Update()
    {        
        if(PlayerController.instance.itemInteractInput.action.WasPressedThisFrame() && canInteract)
        {
            if(item.itemData.itemCategory == ItemData.ItemCategory._default)
            InventoryController.instance.AddNewItem(item);
            else if(item.itemData.itemCategory == ItemData.ItemCategory.disc)
            {
                InventoryController.instance.AddDisc(item);
            }
            InteractManager.instance.currentlyInteracting = false;
        }
    }
   
    public void StartInteract()
    {
        Interacting_Status(true);
        canInteract = true;
    }
    public void EndInteract()
    {
        canInteract = false;
        Interacting_Status(false);
    }
    public void PickedUp()
    {
        return;
    }
    public void Interacting_Status(bool interacting)
    {
        InteractManager.instance.currentlyInteracting = interacting;
        if (interacting)
            InteractManager.instance.PriorityLevel(priority);
        else
            InteractManager.instance.PriorityLevel(0);
    }
}
