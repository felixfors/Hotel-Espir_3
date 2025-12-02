using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class SettingsController : MonoBehaviour
{
    public static SettingsController instance;
    public AudioMixer audioMixer;
    public string currentSlider;
    public List<AudioSettings> audioSettings = new();
    [System.Serializable]
    public class AudioSettings
    {
        public string name;
        public Slider slider;
        public TMP_InputField inputField;
        public float sliderValue;
        public float value;
        public float defaultValue;
    }
    public List<ControlsSliders> controlsSliders = new();
    [System.Serializable]
    public class ControlsSliders
    {
        public string name;
        public Slider slider;
        public TMP_InputField inputField;
        public Vector2 minMaxValue;
        public float sliderValue;
        public float value;
        public float defaultValue;
    }

    private void Start()
    {
        instance = this;
        GetCurrentSettings();
    }
    private void GetCurrentSettings()
    {
        for(int i = 0; i < audioSettings.Count; i ++) // Hämta ljud inställningar
        {
            float valueFromSave = PlayerPrefs.GetFloat(audioSettings[i].name); // hämta sparning
            float playerValue = PlayerPrefs.HasKey(audioSettings[i].name) ? valueFromSave : audioSettings[i].defaultValue; // om det finns sparat minne använd det annars default
            audioSettings[i].slider.value = playerValue; // ändra slider value till sparat minned
            audioSettings[i].inputField.text = playerValue.ToString("F2"); // ändrar input field text till det sparade värdet
            audioSettings[i].sliderValue = audioSettings[i].slider.value; // ändra floaten slidervalue till samma som slider value         
            audioMixer.SetFloat(audioSettings[i].name, Mathf.Log10(audioSettings[i].sliderValue) * 20);          
        }
        for (int i = 0; i < controlsSliders.Count; i++) // Hämta controls slider inställningar
        {
            float valueFromSave = PlayerPrefs.GetFloat(controlsSliders[i].name); // hämta sparning
            float playerValue = PlayerPrefs.HasKey(controlsSliders[i].name) ? valueFromSave : controlsSliders[i].defaultValue; // om det finns sparat minne använd det annars default

            controlsSliders[i].slider.minValue = controlsSliders[i].minMaxValue.x; // ställ in vilket som är lägsta inställningen
            controlsSliders[i].slider.maxValue = controlsSliders[i].minMaxValue.y;// ställ in vilket som är högsta inställningen           
            controlsSliders[i].slider.value = playerValue; // ändra slider value till sparat minned
            float inputfieldNormalized = Mathf.InverseLerp(controlsSliders[i].minMaxValue.x, controlsSliders[i].minMaxValue.y, playerValue);
            controlsSliders[i].inputField.text = inputfieldNormalized.ToString("F2"); // ändrar input field text till det sparade värdet
            controlsSliders[i].sliderValue = playerValue; // ändra floaten slidervalue till samma som slider value         
            ChangeSliderSettings(controlsSliders[i].name, playerValue);
        }
    }
    public void CurrentSlider(string sliderName) // den aktiva slidern
    {
        currentSlider = sliderName;
    }

    //Audio SETTINGS
    public void SetVolumeString(string inputValue) // om vi använder inputField så måste vi konvertera till float först
    {
        float convertedString = float.Parse(inputValue);
        if (convertedString > 1) convertedString = 1;
        if (convertedString < 0) convertedString = 0.0001f;
        if(convertedString >0.0001f || convertedString  < 1)
        SetVolume(convertedString);
    }
    public void SetVolume(float sliderValue) // ändrar volymen genom slidervalue eller en converterad string ^^
    {
        float volume = Mathf.Log10(sliderValue) * 20;
        audioMixer.SetFloat(currentSlider, volume);
        foreach(AudioSettings audio in audioSettings)
        {
            if(audio.name == currentSlider)
            {
                audio.value = volume;
                audio.sliderValue = sliderValue;
                audio.slider.value = sliderValue; // utifall vi ändrar slidern value ifrån string, då uppdateras denna inte av sig själv
                audio.inputField.text = sliderValue.ToString("F2"); // utifall vi ändrar ifrån float, då uppdateras denna inte av sig själv
                PlayerPrefs.SetFloat(audio.name, sliderValue);
                break;
            }
        }
    }
    public void DefaultAudioSettings()
    {
        for(int i  = 0; i < audioSettings.Count; i ++)
        {
            PlayerPrefs.DeleteKey(audioSettings[i].name); // ta bort sparat minne för allt ljud
        }
        GetCurrentSettings(); // ställ om alla inställningar
    }
    public void DefaultControllerSettings()
    {
        for (int i = 0; i < controlsSliders.Count; i++)
        {
            PlayerPrefs.DeleteKey(controlsSliders[i].name); // ta bort sparat minne för alla keybinds
            GetCurrentSettings(); // ställ om alla inställningar
        }
    }


    //Controller SETTINGS

    public void SetControllerSliderString(string inputValue) // om vi använder inputField så måste vi konvertera till float först
    {
        Vector2 _minMaxValue = new Vector2();
        for(int i = 0; i < controlsSliders.Count; i ++)
        {
            if (controlsSliders[i].name == currentSlider)
            {
                _minMaxValue = controlsSliders[i].minMaxValue;
                break;
            }               
        }
        float convertedString = float.Parse(inputValue);
        convertedString = Mathf.Clamp(convertedString, 0, 1);
        if (convertedString > 0 || convertedString < 1)
        {
            float sliderValue = Mathf.Lerp(_minMaxValue.x, _minMaxValue.y, convertedString);
            SetControllerSlider(sliderValue);
        }
           
    }
    public void SetControllerSlider(float sliderValue) // ändrar volymen genom slidervalue eller en converterad string ^^
    {
        //audioMixer.SetFloat(currentSlider, sliderValue);
        foreach (ControlsSliders controls in controlsSliders)
        {
            if (controls.name == currentSlider)
            {
                ChangeSliderSettings(controls.name, sliderValue);
               
                controls.value = sliderValue;
                controls.sliderValue = sliderValue;
                controls.slider.value = sliderValue; // utifall vi ändrar slidern value ifrån string, då uppdateras denna inte av sig själv
                float inputfieldNormalized = Mathf.InverseLerp(controls.minMaxValue.x, controls.minMaxValue.y, sliderValue);
                controls.inputField.text = inputfieldNormalized.ToString("F2"); // utifall vi ändrar ifrån float, då uppdateras denna inte av sig själv
                PlayerPrefs.SetFloat(controls.name, sliderValue);
                break;
            }
        }
    }
    private void ChangeSliderSettings(string target, float sliderValue)
    {
        if (target == "Mouse sensetivity" && SceneManager.GetActiveScene().buildIndex != 0)
            PlayerController.instance.lookSpeed = sliderValue;
        else if (target == "Gamma")
            GraphicSettings.instance.NewGamma(sliderValue);
    }
}
