using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using TMPro;

public class GraphicSettings : MonoBehaviour
{
    public static GraphicSettings instance;

    private bool lastTab;
    public GameObject graphicTab;

    public Volume volume;
    public Vector4 gammaValue;
    public Vector2 gammaClamp;


    [Space(50)]
    public int resolution_Quality;
    public TextMeshProUGUI resolution_Text;
    public GameObject applyButton;
    public List<Vector2Int> resolutionList = new List<Vector2Int>();

    [Space(50)]
    public int displayport_Quality;
    public TextMeshProUGUI displayPort_Text;
    public FullScreenMode fullscreenMode;


    // Start is called before the first frame update
    void Awake()
    {
        instance = this;

        GetSupportedStandardResolutions();

        DisplayPortQuality(PlayerPrefs.GetInt("DisplayportQuality"));
        GetMyResolution();

       
    }
    private void Update()
    {
        if(graphicTab.activeInHierarchy != lastTab)
        {
            
            lastTab = graphicTab.activeInHierarchy;
            if (lastTab) // vi öppnade precis graphic settings
            {
                Debug.Log("Öppnade settings");
                UpdateUI_Info();
            }
        }
    }
    public void UpdateUI_Info()
    {
        ResolutionQuality(0);
        DisplayPortQuality(0);
    }
    private void GetSupportedStandardResolutions()
    {
        List<Vector2Int> standardResolutions = new List<Vector2Int>
    {
        // 16:9
        new Vector2Int(1280, 720),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080),
        new Vector2Int(2560, 1440),
        new Vector2Int(3840, 2160),

        // 4:3
        new Vector2Int(640, 480),
        new Vector2Int(800, 600),
        new Vector2Int(1024, 768),
        new Vector2Int(1152, 864),
        new Vector2Int(1280, 960),
        new Vector2Int(1400, 1050),
        new Vector2Int(1600, 1200),
        new Vector2Int(2048, 1536)
    };

        resolutionList = new List<Vector2Int>();
        Resolution[] systemResolutions = Screen.resolutions;

        foreach (var res in standardResolutions)
        {
            if (systemResolutions.Any(r => r.width == res.x && r.height == res.y))
            {
                resolutionList.Add(res);
            }
        }

        // Sortera efter bredd först, sedan höjd
        resolutionList.Sort((a, b) =>
        {
            int compareWidth = a.x.CompareTo(b.x);
            return compareWidth != 0 ? compareWidth : a.y.CompareTo(b.y);
        });
    }
    private void GetMyResolution()
    {
        Vector2Int currentResolution;

        if (PlayerPrefs.HasKey("ResolutionQuality"))
        {
            int savedRes = PlayerPrefs.GetInt("ResolutionQuality");
            currentResolution = new Vector2Int(resolutionList[savedRes].x, resolutionList[savedRes].y);
        }          
        else
            currentResolution = new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);

        for (int i = 0; i < resolutionList.Count; i ++)
        {
            if (resolutionList[i] == currentResolution)// vi har hittat våran upplösning i inställningarna
            {
                ResolutionQuality(i);
                PlayerPrefs.SetInt("ResolutionQuality", i);
                break;
            }
            
        }
    }
    public void ApplyResolution()
    {
        PlayerPrefs.SetInt("ResolutionQuality", resolution_Quality);
        ResolutionQuality(0);

        Screen.SetResolution(resolutionList[resolution_Quality].x, resolutionList[resolution_Quality].y, fullscreenMode);
    }

    public void NewGamma(float _newValue)
    {
        if (volume.sharedProfile.TryGet<UnityEngine.Rendering.HighDefinition.LiftGammaGain>(out var liftGammaGain))
        {
            float _totalValue = liftGammaGain.gamma.value.w + _newValue;
            if (_totalValue < gammaClamp.x || _totalValue > gammaClamp.y)
                return;
            liftGammaGain.gamma.value = new Vector4(liftGammaGain.gamma.value.x, liftGammaGain.gamma.value.y, liftGammaGain.gamma.value.z, _newValue);

        }
    }
    public void ResolutionQuality(int qualitySetting)
    {
        resolution_Quality += qualitySetting;
        int maxSetting = resolutionList.Count-1;
        if (resolution_Quality > maxSetting)
            resolution_Quality = 0;
        else if (resolution_Quality < 0)
            resolution_Quality = maxSetting;

        resolution_Text.text = resolutionList[resolution_Quality].x + "x" + resolutionList[resolution_Quality].y;

        applyButton.SetActive(resolution_Quality != PlayerPrefs.GetInt("ResolutionQuality"));
    }

    public void DisplayPortQuality(int qualitySetting)
    {
        displayport_Quality += qualitySetting;
        int maxSetting = 2;
        if (displayport_Quality > maxSetting)
            displayport_Quality = 0;
        else if (displayport_Quality < 0)
            displayport_Quality = maxSetting;
        PlayerPrefs.SetInt("DisplayportQuality", displayport_Quality);


        switch (displayport_Quality)
        {
            case 0:
                fullscreenMode = FullScreenMode.ExclusiveFullScreen;
                displayPort_Text.text = "Full screen";
                break;
            case 1:
                fullscreenMode = FullScreenMode.Windowed;
                displayPort_Text.text = "Windowed";
                break;
            case 2:
                fullscreenMode = FullScreenMode.FullScreenWindow;
                displayPort_Text.text = "Borderless Window";
                break;
        }

        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, fullscreenMode);
    }
}
