using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    public static PlayerAudio instance;

    public AudioSource walkAudio;
    private float loudnessmultiplier;

    public AudioClip [] jumpSound;
  
    [Space(50)]
    public List<WoodSound> woodSound = new();
    [System.Serializable]
    public class WoodSound
    {
        public AudioClip[] leftFoot;
        public AudioClip[] rightFoot;
        [Range(1, 25)]
        public int loudness;
    }

    public List<ConcreteSound> concreteSound = new();
    [System.Serializable]
    public class ConcreteSound
    {
        public AudioClip[] leftFoot;
        public AudioClip[] rightFoot;
        [Range(1, 25)]
        public int loudness;
    }

    public List<TileSound> tileSound = new();
    [System.Serializable]
    public class TileSound
    {
        public AudioClip[] leftFoot;
        public AudioClip[] rightFoot;
        [Range(1, 25)]
        public int loudness;
    }

    public List<CarpetSound> carpetSound = new();
    [System.Serializable]
    public class CarpetSound
    {
        public AudioClip[] leftFoot;
        public AudioClip[] rightFoot;
        [Range(1, 25)]
        public int loudness;
    }
    public List<CarpetSound> glassSound = new();
    [System.Serializable]
    public class ClassSound
    {
        public AudioClip[] leftFoot;
        public AudioClip[] rightFoot;
        [Range(1, 25)]
        public int loudness;
    }

    private bool leftFoot;
    [HideInInspector]
    public bool jumpImpact;

    public float footStepTimer;
    private float getCurrentOffset;
    public float walkOffset;
    public float sneakOffset;
    public float crouchOffset;
    public float runOffset;
    
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        Handle_FootSteps();
        GetStates();
    }
    private void GetStates()
    {
        
        if (PlayerController.instance.isGrounded)
        {
            if (PlayerController.instance.walkingState == PlayerController.WalkingStates.idle)
            {
                getCurrentOffset = 0.2f;
                loudnessmultiplier = 0;
                footStepTimer = getCurrentOffset;
            }
            else if (PlayerController.instance.walkingState == PlayerController.WalkingStates.sneak)
            {
                walkAudio.volume = 0.1f;
                loudnessmultiplier = 0.2f;
                getCurrentOffset = sneakOffset;
            }
            else if (PlayerController.instance.walkingState == PlayerController.WalkingStates.crouch)
            {
                walkAudio.volume = 0.01f;
                loudnessmultiplier = 0.1f;
                getCurrentOffset = crouchOffset;
            }
            else if (PlayerController.instance.walkingState == PlayerController.WalkingStates.walk)
            {
                walkAudio.volume = 0.5f;
                loudnessmultiplier = 1f;
                getCurrentOffset = walkOffset;
            }
            else if (PlayerController.instance.walkingState == PlayerController.WalkingStates.run)
            {
                walkAudio.volume = 1;
                loudnessmultiplier = 1.5f;
                getCurrentOffset = runOffset;
            }
        }
        else
        {
            walkAudio.volume = 1f;
            loudnessmultiplier = 1;
        }
        
    }
    public void Handle_FootSteps()
    {
        if (!PlayerController.instance.isGrounded) return;
        if (!PlayerController.instance.isMoving && !jumpImpact) return;

        footStepTimer -= Time.deltaTime;
        if (footStepTimer <= 0|| jumpImpact)
        {
            int layerPlayer = 1 << 3;
            layerPlayer = ~layerPlayer;
            if (Physics.Raycast(transform.position + new Vector3(0,1.5f,0), Vector3.down, out RaycastHit hit, 3,layerPlayer))
            {
                switch(hit.collider.tag)
                {

                    case "Footstep/WOOD":
                        PlayerAudioDetection.instance.VolumeIncrease(woodSound[0].loudness, loudnessmultiplier );
                        walkAudio.PlayOneShot(leftFoot? woodSound[0].leftFoot[Random.Range(0, woodSound[0].leftFoot.Length)] : woodSound[0].rightFoot[Random.Range(0, woodSound[0].rightFoot.Length)]);
                        break;
                    case "Footstep/CONCRETE":
                        PlayerAudioDetection.instance.VolumeIncrease(concreteSound[0].loudness, loudnessmultiplier);
                        walkAudio.PlayOneShot(leftFoot ? concreteSound[0].leftFoot[Random.Range(0, concreteSound[0].leftFoot.Length)] : concreteSound[0].rightFoot[Random.Range(0, concreteSound[0].rightFoot.Length)]);
                        break;
                    case "Footstep/TILE":
                        PlayerAudioDetection.instance.VolumeIncrease(tileSound[0].loudness, loudnessmultiplier);
                        walkAudio.PlayOneShot(leftFoot ? tileSound[0].leftFoot[Random.Range(0, tileSound[0].leftFoot.Length)] : tileSound[0].rightFoot[Random.Range(0, tileSound[0].rightFoot.Length)]);
                        break;
                    case "Footstep/CARPET":
                        PlayerAudioDetection.instance.VolumeIncrease(carpetSound[0].loudness, loudnessmultiplier);
                        walkAudio.PlayOneShot(leftFoot ? carpetSound[0].leftFoot[Random.Range(0, carpetSound[0].leftFoot.Length)] : carpetSound[0].rightFoot[Random.Range(0, carpetSound[0].rightFoot.Length)]);
                        break;
                    case "Footstep/GLASS":
                        PlayerAudioDetection.instance.VolumeIncrease(glassSound[0].loudness, 1);// glass ska inte använda loudnessmultiplier utan det ska låta max varje gång
                        walkAudio.PlayOneShot(leftFoot ? glassSound[0].leftFoot[Random.Range(0, glassSound[0].leftFoot.Length)] : glassSound[0].rightFoot[Random.Range(0, glassSound[0].rightFoot.Length)]);
                        break;
                    default:
                        PlayerAudioDetection.instance.VolumeIncrease(woodSound[0].loudness, loudnessmultiplier); 
                        walkAudio.PlayOneShot(leftFoot ? woodSound[0].leftFoot[Random.Range(0, woodSound[0].leftFoot.Length)] : woodSound[0].rightFoot[Random.Range(0, woodSound[0].rightFoot.Length)]);
                        break;
                }
                
            }            
            leftFoot = !leftFoot;
            walkAudio.pitch = Random.Range(0.95f, 1.1f);
            jumpImpact = false;
            footStepTimer = getCurrentOffset;
        }



    }
}
