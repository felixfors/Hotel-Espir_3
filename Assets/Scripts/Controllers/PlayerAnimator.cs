using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerAnimator : MonoBehaviour
{
    public static PlayerAnimator instance;
    public Animator anim;
    public bool twoHand;
    public int currentAnimation_ID;
    // Start is called before the first frame update
    
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayPlayerAnimation(int animation_ID, bool twoHanded, string triggerString)
    {
        bool wasTwoHanded = twoHand; // gamla vapnet

        twoHand = twoHanded; // uppdatera till nya vapnet

        bool needsDelay = wasTwoHanded && !twoHanded; // bytte från tvåhandat till enhandat

        float duration = twoHanded ? 0.1f : 1;
        float targetWeight = twoHanded ? 1f : 0f;
        DOTween.To(
        () => anim.GetLayerWeight(2),
        x => anim.SetLayerWeight(2, x),
        targetWeight,
        0.3f).SetDelay(needsDelay? 0.5f : 0);

        if (animation_ID != 0) // om siffran inte är 0 så är det en draw animation
            currentAnimation_ID = animation_ID;
        else // siffran är 0 vilket är en withdraw så vi ska kolla upp om vi vilket item som vi ska byta till härnäst
        {
            if (InventoryController.instance.currentItemData)
                currentAnimation_ID = InventoryController.instance.currentItemData.itemData.animationID;
            else
                currentAnimation_ID = 0;
        }
        anim.SetInteger("ID", currentAnimation_ID);
        if (triggerString != "")
            anim.SetTrigger(triggerString);
        else
            anim.SetTrigger("play");
    }
    public void ShowHands(bool show)
    {
        StopAllCoroutines(); // stoppa ev. tidigare pågående lerp
        StartCoroutine(LerpLayerWeight(3, show ? 0f : 1f, 0.3f)); // 0.3 sekunders övergångstid t.ex.
    }
    private IEnumerator LerpLayerWeight(int layerIndex, float targetWeight, float duration)
    {
        float startWeight = anim.GetLayerWeight(layerIndex);
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            float newWeight = Mathf.Lerp(startWeight, targetWeight, t);
            anim.SetLayerWeight(layerIndex, newWeight);
            yield return null;
        }

        // se till att den slutar exakt på målet
        anim.SetLayerWeight(layerIndex, targetWeight);
    }
}
