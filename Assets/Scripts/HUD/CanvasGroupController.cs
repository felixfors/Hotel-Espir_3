using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasGroupController : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve valueCurve;
    [SerializeField]
    private AnimationCurve speedCurve;    
    private float procent;

    //public float durationValue;
    //public float targetValue;

    public bool test;
    public CanvasGroup canvas;
    private void Awake()
    {
        canvas.alpha = 0;
    }
    private void Update()
    {      
        //if(test)
       // {
         //   StartCoroutine(Fade(targetValue, durationValue));
         //   test = false;
       // }
    }

    public IEnumerator Fade(float _targetValue, float _duration)
    {

        float timeElapsed = 0;
        while (timeElapsed < _duration)
        {
            float t = timeElapsed / _duration;
            t = speedCurve.Evaluate(t);

            procent = Mathf.Lerp(procent, _targetValue, t);
            canvas.alpha = Mathf.Lerp(0, 1, valueCurve.Evaluate(procent));
            timeElapsed += Time.deltaTime;

            yield return null;
        }
        procent = _targetValue;
    }

}
