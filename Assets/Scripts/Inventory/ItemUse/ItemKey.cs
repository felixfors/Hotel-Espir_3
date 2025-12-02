using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemKey : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UseItem(ItemData itemData)
    {
        if (InteractManager.instance.lastHit == null)
            return;

        var currentItem = InventoryController.instance.inventory[InventoryController.instance.currentItem];
        GameObject currentHitObject = InteractManager.instance.lastHit.gameObject;
        if (currentHitObject.GetComponent<Doors>()) // Siktar vi på en dörr?
        {
            Doors currentDoor = currentHitObject.GetComponent<Doors>();
            if (!currentDoor.locked) return; // Om dörren är öppen så ska vi avbryta

            if (currentDoor.unlockItem == itemData) // nyckeln stämmer överns med dörrens nyckel
            {
                currentDoor.StartCoroutine(currentDoor.UnlockDoor(currentItem.item));
                if (itemData.useSound)
                {
                    currentDoor.doorAudio.Stop();
                    currentItem.physicalObject.GetComponent<AudioSource>().PlayOneShot(itemData.useSound);
                }
                if (itemData.destroyOnUse)
                {
                    InventoryController.instance.DestroyItem();
                }
                return;
            }
        }
        InventoryController.instance.usingItem = false;
    }
    public void UnUseItem()
    {
        InventoryController.instance.usingItem = false;
        return;
    }
    public void PickedUp()
    {
        return;
    }
}
