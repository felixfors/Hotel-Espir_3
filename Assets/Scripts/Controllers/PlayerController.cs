using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    private PlayerInput playerInput;

    [HideInInspector]
    public CharacterController characterController;

    public WalkingStates walkingState;
    public enum WalkingStates
    {
        idle,sneak, crouch, walk, run, dragging
    }
    public int health = 1;
    public Transform playerCamera;
    //[HideInInspector]
    public float movementSpeed;
    public float crouchSpeed = 2f;
    public float sneakSpeed = 3f;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    private float gravity = 10f;
    public bool isRunning;
    public bool isCrouching;
    public bool isSneaking;

    //Crouch variables
    private float timeToCrouch = 5f;
    private float standingHeight = 1.86f;
    private float crouchHeight = 1f;


    [HideInInspector]
    public bool isGrounded;
    [HideInInspector]
    public bool isMoving;
    //[HideInInspector]
    public bool physicsDrag; // aktiveras om vi släpar en rigidbody

    public Vector2 inputSpeed;

    public InventorySway inventorySway;
    public Vector2 currentLookSpeed;
    public float lookSpeed = 2f;
    public float lookSpeedGamepadMultiplier;
    [HideInInspector]
    public float lookSpeedMultiplier = 1;
    public float lookXLimit = 45f;


    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    public bool canMove = true;
    public bool canLook = true;
    private int cursorCount = 0;
    [Space(50)]
    private bool canZoom;
    private float zoomValue;
    public float zoomSpeed = 1;
    private float zoomVelocity; // behövs för SmoothDamp
    public Vector2 minMaxZoom;

    [Space(50)]
    public float amplitude_Target;
    public float frequency_Target;
    public float amplitude;
    public float frequency;
    public CinemachineVirtualCamera vcam;
    public CinemachineVirtualCamera groundCamera;
    public NoiseSettings[] cameraStates;
    public CinemachineImpulseSource jumpImpulse;

    //Inputs
    [Space(50)]
    public InputActionReference moveInput;
    public InputActionReference runInput;
    public InputActionReference crouchInput;
    public InputActionReference sneakInput;
    public InputActionReference jumpInput;
    public InputActionReference holdBreathInput;
    public InputActionReference lookInput;
    public InputActionReference zoomInput;
    public InputActionReference leanInput;
    public InputActionReference reloadBanishmentboxInput;
    public InputActionReference flashlightInput;
    public InputActionReference itemInteractInput;
    public InputActionReference doorInteractInput;
    public InputActionReference useItemInput;
    public InputActionReference journalInput;
    public InputActionReference journalCategory;
    public InputActionReference cancelAction;
    public InputActionReference scrollWheel;
    public InputActionReference throwInput;
    public InputActionReference menuInput;
    public InputActionReference menuSkip_static;

    private void Awake()
    {
        instance = this;
        playerInput = GetComponent<PlayerInput>();
    }
    void Start()
    {
        lookSpeedMultiplier = 1;
        
        characterController = GetComponent<CharacterController>();
        MouseController(0); // gör så vi inte kan se musen när vi startar
        InventoryController.instance.canUseInventory = true; // vi kan använda inventoryn när vi spawnar
        zoomValue = minMaxZoom.x;
        canZoom = true;
        
    }
    void Update()
    {
        PlayerMovement();
        PlayerWalkingStates();
        Cinemachine();
        IsGrounded();
        CameraZoom();

        if(canMove)
        {
            Crouch();
            Leaning();
        }
        bool runningWithGamepad = DetectController.instance.inputType == DetectController.InputType.gamepad & isRunning;
        
        Vector2 movementInput = lookInput.action.ReadValue<Vector2>();
        Vector2Int int_movementInput = new Vector2Int();

        if (runningWithGamepad)
        {
            int_movementInput = new Vector2Int(Mathf.RoundToInt(Mathf.Sign(movementInput.x)), Mathf.RoundToInt(Mathf.Sign(movementInput.y)));
        }
       inputSpeed = runningWithGamepad? int_movementInput : movementInput;
        
    }

    private void IsGrounded()
    {
        int layerPlayer = 1 << 3;
        layerPlayer = ~layerPlayer;
        Debug.DrawRay(transform.position + new Vector3(0, 1, 0), Vector3.down * 1.05f, Color.red);
        if (Physics.Raycast(transform.position + new Vector3(0,1,0), Vector3.down, out RaycastHit hit, 1.05f, layerPlayer))
        {
            if(!isGrounded)
            {
                PlayerAudio.instance.jumpImpact = true;
                DetectController.instance.RumblePulse(0.1f, 0.2f, 0.2f);
                jumpImpulse.GenerateImpulse(transform.forward);
                isGrounded = true;
            }
        }
        else
        {
            if (isGrounded)
            {
                isGrounded = false;
            }
        }
    }
    private void PlayerWalkingStates()
    {
        if (moveDirection.x == 0 && moveDirection.z == 0)
            isMoving = false;
        else
            isMoving = true;

        if (isGrounded)
        {
            if(physicsDrag)
            {
                isRunning = false;
                if(crouchInput.action.IsPressed())
                {
                    walkingState = WalkingStates.crouch;
                    movementSpeed = crouchSpeed;
                    isCrouching = true;
                    isSneaking = false;
                }
                else
                {
                    walkingState = WalkingStates.sneak;
                    movementSpeed = sneakSpeed;
                    isSneaking = true;
                    isCrouching = false;
                }             
            }
            if (runInput.action.IsPressed() && !isCrouching && !isSneaking & !physicsDrag)
            {
                walkingState = WalkingStates.run;
                movementSpeed = runSpeed;
                isRunning = true;
                isCrouching = false;
                isSneaking = false;
            }
            else if (!runInput.action.IsPressed()) isRunning = false;

            if (crouchInput.action.IsPressed() && !isRunning && !isSneaking & !physicsDrag)
            {
                walkingState = WalkingStates.crouch;
                movementSpeed = crouchSpeed;
                isCrouching = true;
                isRunning = false;
                isSneaking = false;
            }
            else if (!crouchInput.action.IsPressed()) isCrouching = false;

            if(DetectController.instance.inputType == DetectController.InputType.keyboard)
            {
                if (sneakInput.action.IsPressed() && !isRunning && !isCrouching & !physicsDrag)
                {
                    walkingState = WalkingStates.sneak;
                    movementSpeed = sneakSpeed;
                    isSneaking = true;
                    isRunning = false;
                    isCrouching = false;
                }
                else if (!sneakInput.action.IsPressed()) isSneaking = false;
            }
            else if(DetectController.instance.inputType == DetectController.InputType.gamepad && !isRunning && !isCrouching & !physicsDrag)
            {
                Vector2 _move = moveInput.action.ReadValue<Vector2>();
                if(!isMoving)
                {
                    walkingState = WalkingStates.idle;
                    isSneaking = false;
                }
                else if (_move.magnitude >0.5f)
                {
                    walkingState = WalkingStates.walk;
                    isSneaking = false;
                }
                else if(_move.magnitude <0.5f)
                {
                    walkingState = WalkingStates.sneak;
                    isSneaking = true;
                }               
            }


            if (isMoving && !isSneaking && !isCrouching && !isRunning & !physicsDrag)
            {
                walkingState = WalkingStates.walk;
                movementSpeed = walkSpeed;
                isSneaking = false;
                isRunning = false;
                isCrouching = false;
            }
            else if(!isMoving)
            {
                if(!isCrouching && !isSneaking)
                {
                    movementSpeed = walkSpeed;
                    walkingState = WalkingStates.idle;
                }                  
                isRunning = false;
            }
        }

            
        
    }
    private void Crouch()
    {
        float crouchedHeight = isCrouching ? crouchHeight : standingHeight;

        if (characterController.height != crouchedHeight)
        {
            AdjustController(crouchedHeight);

            Vector3 camPosition = playerCamera.transform.localPosition;
            camPosition.y = characterController.height;

            playerCamera.transform.localPosition = camPosition;
        }
    }
    private void AdjustController(float height)
    {
        float center = height / 2;

        characterController.height = Mathf.LerpUnclamped(characterController.height, height, timeToCrouch * Time.deltaTime);
        characterController.center = Vector3.LerpUnclamped(characterController.center, new Vector3(0, center, 0), timeToCrouch * Time.deltaTime);
    }

    private void PlayerMovement()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        Vector2 _moveInput = moveInput.action.ReadValue<Vector2>();
        float curSpeedX = canMove ? movementSpeed * _moveInput.x : 0;
        float curSpeedY = canMove ? movementSpeed * _moveInput.y : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedY) + (right * curSpeedX);

        //if (jumpInput.action.WasPressedThisFrame() && canMove && isGrounded)
        //{
            //moveDirection.y = jumpPower;
        //}
        //else
        //{
            moveDirection.y = movementDirectionY;
        //}

        if (!isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        if (canLook)
        {
            Vector2 _lookInput = lookInput.action.ReadValue<Vector2>();
            rotationX += -_lookInput.y * lookSpeed * lookSpeedMultiplier * DetectController.instance.currentIntensitivity;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, _lookInput.x * lookSpeed * lookSpeedMultiplier * DetectController.instance.currentIntensitivity, 0);
        }
        if (canMove)
        {                    
            characterController.Move(moveDirection * Time.deltaTime);
        }
    }
    public void MouseController(int cursorCounter)
    {
        cursorCount += cursorCounter;
        if (cursorCount < 0)  cursorCount = 0;

        bool cursor = cursorCount > 0;

        canLook = cursor? false: true;
        Cursor.lockState = cursor? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = cursor;

        inventorySway.active = cursor ? false : true;
    }
    private void Leaning()
    {

            float _leanInput = canLook? leanInput.action.ReadValue<float>() : 0;
            float dutch = new float();
            float cameraSide = new float();

            dutch += _leanInput * 10 * -1; // tiltar kameran			
            cameraSide = (_leanInput - (-1)) / (1 - (-1)); // flyttar kameran sidleds

            vcam.m_Lens.Dutch = isRunning ? Mathf.Lerp(vcam.m_Lens.Dutch, 0, Time.deltaTime * 5) : Mathf.Lerp(vcam.m_Lens.Dutch, dutch, Time.deltaTime * 5);

            var bodyCamera = vcam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            bodyCamera.CameraSide = isRunning ? Mathf.Lerp(bodyCamera.CameraSide, 0.5f, Time.deltaTime * 5) : Mathf.Lerp(bodyCamera.CameraSide, cameraSide, Time.deltaTime * 5);
    }
    private void Cinemachine()
    {

        amplitude_Target = Mathf.Lerp(amplitude_Target, amplitude, Time.deltaTime*2);
        frequency_Target = Mathf.Lerp(frequency_Target, frequency, Time.deltaTime*2);
        vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = amplitude_Target;
        vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = frequency_Target;
        
        if (walkingState == WalkingStates.idle)//klar
        {
            frequency = 0.25f;
            amplitude = 1f;
        }
        else if (walkingState == WalkingStates.sneak)//klar
        {
            frequency = 0.7f;
            amplitude = 1.5f;
        }
        else if (walkingState == WalkingStates.crouch)//klar
        {
            frequency = 0.5f;
            amplitude = 1f;
        }
        else if(walkingState == WalkingStates.walk)//klar
        {
            frequency = 1;
            amplitude = 1.5f;
        }           
        else if(walkingState == WalkingStates.run) //klar
        {
            frequency = 2.5f;
            amplitude = 1.3f;
        }           
    }

    private void CameraZoom()
    {
        float targetZoom;

        if (canZoom && zoomInput.action.IsPressed())
            targetZoom = minMaxZoom.y;
        else
            targetZoom = minMaxZoom.x;

        zoomValue = Mathf.SmoothDamp(zoomValue, targetZoom, ref zoomVelocity, 0.3f);
        vcam.m_Lens.FieldOfView = zoomValue;
    }
}

