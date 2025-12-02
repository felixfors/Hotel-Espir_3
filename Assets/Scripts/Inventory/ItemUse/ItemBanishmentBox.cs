using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBanishmentBox : MonoBehaviour
{
    public static ItemBanishmentBox instance;
    //[HideInInspector]
    public bool canUseItem;
    [HideInInspector]
    public bool usingItem;
    private bool usingTimerDelay;
    private float usingTimer;
    private Animator anim;
    #region Audio
    public AudioSource CrankAudioSource;
    public AudioSource musicAudioSource;
    public AudioSource banishAudioSource;
    public AudioClip crankSound;
    public AudioClip musicSound;
    public AudioClip banishSound;
    #endregion
    private float spinTimer;
    [Header("How many seconds should we grind the wheel")]
    public float triggerTime;
    private bool coolDown;
    private bool ejectingDisc;

    [HideInInspector]
    public int discs;

    [Space(50)]
    public AnimationCurve crankCurveFishEye;
    public AnimationCurve banishCurveFishEye;
    public Transform inventoryBanishmentboxTransform;
    public MeshRenderer itemMeshRender;
    private Transform banishmentboxParent;

    // Start is called before the first frame update
    void Start()
    {
        banishmentboxParent = transform;
        anim = GetComponent<Animator>();
    }
    private void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (!canUseItem)
        {
            usingItem = false;
            InventoryController.instance.usingItem = false;
        }
        else if (canUseItem)
        {
            
            if (PlayerController.instance.reloadBanishmentboxInput.action.WasPressedThisFrame() && discs <= 0 && InventoryController.instance.banishmentBoxDiscs > 0 && !coolDown) // lägg in en ny skiva
            {
                StartCoroutine(ReloadDisc());
            }
            if(InventoryController.instance.currentItemData.priority >= InteractManager.instance.currentPriority)
            {
                if (PlayerController.instance.itemInteractInput.action.WasPressedThisFrame() && !coolDown)
                {
                    usingTimerDelay = true;
                    
                }
                    
                if (PlayerController.instance.itemInteractInput.action.WasReleasedThisFrame())
                {
                    usingTimerDelay = false;
                    if(usingItem)
                    PlayerAnimator.instance.PlayPlayerAnimation(3, true, "Stop");
                    InventoryController.instance.usingItem = false;
                }

            }
            if(usingTimerDelay)
            {
                if(!usingItem)
                {
                   
                    usingTimer += Time.deltaTime*6;
                    usingTimer = Mathf.Clamp01(usingTimer);
                    if (usingTimer >= 1)
                    {
                        PlayerAnimator.instance.PlayPlayerAnimation(3, true, "Use");
                        usingItem = true;
                    }
                }                                  
            }
            else
            {
                usingItem = false;
                usingTimer = 0;
            }



            

        }
        if (usingItem) // Snurra på hantaget
        {
            Grinding();
            if (!CrankAudioSource.isPlaying)
            {
                CrankAudioSource.PlayOneShot(crankSound);
            }
        }
        else
        {
            if(CrankAudioSource.isPlaying && CrankAudioSource.clip == crankSound)
            {
                CrankAudioSource.Stop();
            }
            if (musicAudioSource.isPlaying)
            {
                if(!coolDown)
                {
                    FishEyeController.instance.StopAnimation();
                    ChromaticController.instance.StopAnimation();
                    ResetVFX();
                }               
                musicAudioSource.Stop();
            }
                
        }
            
    }
    private void Grinding()
    {
        if (discs <= 0) // om vi inte har någon skiva så ska vi inte kunna förbruka något
            return;

        if (spinTimer < triggerTime )
        {                           
            spinTimer += Time.deltaTime;
            if (!musicAudioSource.isPlaying && spinTimer > 1)
            {
                FishEyeController.instance.StopAnimation();
                ChromaticController.instance.StopAnimation();

                float startValue_Chrome = ChromaticController.instance.vfxLerpValue;
                ChromaticController.instance.StartCoroutine(ChromaticController.instance.VolumeVFX(startValue_Chrome, 1, 2, crankCurveFishEye));

                float startValue_FishEye = FishEyeController.instance.vfxLerpValue;
                FishEyeController.instance.StartCoroutine(FishEyeController.instance.VolumeVFX(startValue_FishEye, 0.75f, triggerTime-1, crankCurveFishEye));
                musicAudioSource.PlayOneShot(musicSound);
            }              
        }           
        else if(spinTimer >= triggerTime) // skivan är förbrukad
        {
            usingItem = false;
            //FishEyeController.instance.StopAnimation();
            //ChromaticController.instance.StopAnimation();
            Debug.Log("Nu så ska det våga");
            float startValue_FishEye = FishEyeController.instance.vfxLerpValue;
            FishEyeController.instance.StartCoroutine(FishEyeController.instance.VolumeVFX(startValue_FishEye, 0, 1f, banishCurveFishEye));

            float startValue_Chrome = ChromaticController.instance.vfxLerpValue;
            ChromaticController.instance.StartCoroutine(ChromaticController.instance.VolumeVFX(startValue_Chrome, 0, 1, banishCurveFishEye));

            banishAudioSource.PlayOneShot(banishSound);
            StartCoroutine(RemoveDisc());
            EnemyController.instance.TryBanish();
            discs--;
            spinTimer = 0;
        }
    }
    IEnumerator RemoveDisc()
    {
        coolDown = true;
        InventoryController.instance.canUseInventory = false;
        PlayerAnimator.instance.PlayPlayerAnimation(3, true, "EmptyBanBox");
        //yield return new WaitForSeconds(1);
        //ResetVFX();
        
        yield return new WaitForSeconds(2.5f);
        InventoryController.instance.canUseInventory = true;
        coolDown = false;
    }
    IEnumerator ReloadDisc()
    {
        coolDown = true;
        InventoryController.instance.canUseInventory = false;
        PlayerAnimator.instance.PlayPlayerAnimation(3, true, "ReloadBanBox");
        //RemoveDisc
        yield return new WaitForSeconds(2);
        discs++; // lägg in ny skiva
        InventoryController.instance.banishmentBoxDiscs--; // ta bort skiva ifrån inventory
        coolDown = false;
        InventoryController.instance.canUseInventory = true;

    }
    private void ResetVFX()
    {
        float startValue_FishEye = FishEyeController.instance.vfxLerpValue;
        FishEyeController.instance.StartCoroutine(FishEyeController.instance.VolumeVFX(startValue_FishEye, 0, 1, crankCurveFishEye));

        float startValue_Chrome = ChromaticController.instance.vfxLerpValue;
        ChromaticController.instance.StartCoroutine(ChromaticController.instance.VolumeVFX(startValue_Chrome, 0, 1, banishCurveFishEye));
    }
    public void UseItem(ItemData itemData)
    {

        
        canUseItem = true;
        spinTimer = 0;
    }
    public void UnUseItem()
    {
        StartCoroutine(HandleMeshRender());// gör så att world item syns igen efter vi kastar bort den
        if (usingItem)
            PlayerAnimator.instance.PlayPlayerAnimation(3, true, "Stop");
        spinTimer = 0;
        usingTimerDelay = false;
        usingTimer = 0;
        canUseItem = false;
        usingItem = false;
        InventoryController.instance.usingItem = false;
        FishEyeController.instance.StopAnimation();
        ChromaticController.instance.StopAnimation();
        ResetVFX();
    }
    private IEnumerator HandleMeshRender() // gör så att world item syns igen efter vi kastar bort den
    {
        bool inInventory = banishmentboxParent.parent == InventoryController.instance.inventoryParent;
        float delay = inInventory? 1 : 0;
        Debug.Log(" finns i inventory" + inInventory);
        yield return new WaitForSeconds(delay);
        itemMeshRender.enabled = true;
    }

    public void PickedUp()
    {
        canUseItem = true;
        Invoke(nameof(DelayedPosition), 0.2f);
        itemMeshRender.enabled = false; // gör så att world item inte syns samtidigt som den animerade lådan
    }
    private void DelayedPosition()
    {
        banishmentboxParent.position = inventoryBanishmentboxTransform.position;
        banishmentboxParent.rotation = inventoryBanishmentboxTransform.rotation;
    }

}
