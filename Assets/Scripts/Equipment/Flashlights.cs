using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlights : MonoBehaviour
{
    public bool isInInventory;
    public GameObject flashLight;
    public bool on;
    [HideInInspector]
    public AudioSource flashlightAudio;
    public AudioClip[] flashLightSound;

    // --- Queue system ---
    private Queue<bool> flashlightQueue = new Queue<bool>();
    public float cooldown = 2f;           // tid mellan varje toggle
    public int maxQueueSize = 3;          // max antal actions i kön
    private bool isProcessingQueue = false;

    void Awake()
    {
        flashlightAudio = GetComponent<AudioSource>();
        on = flashLight.activeInHierarchy;
    }

    void Update()
    {
        if (PlayerController.instance.flashlightInput.action.WasPressedThisFrame() && isInInventory && InventoryController.instance.canUseInventory)
        {
            // Rensa hela kön → bara senaste spelar roll
            flashlightQueue.Clear();
            flashlightQueue.Enqueue(!on);


            // Starta processen om den inte redan körs
            if (!isProcessingQueue)
                StartCoroutine(ProcessFlashlightQueue());
        }
    }

    IEnumerator ProcessFlashlightQueue()
    {
        isProcessingQueue = true;

        while (flashlightQueue.Count > 0)
        {
            
            bool nextState = flashlightQueue.Dequeue();
            if (PlayerAnimator.instance.twoHand) // om vi försöker öppna ficklampan samtidigt som vi håller i twohanded så betyder det att vi vill tända lampan
            {
                nextState = true;
            }
            
                on = nextState;
            FlashlightOnOff(on);

            // vänta innan nästa toggle får köras
            yield return new WaitForSeconds(cooldown);
        }

        isProcessingQueue = false;
    }

    public void FlashlightOnOff(bool state)
    {
        if (state)
        {
            if (!PlayerAnimator.instance.twoHand)
            {
                PlayerAnimator.instance.anim.SetBool("Flashlight",on);
                //PlayerAnimator.instance.PlayPlayerAnimation(0, false, "Flashlight");
                StartCoroutine(DelayedItemVisibleSwap(flashLight, true, 0.85f));
            }
            else
            {
                //InventoryController.instance.SwapToItem(0);
                StartCoroutine(TurnOnFromTwoHanded());
            }
        }
        else
        {

            //PlayerAnimator.instance.PlayPlayerAnimation(0, false, "Flashlight");
            PlayerAnimator.instance.anim.SetBool("Flashlight", false);
            StartCoroutine(DelayedItemVisibleSwap(flashLight, false, 0.3f));
        }
    }

    public void PickedUpFlashlight()
    {
        isInInventory = true;
        on = true;
        FlashlightOnOff(true);
    }
    public void GetFlashLightData(bool isOn) //körs vid save load, in flashlight finns i inventory
    {
        isInInventory = true;
        on = isOn;
        if(isOn)
            FlashlightOnOff(isOn);
    }
    public IEnumerator DelayedItemVisibleSwap(GameObject obj, bool active, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        obj.SetActive(active);
    }
    public IEnumerator TurnOnFromTwoHanded()
    {
        InventoryController.instance.canUseInventory = false;
        InventoryController.instance.SwapToItem(0); // byt till händerna som är "one handed"


        yield return new WaitForSeconds(1);
        FlashlightOnOff(true);
        //PlayerAnimator.instance.PlayPlayerAnimation(0, false, "Flashlight");
        //StartCoroutine(DelayedItemVisibleSwap(flashLight, true, 0.85f));

        yield return new WaitForSeconds(1);
        InventoryController.instance.canUseInventory = true;
    }

    public void PlayAudio(int sound)
    {
        flashlightAudio.PlayOneShot(sound ==0? flashLightSound[0] : flashLightSound[1]);
    }
}