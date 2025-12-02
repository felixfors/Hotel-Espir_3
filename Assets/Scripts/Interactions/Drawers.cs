using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Drawers : MonoBehaviour
{
    private bool interacting;
    public int priorityLevel;
    public Transform drawerBase;
    private Vector3 drawerPosition;

    public float moveSpeed = 2;
    public float mouseSensitivity = 0.1f;
   
    private Transform player;

    private int forwardDirection;
    private int sideDirection;

    private bool dragging;
    private bool rightSide;
    private bool infront;
    private float forwardDot;
    private float sideDot;

    public bool locked;
    private bool unlocking;
    private float rattleTimer;
    

    public float drawerValue;
    public float maxOpen;

    [Space(20)]
    public float rattleSpeed;
    public float rattleAmplitude;
    public float maxRattlePos;
    private bool rattling;
    private AudioSource audioSource;
    public AudioClip rattleSound;
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        player = PlayerController.instance.transform;
    }
    private void Update()
    {
        if (!locked)
            InteractLogic();
        else
        {
            if(interacting && PlayerController.instance.doorInteractInput.action.WasPressedThisFrame())
            {
                if (locked && !rattling && !unlocking)
                {
                    audioSource.PlayOneShot(rattleSound);
                    rattling = true;
                }
                    
            }
        }
        if (rattling)
        {
            if (unlocking)
                rattling = false;
            Rattle();
        }
    }
    public void Rattle()
    {
        if (unlocking) return;
        var localpos = transform.localPosition;
        float duration = 1;

        rattleTimer += Time.deltaTime;
        float t = rattleTimer / duration;

        if (t < duration)
        {
            localpos.z += Mathf.Sin(Time.time * rattleSpeed) * rattleAmplitude;
            localpos.z = Mathf.Clamp(localpos.z, 0, maxRattlePos);
            transform.localPosition = Vector3.Lerp(localpos, transform.localPosition, t);
        }
        else
        {
            if (transform.localPosition != localpos)
            {
                localpos.z = 0;
                transform.localPosition = Vector3.Lerp(transform.localPosition, localpos, t);
            }
            else
            {
                transform.localPosition = localpos;
                rattleTimer = 0;
                rattling = false;
            }
        }
    }
    private void InteractLogic()
    {
        if (interacting)
        {
            if (PlayerController.instance.doorInteractInput.action.WasPressedThisFrame())
            {
                Interacting_Status(true);
                InteractionDirection();
                dragging = true;
            }
            else if (PlayerController.instance.doorInteractInput.action.WasReleasedThisFrame())
            {
                dragging = false;
                Interacting_Status(false);
            }



        }
        else if (!interacting && dragging)
        {
            if (PlayerController.instance.doorInteractInput.action.WasReleasedThisFrame())
            {
                Interacting_Status(false);
                dragging = false;
            }

        }
        if (dragging && Vector3.Distance(transform.position, player.position) >= InteractManager.instance.interactDistance)
            dragging = false;


        if (dragging)
        {
            InteractionMovement();
        }
        if (transform.localPosition != drawerPosition)
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, drawerPosition, moveSpeed * Time.deltaTime);
    }
    private void InteractionDirection()
    {
        Vector3 directionForward = (player.position - drawerBase.transform.position).normalized;
        forwardDot = Vector3.Dot(drawerBase.transform.forward, directionForward);

        Vector3 directionSideways = (player.position - transform.position).normalized;
        sideDot = Vector3.Dot(drawerBase.transform.right, directionSideways);

        if (forwardDot > 0)
        {
            infront = true;
            forwardDirection = 1;
        }
        else if (forwardDot < 0)
        {
            infront = false;
            forwardDirection = -1;
        }
        if (sideDot > 0)
        {
            rightSide = false;
            sideDirection = -1;
        }
        else if (sideDot < 0)
        {
            rightSide = true;
            sideDirection = 1;
        }       
    }
    private void InteractionMovement()
    {
        Vector2 lookInput = PlayerController.instance.lookInput.action.ReadValue<Vector2>() * mouseSensitivity;
        float mouseY = lookInput.y;
        float mouseX = lookInput.x;

        // Välj den axel med störst absolut rörelse
        if (Mathf.Abs(mouseY) > Mathf.Abs(mouseX))
            drawerValue += mouseY * forwardDirection;
        else
            drawerValue += mouseX * sideDirection;

        drawerValue = Mathf.Clamp(drawerValue, 0, maxOpen);
        drawerPosition = new Vector3(0, 0, drawerValue);
        
    }

    public void SetSavedValue(float _drawerValue)
    {
        drawerValue = _drawerValue;
        drawerPosition = new Vector3(0, 0, drawerValue);
        transform.localPosition = drawerPosition;
    }

    public void Interacting_Status(bool interacting)
    {
        InteractManager.instance.currentlyInteracting = interacting;
        if (interacting)
            InteractManager.instance.PriorityLevel(priorityLevel);
        else
            InteractManager.instance.PriorityLevel(0);
    }
    public void StartInteract()
    {
        interacting = true;
    }
    public void EndInteract()
    {
        interacting = false;
    }
}
