using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ItemOilcan : MonoBehaviour
{
    public Transform useTarget;
    public UnityEvent useEvent;
    public AudioSource audioSource;
    private bool used = false;
    private ItemData itemData;
    private EventData eventData;
    private void Start()
    {
        eventData = GetComponent<EventData>();
        if(eventData.triggered)
        {
            used = true;
            useEvent.Invoke();
        }
    }
    public void UseItem(ItemData _itemData)
    {
        InventoryController.instance.usingItem = false;
        if (itemData == null)
            itemData = _itemData;

        if (InteractManager.instance.lastHit == null)
            return;
        if (used)
            return;

        if(InteractManager.instance.lastHit == useTarget)
        {         
            StartCoroutine(UsingItem());           
        }
    }

    public IEnumerator UsingItem()
    {
        Debug.Log("häll upp olja");
        InventoryController.instance.usingItem = true;
        PlayerAnimator.instance.PlayPlayerAnimation(2,false,"Use");
        audioSource.PlayOneShot(itemData.useSound);
        yield return new WaitForSeconds(1);
        InventoryController.instance.usingItem = false;
        used = true;
        useEvent.Invoke();
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
