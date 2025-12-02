using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class PrintScreen : MonoBehaviour
{
    [Header("Tryck på tangenten för att ta en bild")]
    public KeyCode screenshotKey = KeyCode.F12;

    void Update()
    {
        if (Input.GetKeyDown(screenshotKey))
        {
            TakeScreenshot();
        }
    }

    void TakeScreenshot()
    {
        string folderPath = Path.Combine(Application.dataPath, "Screenshots");

        // Skapa mappen om den inte finns
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string fileName = $"screenshot_{timestamp}.png";
        string fullPath = Path.Combine(folderPath, fileName);

        ScreenCapture.CaptureScreenshot(fullPath);

        Debug.Log($"Screenshot saved to: {fullPath}");
    }
}
