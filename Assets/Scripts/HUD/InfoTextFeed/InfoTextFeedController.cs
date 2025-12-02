using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class InfoTextFeedController : MonoBehaviour
{
    public bool subtitleOn;
    private bool _subtitle;
    public static InfoTextFeedController instance;
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI textFeed;
    
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        _subtitle = subtitleOn;
        UpdateTextFeed(null);
    }
    private void Update()
    {
        if(_subtitle != subtitleOn)
        {
            canvasGroup.alpha = subtitleOn ? 1 : 0;
            _subtitle = subtitleOn;
        }
    }
    public void UpdateTextFeed(string _newFeed)
    {
        if (!subtitleOn)
            return;
        
        if(_newFeed !=null)
        {
            canvasGroup.alpha = 1;
            textFeed.text = _newFeed;
        }
        else
        {
            canvasGroup.alpha = 0;
            textFeed.text = "";
        }
        
    }
}
