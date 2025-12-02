using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteItem : MonoBehaviour
{
    private bool canInteract;
    public NoteData noteData;
    public bool pickedUp;
    private MeshCollider meshCollider;
    // Start is called before the first frame update
    void Start()
    {
        meshCollider = GetComponent<MeshCollider>();
    }

    void Update()
    {
        if (PlayerController.instance.itemInteractInput.action.WasPressedThisFrame() && canInteract && !NoteController.instance.readingNote)
        {
            if (NoteController.instance.gotJournal)
                PickedUp();
            else
            {
                meshCollider.enabled = false;
                NoteController.instance.ClosedNoteEvent += RenableNote;
            }
                

            NoteController.instance.DisplayNote(noteData);
            
        }
    }
    private void RenableNote()
    {
        Invoke("renableNoteDelay",0.3f);
        NoteController.instance.ClosedNoteEvent -= RenableNote;
    }
    private void renableNoteDelay()
    {
        meshCollider.enabled = true;
    }
    public void PickedUp()
    {
        pickedUp = true;
        gameObject.SetActive(false);
    }
    public void StartInteract()
    {


        if (!canInteract)
            canInteract = true;
    }
    public void EndInteract()
    {
        if (canInteract)
        {
            canInteract = false;
        }
            
    }
}
