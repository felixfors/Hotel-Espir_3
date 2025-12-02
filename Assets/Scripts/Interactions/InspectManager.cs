using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;
public class InspectManager : MonoBehaviour
{
    public bool inspecting;
    public CinemachineVirtualCamera camera;
    private AudioSource audioSource;
    public AudioClip soundStart;
    public AudioClip soundEnd;

    public GameObject interactHUD;
    public UnityEvent StartEvent;
    public UnityEvent EndEvent;
    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.GetComponent<AudioSource>())
            audioSource = GetComponent<AudioSource>();
        if (interactHUD)
            interactHUD.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(inspecting)
        {
            bool moving = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;
            if (!moving) // om vi står stilla gör ingenting
                return;
            EndInspecting();
            
        }
    }
    public void StartInspecting()
    {
        if (interactHUD)
            interactHUD.SetActive(true);
        if (audioSource && soundStart)
            audioSource.PlayOneShot(soundStart);
        camera.Priority = 2;
        inspecting = true;
        PlayerController.instance.canMove = false;
        InventoryController.instance.canUseInventory = false;
        StartEvent.Invoke();

    }
    public void EndInspecting()
    {
        if (interactHUD)
            interactHUD.SetActive(false);
        if (audioSource && soundEnd)
            audioSource.PlayOneShot(soundEnd);
        camera.Priority = 0;
        inspecting = false;
        if(PlayerController.instance != null)
            PlayerController.instance.canMove = true;
        InventoryController.instance.canUseInventory = true;
        EndEvent.Invoke();
    }
}
