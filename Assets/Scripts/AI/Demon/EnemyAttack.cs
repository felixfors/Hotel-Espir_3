//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;

public class EnemyAttack : MonoBehaviour
{
    public static EnemyAttack instance;
    public static event System.Action onAttacked;

    [HideInInspector]
    public bool jumpScareActive;

    public CinemachineVirtualCamera camera;
    public CinemachineImpulseSource scareImpulse;

    public CanvasGroupController blinkingCanvas;
    
    public Volume vfxVolume;
    public Light enemyLight;
    public AnimationCurve animCurvePostProcessing;
    private float vfxLerpValue;

    public AudioSource attackAudio;
    public AudioClip []attackSound;

    public AudioSource deathDragAudio;
    public AudioClip deathDragSound;
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    public void StartJumpScare() // denna kallas ifrån Chase i EnemyController
    {
        SaveController.instance.SaveGame();
        PlayerController.instance.health -= 1;
        jumpScareActive = true;
        camera.Priority = 10;    
        EnemyController.instance.animator.SetTrigger("Attack");
        scareImpulse.GenerateImpulse(transform.forward);
        StartCoroutine("Attacked");
    }
    public void RepositionGhost()
    {
        Transform _longestDistance = EnemyController.instance.wayPoints[0];
        List<Transform> _positionlist = new();

        float _threshold = 20f;// minimum distance in meters

        for(int i = 0; i < EnemyController.instance.wayPoints.Count; i ++)
        {
            // vi har hittat en punkt längre bort
            if (Vector3.Distance(transform.position, EnemyController.instance.wayPoints[i].position) > Vector3.Distance(transform.position,_longestDistance.position)) 
            {
                if (Vector3.Distance(transform.position, EnemyController.instance.wayPoints[i].position) > _threshold) // Lägg till alla möjliga spawnpoints över x meter
                    _positionlist.Add(EnemyController.instance.wayPoints[i]);

                _longestDistance = EnemyController.instance.wayPoints[i]; // Uppdatera längsta punkten
            }
        }
        Transform _newGhostPosition = _positionlist[Random.Range(0, _positionlist.Count)];
        EnemyController.instance.agent.Warp(_newGhostPosition.position);
        EnemyController.instance.target.position = _newGhostPosition.position;

    } // spawna om ghost
    private void RepositionPlayer()
    {
        Transform _longestDistance = EnemyController.instance.wayPoints[0];
        List<Transform> _positionlist = new();

        float _threshold = 20f;// minimum distance in meters

        for (int i = 0; i < EnemyController.instance.wayPoints.Count; i++)
        {
            // vi har hittat en punkt längre bort
            if (Vector3.Distance(transform.position, EnemyController.instance.wayPoints[i].position) > Vector3.Distance(transform.position, _longestDistance.position))
            {
                if (Vector3.Distance(transform.position, EnemyController.instance.wayPoints[i].position) > _threshold) // Lägg till alla möjliga spawnpoints över x meter
                    _positionlist.Add(EnemyController.instance.wayPoints[i]);

                _longestDistance = EnemyController.instance.wayPoints[i]; // Uppdatera längsta punkten
            }
        }
        Transform _newPlayerPosition = _positionlist[Random.Range(0, _positionlist.Count)];
        PlayerController.instance.transform.position = _newPlayerPosition.position;

    } // spawna om spelare
    IEnumerator Attacked()
    {
        onAttacked?.Invoke(); // detta triggar andra scripts, till exempel om spelaren interaktar med något så ska det avbrytas innan fångsten

        DetectController.instance.RumblePulse(0.2f, 1f, 2f);
        PlayerController.instance.canMove = false;
        InteractManager.instance.canInteract = false;
        InventoryController.instance.canUseInventory = false;
        NoteController.instance.CloseJournal(true);
        if(InventoryController.instance.flashLight.on)
        {
            InventoryController.instance.flashLight.FlashlightOnOff(false); // stäng av flashlight
            InventoryController.instance.flashLight.flashlightAudio.Stop(); // gör så vi inte hör ljudet av att lampan stängs av
        }
        
        InventoryController.instance.currentItem = 0; // byt current item till händerna
        InventoryController.instance.SwapItem(0); // ta fram händerna
        EnemyController.instance.playerIsCaught = true;
        attackAudio.Stop();
        if(attackSound.Length >0)
        attackAudio.PlayOneShot(attackSound[Random.Range(0, attackSound.Length)]); // spela attack ljud
        StartCoroutine(VolumeVFX(vfxVolume.weight, 1, 0.2f)); // sätt på poseprocessing
        enemyLight.enabled = true;
        yield return new WaitForSeconds(2);
        blinkingCanvas.StartCoroutine(blinkingCanvas.Fade(1, 2));   
        
        yield return new WaitForSeconds(2);
        EnemyController.instance.SwitchState(EnemyController.States.patrol);
        deathDragAudio.PlayOneShot(deathDragSound); // ljudeffekten när kroppen drag längst golvet
        enemyLight.enabled = false;
        PlayerController.instance.groundCamera.Priority = 10; // kameran när vi ligger på golver
        if (PlayerController.instance.health < 0)
        {
            MenuController.instance.DeathScreen();
            camera.Priority = 0;
            PlayerController.instance.groundCamera.Priority = 0;
            yield break;
        }          
        camera.Priority = 0;
        yield return new WaitForSeconds(6);
        PlayerController.instance.groundCamera.Priority = 0; // kameran när vi ligger på golver
              
        RepositionGhost();
        RepositionPlayer();
        
        blinkingCanvas.StartCoroutine(blinkingCanvas.Fade(0, 5));
        
        StartCoroutine(VolumeVFX(vfxVolume.weight, 0, 1)); // stäng på poseprocessing             
        EnemyController.instance.lostVision = false; // annars kommer Ghosten direkt gå till search Läge
        EnemyController.instance.playerIsCaught = false;
        EnemyController.instance.freezeGhost = false;
        PlayerController.instance.canMove = true;
        InteractManager.instance.canInteract = true;
        InventoryController.instance.canUseInventory = true;
        
        EnemyController.instance.SwitchState(EnemyController.States.idle);
        jumpScareActive = false;
        SaveController.instance.SaveGame();
    }
    IEnumerator VolumeVFX(float start, float end, float speed) // PostProcessing
    {
        
        float timeElapsed = 0;
        while (timeElapsed < speed)
        {
            float t = timeElapsed / speed;
            t = animCurvePostProcessing.Evaluate(t);

            vfxLerpValue = Mathf.Lerp(start, end, t);
            vfxVolume.weight = vfxLerpValue;
            timeElapsed += Time.deltaTime;

            yield return null;
        }
        vfxLerpValue = end;
    }

}
