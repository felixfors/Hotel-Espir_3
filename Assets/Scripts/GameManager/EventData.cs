using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventData : MonoBehaviour
{
    public bool loop; // can this event be triggered multiple times
    public bool triggered; // has this event been triggered
    public int triggeredCount; // how many times have this event been triggered
    public float floatValue;
    public void EventAction(bool state)
    {
        triggered = state;
        triggeredCount++;
    }
}

