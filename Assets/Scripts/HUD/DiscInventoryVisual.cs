using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DiscInventoryVisual : MonoBehaviour
{
    private int disc;
    public GameObject [] discImages;
    // Start is called before the first frame update
    void Start()
    {
        UpdateDiscHUD();
    }

    // Update is called once per frame
    void Update()
    {
        if(disc != InventoryController.instance.banishmentBoxDiscs)
        {
            disc = InventoryController.instance.banishmentBoxDiscs;
            UpdateDiscHUD();           
        }
    }
    private void UpdateDiscHUD()
    {
        
        for(int i = 0; i < discImages.Length; i ++)
        {
            if (i < disc)
            {
                discImages[i].SetActive(true);
            }              
            else
            {
                discImages[i].SetActive(false);
            }               
        }
    }
}
