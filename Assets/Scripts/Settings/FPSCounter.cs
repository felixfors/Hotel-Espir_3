using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    private bool showFPS;
    public CanvasGroup fpsCanvas;
    public TextMeshProUGUI fpsText;// UI Text för att visa FPS

    private float deltaTime = 0.0f;
    private float totalTime = 0.0f;
    private int frameCount = 0;
    private float averageFPS = 0.0f;
    private float updateInterval = 1.0f; // Uppdatera varje sekund

    void Update()
    {
        if (!showFPS)
            return;

        // Beräkna aktuell FPS
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;

        // Samla in data för genomsnittlig FPS
        totalTime += Time.unscaledDeltaTime;
        frameCount++;

        // Uppdatera genomsnittlig FPS varje sekund
        if (totalTime >= updateInterval)
        {
            averageFPS = frameCount / totalTime;
            totalTime = 0;
            frameCount = 0;
        }

        // Uppdatera UI-text
        if (fpsText != null)
        {
            fpsText.text = $"FPS: {fps:F0}\nAvg: {averageFPS:F1}";
        }
    }
    public void DisplayFPS_Status(bool FPS_Status)
    {
        showFPS = FPS_Status;
        fpsCanvas.alpha = showFPS? 1 : 0;
    }
}
