using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Processors;

[System.Serializable]
public class StickResponseCurveProcessor : InputProcessor<Vector2>
{
    [Header("Deadzone Settings")]
    [Tooltip("Minimum stick input som accepteras.")]
    [Range(0f, 0.5f)]
    public float deadzone = 0.1f;

    [Header("Response Curve Settings")]
    [Tooltip("Hur stark EaseInOut kurvan är. 1 = normal, högre = mjukare start.")]
    [Range(1f, 5f)]
    public float curveIntensity = 2f;

    [Header("Smoothing Settings")]
    [Tooltip("Smoothing speed (0 = ingen smoothing).")]
    [Range(0f, 30f)]
    public float smoothingSpeed = 0f;

    private Vector2 smoothedValue = Vector2.zero;

    public override Vector2 Process(Vector2 value, InputControl control)
    {
        float magnitude = value.magnitude;

        // Deadzone
        if (magnitude < deadzone)
        {
            smoothedValue = Vector2.zero;
            return Vector2.zero;
        }

        // Normalisera riktning
        Vector2 direction = value.normalized;

        // Ta bort deadzone och skala om
        magnitude = Mathf.InverseLerp(deadzone, 1f, magnitude);

        // Applicera EaseInOut kurva
        magnitude = ApplyEaseInOut(magnitude, curveIntensity);

        // Kombinera riktning och ny magnitud
        Vector2 processedValue = direction * magnitude;

        // Smoothing
        if (smoothingSpeed > 0f)
        {
            smoothedValue = Vector2.Lerp(smoothedValue, processedValue, Time.deltaTime * smoothingSpeed);
            return smoothedValue;
        }
        else
        {
            return processedValue;
        }
    }

    private float ApplyEaseInOut(float t, float intensity)
    {
        // Tvinga clamp
        t = Mathf.Clamp01(t);

        // EaseInOut baserat på intensitet
        t = Mathf.Pow(t, intensity) * (3f - 2f * Mathf.Pow(t, intensity));
        return t;
    }
}

#if UNITY_EDITOR
[UnityEditor.InitializeOnLoad]
#endif
public static class StickProcessorAAARegister
{
    static StickProcessorAAARegister()
    {
        InputSystem.RegisterProcessor<StickResponseCurveProcessor>();
    }
}