using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightItem : MonoBehaviour
{
    private bool canInteract;
    public AudioClip pickUpSound;
    // Start is called before the first frame update
    void Start()
    {
        if (InventoryController.instance.flashLight.isInInventory)
            gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(PlayerController.instance.itemInteractInput.action.WasPressedThisFrame() && canInteract)
        {
            InventoryController.instance.flashLight.PickedUpFlashlight();
            InventoryController.instance.inventoryAudio.PlayOneShot(pickUpSound ? pickUpSound : InventoryController.instance.swapItemSound[Random.Range(0,1)]); // om vi har valt pickup sound kör det annars ta inventory sound
            
            Color itemColor = NotificationController.instance.colorPallet[0];            
            NotificationController.instance.NewNotification("<b><color=#" + ColorUtility.ToHtmlStringRGBA(itemColor) + ">" +"Flashlight"+ "</color></b>" + " added to equipment");
            gameObject.SetActive(false);
        }
    }
    public void StartInteract()
    {
        if(!canInteract)
            canInteract = true;
    }
    public void EndInteract()
    {
        if (canInteract)
            canInteract = false;
    }
}
