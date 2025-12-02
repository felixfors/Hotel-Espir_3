using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Linq;
using TMPro;

public class InputSprite : MonoBehaviour
{
    [HideInInspector]
    public bool active;
    [Header("References")]
    public InputActionReference actionReference;

    public Image positiveIconImage;
    public Image negativeIconImage;
    public TextMeshProUGUI positiveText;
    public TextMeshProUGUI negativeText;

    public TextMeshProUGUI inputText;
    public IconData iconData;

    private Color iconColorPressed;
    public bool updateSprite;

    private void Start()
    {
        iconColorPressed = Color.white;
        iconColorPressed.a = 0.5f;
    }
    private void OnEnable()
    {
        DetectController.OnInputDeviceChanged += UpdateIcon;
    }

    private void OnDisable()
    {
        DetectController.OnInputDeviceChanged -= UpdateIcon;
    }
    public void IsActive(bool state)
    {
        ResetIconColor();
        active = state;
    }
    private void Update()
    {
        if(actionReference.action.WasPerformedThisFrame() && active)
        {
            // Hämta värdet som float
            float value = actionReference.action.ReadValue<float>();

            if (value > 0f)
            {
                if(positiveIconImage)
                {
                    positiveIconImage.color = iconColorPressed;
                    Invoke("ResetIconColor", 0.1f);
                }
            }
            else if (value < 0f)
            {
                if(negativeIconImage)
                {
                    negativeIconImage.color = iconColorPressed;
                    Invoke("ResetIconColor", 0.1f);
                }
                
            }
            else
            {
                if(positiveIconImage)
                {
                    positiveIconImage.color = iconColorPressed;
                    Invoke("ResetIconColor", 0.1f);
                }   
            }
        }
    }
    private void ResetIconColor()
    {
        if(positiveIconImage)
        positiveIconImage.color = Color.white;
        if(negativeIconImage)
        negativeIconImage.color = Color.white;
    }
    private void UpdateIcon()
    {
        if (!active)
            return;
        // måste göra så att UpdateIcon k
        if (actionReference == null || iconData == null)
            return;

        var action = actionReference.action;

        // Rensa ikoner och text
        if(positiveIconImage)
            positiveIconImage.sprite = null;
        if(negativeIconImage)
            negativeIconImage.sprite = null;
        if(positiveText)
            positiveText.text = "";
        if(negativeText)
            negativeText.text = "";

        // Hämta rätt bindingar baserat på input device och composite-delar
        var positiveBinding = FindRelevantBinding(action, "positive");
        if (positiveBinding.HasValue && positiveIconImage)
            SetIconForBinding(positiveBinding.Value, positiveIconImage,"positive");

        var negativeBinding = FindRelevantBinding(action, "negative");
        if (negativeBinding.HasValue && negativeIconImage)
            SetIconForBinding(negativeBinding.Value, negativeIconImage,"negative");

        // Om ingen av dem hittades, ta första icke-composite binding som fallback
        if (!positiveBinding.HasValue && !negativeBinding.HasValue)
        {
            var fallback = action.bindings.FirstOrDefault(b => !b.isComposite && !b.isPartOfComposite);
            if (fallback != null)
            {
                SetIconForBinding(fallback, positiveIconImage,"positive");
                negativeIconImage.sprite = null;
            }
        }
    }

    private InputBinding? FindRelevantBinding(InputAction action, string compositePartName)
    {
        bool isGamepad = DetectController.instance.inputType == DetectController.InputType.gamepad;

        for (int i = 0; i < action.bindings.Count; i++)
        {
            var binding = action.bindings[i];

            if (binding.isComposite)
            {
                for (int j = i + 1; j < action.bindings.Count && action.bindings[j].isPartOfComposite; j++)
                {
                    var part = action.bindings[j];
                    string path = part.path.ToLower();

                    bool correctDevice =
                        (isGamepad && path.Contains("<gamepad>")) ||
                        (!isGamepad && (path.Contains("<keyboard>") || path.Contains("<mouse>")));

                    if (part.name.ToLower().Contains(compositePartName.ToLower()) && correctDevice)
                    {
                        return part;
                    }
                }
            }
            else
            {
                string path = binding.path.ToLower();

                bool correctDevice =
                    (isGamepad && path.Contains("<gamepad>")) ||
                    (!isGamepad && (path.Contains("<keyboard>") || path.Contains("<mouse>")));

                if (correctDevice)
                {
                    return binding;
                }
            }
        }

        return null;
    }

    private void SetIconForBinding(InputBinding binding, Image targetImage, string polarity)
    {
        string inputName = binding.effectivePath.ToLower();
        bool foundIcon = false;

        if (DetectController.instance.inputType == DetectController.InputType.gamepad)
        {
            foreach (var gamepadIcon in iconData.gamepadSprites)
            {               
                if (inputName == gamepadIcon.inputName.ToLower()) // ← exakt matchning!
                {
                    if (DetectController.instance.gamepadType == DetectController.GamepadType.xbox)
                    {
                        targetImage.enabled = true;
                        targetImage.sprite = gamepadIcon.xboxSprite;
                    }
                    else if (DetectController.instance.gamepadType == DetectController.GamepadType.playstation)
                    {
                        targetImage.enabled = true;
                        targetImage.sprite = gamepadIcon.playstationSprite;
                    }

                    foundIcon = true;
                    break;
                }
            }
        }
        else if (DetectController.instance.inputType == DetectController.InputType.keyboard)
        {
            //Debug.Log(inputName);
            foreach (var keyboardIcon in iconData.keyboardSprites)
            {
                if (inputName == keyboardIcon.inputName.ToLower()) // ← exakt matchning!
                {
                    targetImage.enabled = true;
                    targetImage.sprite = keyboardIcon.sprite;
                    foundIcon = true;
                    break;
                }
            }
        }
        if (!foundIcon)
        {
            targetImage.sprite = null;
            targetImage.enabled = false;
            if (polarity == "positive")
                positiveText.text = InputControlPath.ToHumanReadableString(binding.effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
            else if (polarity == "negative")
                negativeText.text = InputControlPath.ToHumanReadableString(binding.effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
            else
                positiveText.text = InputControlPath.ToHumanReadableString(binding.effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
        }
        else
        {
            if(positiveText)
            positiveText.text = "";
            if(negativeText)
            negativeText.text = "";
        }
    }
}