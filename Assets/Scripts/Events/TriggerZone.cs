using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class TriggerZone : MonoBehaviour
{
    public GameObject trigger;
    public AudioSource audioSource;
    public AudioClip eventSound;

    public bool triggered;
    [HideInInspector]
    public EventData triggerData;

    public bool actionTrigger;
    private EventData actionTriggerData;

    public UnityEvent actionEvent;

    private void Awake()
    {
        trigger.AddComponent<TriggerRequirment>(); // lägger till komponent för collision
        trigger.GetComponent<TriggerRequirment>().triggerController = this; // gör så den nya komponenten ska snacka tillbaka till skaparen

        triggerData = trigger.GetComponent<EventData>();
        actionTriggerData = GetComponent<EventData>();

        trigger.GetComponent<MeshRenderer>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
    }
    private void Start()
    {
        if (actionTriggerData.triggered)
            actionEvent.Invoke();
    }
    public void OnTriggerEnter(Collider other) // ActionTrigger
    {
        if(other.gameObject.tag == "Player" && !actionTriggerData.triggered)
        {
            if(trigger == null)
            {
                actionTriggerData.triggered = true;
                if (audioSource && eventSound)
                    audioSource.PlayOneShot(eventSound);

                actionEvent.Invoke();
            }
            else if(trigger !=null && triggerData.triggered)
            {
                actionTriggerData.triggered = true;
                if(audioSource && eventSound)
                    audioSource.PlayOneShot(eventSound);

                actionEvent.Invoke();
            }
        }
    }

}
