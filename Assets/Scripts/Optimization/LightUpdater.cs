using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class LightUpdater : MonoBehaviour
{
    public ShadowQuality shadowQuality; 
    HDAdditionalLightData lightData;
    Light lightSource;
    private Transform player;
    private float playerDirection;
    private float distance;
    private float updateRate;
    private float timer;





    // Start is called before the first frame update
    void Start()
    {
        lightSource = GetComponent<Light>();
        lightData = GetComponent<HDAdditionalLightData>();
        player = PlayerController.instance.transform;
       
    }

    // Update is called once per frame
    void Update()
    {
        distance = Vector3.Distance(player.position, transform.position);

        Vector3 lightDirection = transform.position - player.position;
        playerDirection = Vector3.Dot(player.forward, lightDirection.normalized);
        

        if (lightSource.shadows != LightShadows.Hard && distance <5 || playerDirection >0) // slå på skuggorna 
        {
            lightSource.shadows = LightShadows.Hard;
        }
        else if (lightSource.shadows != LightShadows.None && distance >5 && playerDirection <0)
        {
            lightSource.shadows = LightShadows.None;
        }

        
        if (!gameObject.activeInHierarchy)
        return;             

        if (distance > shadowQuality.cascade[shadowQuality.cascade.Count-1].maxDistance)
            return;

        for(int i = 0; i < shadowQuality.cascade.Count; i ++)
        {
            if(distance < shadowQuality.cascade[i].maxDistance)
            {
                updateRate = shadowQuality.cascade[i].updateRate;
                break;
            }
        }


        timer -= Time.deltaTime;
        
        if(timer <=0)
        {
            UpdateShadows();
            timer = updateRate;
        }       
    }
    private void UpdateShadows()
    {
        lightData.RequestShadowMapRendering();
    }
}
