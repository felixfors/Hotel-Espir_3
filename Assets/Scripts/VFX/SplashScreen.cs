using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashScreen : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float target;
    public float delay;
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        StartCoroutine(FadeCanvas(target,delay,speed));
    }
    public IEnumerator FadeCanvas(float target, float delay, float speed)
    {
        if(delay >0)
        yield return new WaitForSeconds(delay);

        // Om canvasGroup är null, hämta det automatiskt från samma GameObject
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (Mathf.Abs(canvasGroup.alpha - target) > 0.01f)
        {
            time += Time.deltaTime * speed;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, target, time);
            yield return null;
        }

        canvasGroup.alpha = target; // säkerställ att vi hamnar exakt på target
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}
