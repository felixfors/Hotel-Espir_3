using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class ReflectionUpdater : MonoBehaviour
{
    private Camera targetCamera; // Dra in kameran i Inspector
    public Renderer rend;
    public GameObject[] reflection;

    private int reflectionQuality; 
    void Start()
    {
        targetCamera = Camera.main;
    }

    void Update()
    {
        if(reflectionQuality>0)
        {
            if (IsVisibleFrom(rend, targetCamera))
            {
                if (!reflection[reflectionQuality].activeInHierarchy)
                    reflection[reflectionQuality].SetActive(true);
            }
            else
            {
                if (reflection[reflectionQuality].activeInHierarchy)
                    reflection[reflectionQuality].SetActive(false);
            }
        }       

    }

    bool IsVisibleFrom(Renderer renderer, Camera camera)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }
    public void RealTimeReflection(int qualitySetting)
    {
        reflectionQuality = qualitySetting;
        foreach(GameObject reflectionQuality in reflection)
        {
            reflectionQuality.gameObject.SetActive(false);
        }
        if(qualitySetting >0)
        {
            reflection[qualitySetting].gameObject.SetActive(true);
        }
    }
}
