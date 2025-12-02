using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JournalItem : MonoBehaviour
{
    private bool canInteract;
    public AudioClip pickUpSound;
    // Start is called before the first frame update
    void Start()
    {
        if (NoteController.instance.gotJournal)
            gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerController.instance.itemInteractInput.action.WasPressedThisFrame() && canInteract)
        {
            NoteController.instance.gotJournal = true;
            NoteController.instance.audioSource.PlayOneShot(pickUpSound);
            Color itemColor = NotificationController.instance.colorPallet[0];
            NotificationController.instance.NewNotification("<b><color=#" + ColorUtility.ToHtmlStringRGBA(itemColor) + ">" + "Journal" + "</color></b>" + " added to equipment");
            gameObject.SetActive(false);
        }
    }
    public void StartInteract()
    {
        if (!canInteract)
            canInteract = true;
    }
    public void EndInteract()
    {
        if (canInteract)
            canInteract = false;
    }
}
