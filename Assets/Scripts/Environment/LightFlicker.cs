using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
public class LightFlicker : MonoBehaviour
{

    public float intensityRange;
    [Header("Dimming duration in sec")]
    public Vector2 speed;

    private bool dimming;
    private float timer;
    [Header("Time between next lightChange")]
    public Vector2 intervalls;

    public AnimationCurve animCurve;
    HDAdditionalLightData lightsource;
    float baseValue;
    float currentIntesity;
    // Start is called before the first frame update
    void Start()
    {
        lightsource = GetComponent<HDAdditionalLightData>();
        baseValue = lightsource.intensity;
        currentIntesity = baseValue;
    }

    // Update is called once per frame
    void Update()
    {
        if(!dimming)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
                StartCoroutine(ChangeIntensity());
        }
    }


    IEnumerator ChangeIntensity()
    {
        dimming = true;
        float timeElapsed = 0;
        float _speed = Random.Range(speed.x, speed.y);
        float start = currentIntesity;
        float end = Random.Range(baseValue - intensityRange, baseValue + intensityRange);
        while (timeElapsed < _speed)
        {
            float t = timeElapsed / _speed;
            t = animCurve.Evaluate(t);

            currentIntesity = Mathf.Lerp(start, end, t);
            lightsource.intensity = currentIntesity;

            timeElapsed += Time.deltaTime;

            yield return null;
        }
        timer = Random.Range(intervalls.x, intervalls.y);

        currentIntesity = end;
        dimming = false;
    }
}
