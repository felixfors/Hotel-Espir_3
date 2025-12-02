using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
public class DetectController : MonoBehaviour
{
    public static DetectController instance;
    public static event System.Action OnInputDeviceChanged;
    public InputType inputType;
    public enum InputType { keyboard, gamepad };

    public GamepadType gamepadType;
    public enum GamepadType { xbox, playstation, other};

public float mouseIntensitivity = 1;
    public float gamepadIntensitivity = 40;
    public float currentIntensitivity;


    private Coroutine stopRumbleAfterTimeCoroutine;

    private void Awake()
    {
        instance = this;
    }
    private void OnEnable()
    {
        InputSystem.onEvent += OnInputEvent;
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onEvent -= OnInputEvent;
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
            return;

        if (device is Gamepad)
        {
            ChangeLookIntensitivity(gamepadIntensitivity);
            inputType = InputType.gamepad;
        }
        else if (device is Keyboard || device is Mouse)
        {
            ChangeLookIntensitivity(mouseIntensitivity);
            inputType = InputType.keyboard;
        }
        else
        {
            Debug.Log($"Annat device: {device.displayName}");
        }
        OnInputDeviceChanged?.Invoke();
    }

    private void ChangeLookIntensitivity(float _value)
    {
        PlayerController.instance.lookSpeedGamepadMultiplier = _value;
        currentIntensitivity = _value;
    }

    public void RumblePulse(float lowFrequency, float highFrequency, float duration)
    {
        if(inputType == InputType.gamepad)
        {
            Gamepad.current.SetMotorSpeeds(lowFrequency, highFrequency);
                stopRumbleAfterTimeCoroutine = StartCoroutine(StopRumble(duration, Gamepad.current));

        }
    }
    private IEnumerator StopRumble(float duration, Gamepad pad)
    {
        float elapsedTime = 0;
        while(elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        pad.SetMotorSpeeds(0f,0f);
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Gamepad)
        {
            if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected)
            {
                string deviceName = device.name.ToLower();

                if (deviceName.Contains("xbox"))
                {
                    gamepadType = GamepadType.xbox;
                }
                else if (deviceName.Contains("dualshock") || deviceName.Contains("dualsense") || deviceName.Contains("wireless controller"))
                {
                    gamepadType = GamepadType.playstation;
                }
                else
                {
                    gamepadType = GamepadType.other;
                }
            }
        }
    }



}
