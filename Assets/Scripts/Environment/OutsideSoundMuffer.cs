using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutsideSoundMuffer : MonoBehaviour
{
    public bool closeToWindow;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == PlayerController.instance.gameObject)
        {
            EnvironmentAudio.instance.RemoveWindow(this);
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == PlayerController.instance.gameObject)
        {
            float distance = Vector3.Distance(PlayerController.instance.transform.position, transform.position);
            EnvironmentAudio.instance.AddWindow(this, distance);
        }
            
    }
}
