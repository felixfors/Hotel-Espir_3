using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractManager : MonoBehaviour
{
    public static InteractManager instance;
    public int currentPriority;
    public float interactDistance;
    public bool debugRay = true;
    public Transform lastHit;
    public bool canInteract;
    private bool lastInteractive;
    public bool currentlyInteracting;
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        canInteract = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (lastHit && !lastHit.gameObject.activeInHierarchy)
            lastHit = null;
        if(canInteract)
        {
            if (MenuController.instance.paused)
            {
                if(lastHit)
                {
                    lastHit.transform.SendMessage("EndInteract");
                    lastHit = null;
                }
                return;
            }
            else
               
            InteractBeam();
        }
        if(lastInteractive != currentlyInteracting)
        {
            lastInteractive = currentlyInteracting;
            if (!currentlyInteracting)
                PriorityLevel(0);
        }
    }
    public void PriorityLevel(int priority)
    {
        currentPriority = priority;
    }
    private void InteractBeam()
    {
        Vector3 rayOrigin = new Vector3(0.5f, 0.5f, 0f); // center of the screen
        float rayLength = interactDistance;

        // actual Ray
        Ray ray = Camera.main.ViewportPointToRay(rayOrigin);

        // debug Ray
        if(debugRay)
        Debug.DrawRay(ray.origin, ray.direction * rayLength, Color.red);

        int raycastIgnoreLayer = 1 << 2;
        int playerLayer = 1 << 3;        
        int doorLayer = 1 << 11;
        int waypointLayer = 1 << 12;
        int playerBlock = 1 << 13;
        int inventoryLayer = 1 << 17;
        int layerMask = raycastIgnoreLayer | playerLayer | doorLayer | waypointLayer | playerBlock | inventoryLayer;
        layerMask = ~layerMask;

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, rayLength, layerMask))
        {
            
            if (hit.transform.tag == "Interactable" && !lastHit)
            {
                Debug.Log("vi träffar " + hit.transform.name);
                hit.transform.SendMessage("StartInteract", SendMessageOptions.DontRequireReceiver);
                lastHit = hit.transform;
            }
            else if (lastHit && hit.transform != lastHit)
            {
                if (!currentlyInteracting)
                    PriorityLevel(0);
                lastHit.transform.SendMessage("EndInteract", SendMessageOptions.DontRequireReceiver);
                lastHit = null;
                
            }
                
        }
        else
        {
            if(!currentlyInteracting)
            PriorityLevel(0);
            if(lastHit)
            {
                if (lastHit.gameObject.activeInHierarchy)
                    lastHit.transform.SendMessage("EndInteract", SendMessageOptions.DontRequireReceiver);
                else
                    lastHit = null;
                lastHit = null;
            }
            
        }
       
    }
}
