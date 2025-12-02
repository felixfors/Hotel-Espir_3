using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderLight : MonoBehaviour
{
    public GameObject lightObject;

    // Start is called before the first frame update
    void Start()
    {
    }
    private void OnEnable()
    {
        ThunderController.onThunderPlayed += Thunder;
    }

    private void OnDisable()
    {
        ThunderController.onThunderPlayed -= Thunder;
    }

    private void Thunder()
    {
   
        if (ThunderController.instance.thunderIntensity >= 0.01f)
        {
            lightObject.SetActive(true);
        }
        else
        {
            lightObject.SetActive(false);
        }
    }
}
