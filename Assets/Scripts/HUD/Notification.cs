using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Notification : MonoBehaviour
{
    public TextMeshProUGUI notificationText;
    public CanvasGroup canvasGroup;
    public int disableTimer;

    public float fadeDuration;
    private float lerpedValue;
    public AnimationCurve animCurve;

    public void SetText(string newText)
    {
        notificationText.text = newText;
        canvasGroup.alpha = 1;
        Invoke("DisableNotification", 4);
    }
    private void DisableNotification()
    {
        StartCoroutine(DisableNotification(1, 0));
    }
    IEnumerator DisableNotification(float start, float end)
    {
        float timeElapsed = 0;
        while (timeElapsed < fadeDuration)
        {
            float t = timeElapsed / fadeDuration;
            t = animCurve.Evaluate(t);

            lerpedValue = Mathf.Lerp(start, end, t);
            canvasGroup.alpha = lerpedValue;
            timeElapsed += Time.deltaTime;

            yield return null;
        }
        lerpedValue = end;
        canvasGroup.alpha = end;
    }


}
