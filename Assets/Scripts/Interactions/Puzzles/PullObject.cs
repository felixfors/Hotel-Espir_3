using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class PullObject : MonoBehaviour
{
    public EventData eventData;
    private Transform player;
    public bool canInteract;
    private bool interacting;
    private bool dragging;
    
    public Transform dragObject;
    public Vector3 startPos;
    public Vector3 endPos;

    [SerializeField] private float dragSpeed = 1f;              // Basfart längs sträckan
    [SerializeField] private float shoveCyclesPerSecond = 1.5f; // Hur många vågor per sekund vid full input
    [SerializeField] private AnimationCurve shoveCurve;         // 0 -> 1 -> 0, en putt

    [SerializeField] private float shoveCurveValue;
    private float progress01 = 0f; // 0 = startPos, 1 = endPos
    private float shoveTime = 0f;  // Tid/phase för vågen
    private float prevShoveFactor = 0f; // för att hitta högsta punkten (när damian ska triggas) 
    private bool hasPingedThisCycle = false; // för att hitta högsta punkten (när damian ska triggas) 

    [SerializeField] private AudioSource dragAudioSource;
    [SerializeField] private float maxVolume = 0.8f;
    [SerializeField] private AudioSource damianVoiceSource;

    private bool oiledUp; // sparas i save genom item som används
    private bool damianWarning;
    private bool damianTriggered;

    [Range(0f, 1f)]
    public float posValue;

    public GameObject dragHud;

    

    public Transform damianSpawn;
    public Transform playerDragPosition;
    private float followSpeedMulitplier;

    

    public InputSprite inputSprite;

    [Space(50)]
    public CinemachineVirtualCamera cameraPos;
    public Animator armAnimator;
    public GameObject armObj;
    private Vector2 cameraRot;
    public float lookSpeed;
    public Vector2 lookLimit;
    public Transform pullCamera;
    // Start is called before the first frame update
    void Start()
    {
        player = PlayerController.instance.transform;
        
        GetSavedData();
    }
    private void GetSavedData()
    {
        progress01 = eventData.floatValue; // hämta sparning om det finns
        dragObject.localPosition = Vector3.Lerp(startPos, endPos, progress01);
        if (eventData.triggered)
            damianWarning = true;
    }
    // Update is called once per frame
    void Update()
    {
        if(canInteract && PlayerController.instance.itemInteractInput.action.WasPressedThisFrame() && !interacting)
        {
            PlayerAnimator.instance.ShowHands(false);
            StartDrag();
        }

        if (dragging)
        {
            DragObject();
            CameraLook();

            if (PlayerController.instance.throwInput.action.WasPressedThisFrame())
                EndDrag();
            
            if (progress01 >= 0.03f && !damianWarning && !oiledUp)
            {
                damianVoiceSource.Play();
                eventData.triggered = true;
                damianWarning = true;
            }
            if (progress01 >= 0.25f)
                damianTriggered = true;

            if (damianWarning && damianTriggered && !oiledUp)
            {
                if (prevShoveFactor < shoveCurveValue) // det är lägre
                {
                    prevShoveFactor = shoveCurveValue;
                }
                else if (prevShoveFactor > shoveCurveValue && !hasPingedThisCycle) // här vänder det
                {
                    if(Vector3.Distance(player.position, EnemyController.instance.transform.position) >=10)
                        EnemyController.instance.agent.Warp(damianSpawn.position);
                    EnemyController.instance.DirectSearch(player);
                    hasPingedThisCycle = true;
                }
                else if (hasPingedThisCycle && shoveCurveValue <= 0.1f)
                    hasPingedThisCycle = false;
                prevShoveFactor = shoveCurveValue;
            }

            if (oiledUp && maxVolume != 0.1f)
                maxVolume = 0.1f;
            else if(!oiledUp && maxVolume != 1)
                maxVolume = 1;

            eventData.floatValue = progress01;
        }
        
    }
    private void CameraLook()
    {
        Vector2 lookInput = PlayerController.instance.lookInput.action.ReadValue<Vector2>();
        float sens = PlayerController.instance.lookSpeed * DetectController.instance.currentIntensitivity;

        // Pitch (upp/ner)
        cameraRot.x -= lookInput.y * sens;
        cameraRot.x = Mathf.Clamp(cameraRot.x, -lookLimit.x, lookLimit.x);

        // Yaw (vänster/höger) – begränsad
        cameraRot.y += lookInput.x * sens;
        cameraRot.y = Mathf.Clamp(cameraRot.y, -lookLimit.y, lookLimit.y);

        // Endast lokal rotation = begränsad "look around"
        pullCamera.localRotation = Quaternion.Euler(cameraRot.x, cameraRot.y, 0);
    }
    private void StartDrag()
    {
        EnemyAttack.onAttacked += EndDrag; // bara subba när vi behöver
        PlayerController.instance.canMove = false;
        StartCoroutine(CheckReadyDistance());
        armAnimator.SetTrigger("Start");
        gameObject.layer = 0;
        dragging = true;
        followSpeedMulitplier = 1;
        Invoke("ChangeFollowSpeed",1);
        cameraPos.Priority = 2;
        dragHud.SetActive(true);
        PlayerController.instance.canLook = false;
        PlayerController.instance.physicsDrag = true;
        InventoryController.instance.canUseInventory = false;              
    }
    private void EndDrag()
    {
        
        gameObject.layer = 9;
        dragAudioSource.Stop();
        dragging = false;
        CancelInvoke("ChangeCamera");
        followSpeedMulitplier = 1;
      
        dragHud.SetActive(false);
        
        armAnimator.SetTrigger("End");
        StartCoroutine(EndDragCoroutine());
        EnemyAttack.onAttacked -= EndDrag; // bara subba när vi behöver
    }
    private IEnumerator EndDragCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        PlayerAnimator.instance.ShowHands(true);
        cameraPos.Priority = 0;
        armObj.SetActive(false);
        PlayerController.instance.canMove = true;
        PlayerController.instance.canLook = true;
        PlayerController.instance.physicsDrag = false;
        InventoryController.instance.canUseInventory = true;
    }
    private IEnumerator CheckReadyDistance()
    {
        while (true)
        {
            float distance = Vector3.Distance(PlayerController.instance.transform.position, playerDragPosition.position);

            if (distance < 0.5f)
            {
                armObj.SetActive(true);
                PlayerController.instance.canMove = true;
                yield break; // Stoppar coroutinen
            }
            // Vänta en frame innan nästa kontroll
            yield return null;
        }
    }
    private void ChangeFollowSpeed()
    {
        if (dragging)
            followSpeedMulitplier = 10;
    }
    private void DragObject()
    {
        // --- Flytta & rotera spelaren som innan ---
        PlayerController.instance.transform.position = Vector3.Lerp(
            PlayerController.instance.transform.position,
            playerDragPosition.position,
            Time.deltaTime * 2f * followSpeedMulitplier);

        PlayerController.instance.transform.rotation = Quaternion.Lerp(
            PlayerController.instance.transform.rotation,
            playerDragPosition.rotation,
            Time.deltaTime * 2f * followSpeedMulitplier);

        // --- Vågig rörelse för lådan + ljud ---
        Vector2 move = PlayerController.instance.moveInput.action.ReadValue<Vector2>();

        // Invertera om rörelsen känns baklänges (vänd tecken vid behov)
        float moveInput = -move.y; // testa utan minus om du vill
        moveInput = Mathf.Clamp(moveInput, -1f, 1f);

        float inputMagnitude = Mathf.Abs(moveInput);
        float targetVolume = 0f;

        if (inputMagnitude > 0.01f)
        {
            // Ticka phase för vågen (loopar mellan 0–1)
            shoveTime += Time.deltaTime * shoveCyclesPerSecond * inputMagnitude;
            float phase = Mathf.Repeat(shoveTime, 1f);

            // Kurvans aktuella värde (0→1→0)
            float shoveFactor = shoveCurve != null ? shoveCurve.Evaluate(phase) : 1f;

            // Debug/inspector
            shoveCurveValue = shoveFactor;

            // Rörelsehastighet med vågig känsla
            float signedSpeed = dragSpeed * moveInput * shoveFactor;

            // Uppdatera progress längs sträckan
            progress01 += signedSpeed * Time.deltaTime;
            progress01 = Mathf.Clamp01(progress01);

            // Volym = inputstyrka * kurvtoppar * maxVolume
            targetVolume = inputMagnitude * shoveFactor * maxVolume;
        }
        else
        {
            shoveCurveValue = 0f;
        }

        // Mjuk övergång för volymen
        if (dragAudioSource != null)
        {
            dragAudioSource.volume = Mathf.Lerp(
                dragAudioSource.volume,
                targetVolume,
                Time.deltaTime * 10f
            );

            // Starta/stoppa beroende på volym
            if (dragAudioSource.volume > 0.01f && !dragAudioSource.isPlaying)
            {
                dragAudioSource.Play();
            }
            else if (dragAudioSource.volume <= 0.01f && dragAudioSource.isPlaying)
            {
                dragAudioSource.Stop();
            }
        }
        float moveY = PlayerController.instance.moveInput.action.ReadValue<Vector2>().y;
        armAnimator.SetFloat("Direction", moveY);
        armAnimator.SetBool("Moving",moveY != 0);
        // Sätt lådans position
        dragObject.localPosition = Vector3.Lerp(startPos, endPos, progress01);
    }

    public void OiledUp()
    {
        oiledUp = true;
    }
    public void StartInteract()
    {
        if (!canInteract)
            canInteract = true;
    }
    public void EndInteract()
    {
        if (canInteract)
            canInteract = false;
    }
}
