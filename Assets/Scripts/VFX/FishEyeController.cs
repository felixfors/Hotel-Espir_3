using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
public class FishEyeController : MonoBehaviour
{
    public static FishEyeController instance;
    public AnimationCurve animCurvePostProcessing;
    public float vfxLerpValue;
    public Volume vfxVolume;
    public bool test;
    public float target;
    public float transitionSpeed;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if(test)
        {
            StartCoroutine(VolumeVFX(vfxLerpValue, target, transitionSpeed, animCurvePostProcessing));
            test = false;
        }
    }
    public void StopAnimation()
    {
        StopAllCoroutines();
    }
    public IEnumerator VolumeVFX(float start, float end, float speed, AnimationCurve curve) // PostProcessing
    {
        float timeElapsed = 0;
        LensDistortion lens;
        
        while (timeElapsed < speed)
        {
            float t = timeElapsed / speed;
            t = curve.Evaluate(t);

            vfxLerpValue = Mathf.Lerp(start, end, t);

            if (vfxVolume.profile.TryGet(out lens))
            {
                ClampedFloatParameter tempFloat = new ClampedFloatParameter(vfxLerpValue, -1f, 1f);
                lens.intensity.SetValue(tempFloat);           
            }

            timeElapsed += Time.deltaTime;

            yield return null;
        }
        vfxLerpValue = end;
        if (vfxVolume.profile.TryGet(out lens))
        {
            ClampedFloatParameter done = new ClampedFloatParameter(end, -1f, 1f);
            lens.intensity.SetValue(done);
        }
    }
    public void ModLensDistortion(bool state)
    {
        Volume volume = GetComponent<Volume>();
        LensDistortion lens;

        if (volume.profile.TryGet(out lens))
        {
            lens.active = state;
        }
    }
}
