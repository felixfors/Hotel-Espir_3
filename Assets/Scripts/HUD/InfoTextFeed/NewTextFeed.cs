using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NewTextFeed : MonoBehaviour
{
    public string overridetext;
    private bool show;
    public TextMeshProUGUI inGameText;

    public void StartInteract()
    {
        if(!show)
        {
            string newText;
            newText = overridetext;
            newText += (inGameText ? inGameText.text : "");
            InfoTextFeedController.instance.UpdateTextFeed(inGameText? newText : overridetext);
            show = true;
        }
    }
    public void EndInteract()
    {
        if(show)
        {
            InfoTextFeedController.instance.UpdateTextFeed(null);
            show = false;
        }
    }
}
