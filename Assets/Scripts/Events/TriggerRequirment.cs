using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerRequirment : MonoBehaviour
{
    public TriggerZone triggerController;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            triggerController.triggerData.triggered = true;
        }
    }
}
