using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragObject : MonoBehaviour
{
    private Transform player;
    private bool interacting;
    private bool dragging;

    public float dragLoudnessSensetivity = 1;
    [SerializeField] private float acceleration = 20f;
    public Vector3 centralOfGravity;
    private Rigidbody rb;

    private Vector2 moveInput;

    public AudioSource audioSource;
    public AudioClip squeekyWheelsSound;
    public float test;
    private void Start()
    {
        player = PlayerController.instance.transform;
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centralOfGravity;
    }
    void Update()
    {
        if (interacting)
        {
            dragging = PlayerController.instance.itemInteractInput.action.IsPressed(); // aktivera om vi siktar och klickar
        }
        else if(dragging && !interacting && !PlayerController.instance.itemInteractInput.action.IsPressed()) // avaktivera om vi inte siktar och slutar klicka
        {
            dragging = false;
        }
        else if(dragging && Vector3.Distance(transform.position,player.position) >= InteractManager.instance.interactDistance) // avaktivera om vi går för långt bort
            dragging = false;

        PlayerController.instance.physicsDrag = dragging;
        if (rb.velocity.magnitude > 0.1f)
            Loudness();
        
        SFX();
    }

    private void SFX()
    {
        float loudness = rb.velocity.magnitude * dragLoudnessSensetivity;

        float volume = Mathf.Clamp01(loudness);
        audioSource.volume = volume;
        
        if (!audioSource.isPlaying)
        {
            if(volume > 0.1f)
                audioSource.PlayOneShot(squeekyWheelsSound);
        }
        else
        {
            if (volume <=0)
                audioSource.Stop();
        }
            
    }
    private void Loudness()
    {
        float loudness = rb.velocity.magnitude * dragLoudnessSensetivity;
        loudness = Mathf.Clamp(loudness,0, EnemyController.instance.hearingDistance);
        EnemyController.instance.SoundImpact(loudness,transform);
        PlayerAudioDetection.instance.SoundImpact(loudness);
     
    }
    void FixedUpdate()
    {
        if (dragging)
        {
            Dragging();
            Rotate();
        }                             
    }
    private void Dragging()
    {
        moveInput = PlayerController.instance.moveInput.action.ReadValue<Vector2>();

        // Läs spelarens rotation (den som interagerar)
        Transform playerTransform = PlayerController.instance.transform;

        // Gör om moveInput till en riktning i världen baserat på spelarens rotation
        Vector3 moveDir = playerTransform.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y));

        // Applicera kraft i den riktningen
        rb.AddForce(moveDir * acceleration, ForceMode.Acceleration);
    }
    private void Rotate()
    {
        // Läs musens X-rörelse
        float mouseX = PlayerController.instance.lookInput.action.ReadValue<Vector2>().x;

        // Om musen inte rör sig – gör inget
        if (Mathf.Abs(mouseX) < 0.01f)
            return;

        // Bestäm riktningen (vänster eller höger)
        Vector3 targetDir = mouseX > 0 ? player.transform.right : -player.transform.right;

        // Platta till riktningen (bara Y-rotation)
        targetDir.y = 0f;
        targetDir.Normalize();

        // Beräkna målrotationen – lådan tittar åt spelarens höger/vänster
        Quaternion targetRotation = Quaternion.LookRotation(targetDir, Vector3.up);

        // Mjuk övergång
        float rotateSpeed = 0.5f;
        Quaternion newRotation = Quaternion.Slerp(rb.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime);

        // Rotera lådan med fysik
        rb.MoveRotation(newRotation);
    }
    public void Adjustloudness(float newLoudness)
    {
        dragLoudnessSensetivity = newLoudness;
    }
    public void StartInteract()
    {
        interacting = true;
    }
    public void EndInteract()
    {
        interacting = false;
    }
    private void OnDrawGizmos()
    {
        // om du vill se den även i editorn innan spelet körs:
        Gizmos.color = Color.yellow;

        // räkna om lokal position till världsposition
        Vector3 worldCenter = transform.TransformPoint(centralOfGravity);

        Gizmos.DrawSphere(worldCenter, 0.1f);

        // om du vill rita en linje från objektets mitt till CoM
        Gizmos.DrawLine(transform.position, worldCenter);
    }
}
