using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EventData))]
public class AITriggerZone : MonoBehaviour
{
    public Transform AI_startPos;
    public Transform AI_GoalPos;
    public AudioSource audioSource;
    public AudioClip triggerSound;
    private EventData eventData;


    private void Awake()
    {
        eventData = GetComponent<EventData>();
    }
    public void OnTriggerEnter(Collider other)
    {
        if (!eventData.loop && eventData.triggered)
            return;
        if(other.tag == "Player")
        {
            if (!eventData.triggered)
                eventData.triggered = true;

            eventData.triggeredCount++;

            if (audioSource && triggerSound)
                audioSource.PlayOneShot(triggerSound);

            EnemyController.instance.FreezeGhost(false);
            EnemyController.instance.agent.Warp(AI_startPos.position);
            EnemyController.instance.target.position = AI_startPos.position;
            Invoke("ReDirectGhost",0.1f);
        }
    }
    private void ReDirectGhost()
    {
        EnemyController.instance.target.position = AI_GoalPos.position;
    }
}
