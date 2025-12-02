using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIFootSteps : MonoBehaviour
{
    public AudioSource walkAudio;

    [Space(50)]
    public List<WoodSound> woodSound = new();
    [System.Serializable]
    public class WoodSound
    {
        public AudioClip[] leftFoot;
        public AudioClip[] rightFoot;
    }

    public List<ConcreteSound> concreteSound = new();
    [System.Serializable]
    public class ConcreteSound
    {
        public AudioClip[] leftFoot;
        public AudioClip[] rightFoot;
    }

    public List<TileSound> tileSound = new();
    [System.Serializable]
    public class TileSound
    {
        public AudioClip[] leftFoot;
        public AudioClip[] rightFoot;
    }

    public List<CarpetSound> carpetSound = new();
    [System.Serializable]
    public class CarpetSound
    {
        public AudioClip[] leftFoot;
        public AudioClip[] rightFoot;
    }

    private bool leftFoot;


    // Update is called once per frame
    void Update()
    {
        GetStates();
    }
    private void GetStates()
    {

        if (EnemyController.instance.walkSpeed == 0)
        {
            walkAudio.volume = 0;
        }
        else if (EnemyController.instance.walkSpeed == 1)
        {
            walkAudio.volume = 0.5f;
        }
        else if (EnemyController.instance.walkSpeed == 2)
        {
            walkAudio.volume = 1f;
        }

    }
    public void Handle_FootSteps()
    {
            int layerPlayer = 1 << 3;
            layerPlayer = ~layerPlayer;
            if (Physics.Raycast(transform.position + new Vector3(0, 1.5f, 0), Vector3.down, out RaycastHit hit, 3, layerPlayer))
            {
                switch (hit.collider.tag)
                {

                    case "Footstep/WOOD":
                        walkAudio.PlayOneShot(leftFoot ? woodSound[0].leftFoot[Random.Range(0, woodSound[0].leftFoot.Length)] : woodSound[0].rightFoot[Random.Range(0, woodSound[0].rightFoot.Length)]);
                        break;
                    case "Footstep/CONCRETE":
                        walkAudio.PlayOneShot(leftFoot ? concreteSound[0].leftFoot[Random.Range(0, concreteSound[0].leftFoot.Length)] : concreteSound[0].rightFoot[Random.Range(0, concreteSound[0].rightFoot.Length)]);
                        break;
                    case "Footstep/TILE":
                         walkAudio.PlayOneShot(leftFoot ? tileSound[0].leftFoot[Random.Range(0, tileSound[0].leftFoot.Length)] : tileSound[0].rightFoot[Random.Range(0, tileSound[0].rightFoot.Length)]);
                        break;
                    case "Footstep/CARPET":
                        
                        walkAudio.PlayOneShot(leftFoot ? carpetSound[0].leftFoot[Random.Range(0, carpetSound[0].leftFoot.Length)] : carpetSound[0].rightFoot[Random.Range(0, carpetSound[0].rightFoot.Length)]);
                        break;
                    default:
                        walkAudio.PlayOneShot(leftFoot ? concreteSound[0].leftFoot[Random.Range(0, concreteSound[0].leftFoot.Length)] : concreteSound[0].rightFoot[Random.Range(0, concreteSound[0].rightFoot.Length)]);
                        break;
                }

            }
            leftFoot = !leftFoot;
            walkAudio.pitch = Random.Range(1f, 1.1f);        
    }
}
