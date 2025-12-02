using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SettingsMenu : MonoBehaviour
{
    public List<SettingTabs> settingsTab = new();
    [System.Serializable]
    public class SettingTabs
    {
        public string tabName;
        public GameObject tabObject;
        public Image tabButton;

    }

    public Color selectedColor;
    public Color normalColor;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OpenSettingsTab(int id)
    {
        foreach(SettingTabs tab in settingsTab)
        {
            if (tab.tabObject.activeInHierarchy)
                tab.tabObject.SetActive(false);
            tab.tabButton.color = normalColor;
        }
        settingsTab[id].tabObject.SetActive(true);
        settingsTab[id].tabButton.color = selectedColor;
    }
    public void CloseSettingTabs()
    {
        MenuController.instance.settingsMenuOpen = false;
        foreach (SettingTabs tab in settingsTab)
        {
            if (tab.tabObject.activeInHierarchy)
                tab.tabObject.SetActive(false);
            tab.tabButton.color = normalColor;
        }
    }
    public void SettingButtonsVisible(bool show)
    {       
        foreach(SettingTabs tab in settingsTab)
        {
            tab.tabButton.gameObject.SetActive(show);
        }
    }
}
