using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySway : MonoBehaviour
{
    public CharacterController charactercontroller;
    public PlayerController mover;

    [HideInInspector]
    public bool active;

    [Header("Sway")]
    public float step = 0.01f;
    public float maxStepDistance = 0.06f;
    [HideInInspector]
    public Vector3 swayPos;

    [Header("Sway Rotation")]
    public float rotationStep = 4f;
    public float maxRotationStep = 5f;
    [HideInInspector]
    public Vector3 swayEulerRot;

    public float smooth = 10f;
    [HideInInspector]
    public float smoothRot = 12f;

    [Header("Bobbing")]
    public float speedCurve;
    public float curveSin { get => Mathf.Sin(speedCurve); }
    public float curveCos { get => Mathf.Cos(speedCurve); }

    public Vector3 travelLimit = Vector3.one * 0.025f;
    public Vector3 bobLimit = Vector3.one * 0.01f;
    [HideInInspector]
    public Vector3 bobPosition;

    public float bobExaggeration;
    public float idleBreath;

    [Header("Bob Rotation")]
    public Vector3 multiplier;
    [HideInInspector]
    public Vector3 bobEulerRotation;

    public Vector3 velocity;

    [Header("Offset")]
    public Vector3 pivotOffset = new Vector3(0, 1.7f, 0); // Justera efter din modell

    Vector2 walkInput;
    Vector2 lookInput;

    void Start()
    {

    }

    void LateUpdate()
    {
        if (!PlayerController.instance.canMove)
            return;

        
        GetInput();
        VelocityCalc();
        if(active)
        {
            Sway();
            SwayRotation();
        }
       
        BobOffset();
        BobRotation();
        CompositePositionRotation();
    }

    void GetInput()
    {
        Vector2 moveInput = PlayerController.instance.moveInput.action.ReadValue<Vector2>();
        walkInput.x = moveInput.x;
        walkInput.y = moveInput.y;
        walkInput = walkInput.normalized;

        if(active)
        {
            Vector2 mouseInput = PlayerController.instance.lookInput.action.ReadValue<Vector2>();
            lookInput.x = mouseInput.x;
            lookInput.y = mouseInput.y;
        }
        
    }

    void VelocityCalc()
    {
        Vector3 velocityTemp = charactercontroller.velocity;
        velocity = velocityTemp;
    }

    void Sway()
    {
        Vector3 invertLook = lookInput * -step;
        invertLook.x = Mathf.Clamp(invertLook.x, -maxStepDistance, maxStepDistance);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxStepDistance, maxStepDistance);
        swayPos = invertLook;
    }

    void SwayRotation()
    {
        Vector2 invertLook = lookInput * -rotationStep;
        invertLook.x = Mathf.Clamp(invertLook.x, -maxRotationStep, maxRotationStep);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxRotationStep, maxRotationStep);
        swayEulerRot = new Vector3(invertLook.y, invertLook.x, invertLook.x);
    }

    void CompositePositionRotation()
    {
        Vector3 targetPos = pivotOffset + swayPos + bobPosition;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * smooth);

        Quaternion targetRot = Quaternion.Euler(swayEulerRot) * Quaternion.Euler(bobEulerRotation);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * smoothRot);
    }

    void BobOffset()
    {
        speedCurve += Time.deltaTime * (mover.isGrounded ? velocity.magnitude : 1f) + 0.01f;

        bobPosition.x = (curveCos * bobLimit.x * (mover.isGrounded ? 1 : 0)) - (walkInput.x * travelLimit.x);
        bobPosition.y = (curveSin * bobLimit.y) - (velocity.y * travelLimit.y);
        bobPosition.z = -(walkInput.y * travelLimit.z);
    }

    void BobRotation()
    {
        bobEulerRotation.x = (walkInput != Vector2.zero ? multiplier.x * (Mathf.Sin(2 * speedCurve)) :
                                                        multiplier.x * (Mathf.Sin(idleBreath * speedCurve) / 2));
        bobEulerRotation.y = (walkInput != Vector2.zero ? multiplier.y * curveCos : 0);
        bobEulerRotation.z = (walkInput != Vector2.zero ? multiplier.z * curveCos * walkInput.x : 0);
    }
}
