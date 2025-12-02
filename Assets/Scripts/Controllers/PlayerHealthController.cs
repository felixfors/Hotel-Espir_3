using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthController : MonoBehaviour
{
    public static PlayerHealthController instance;

    public int health = 1;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;    
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Respawn()
    {

    }
}
