using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Cockroach : MonoBehaviour
{
    [Header("Rörelseinställningar")]
    public float moveSpeed = 2f;
    public float surfaceStickForce = 10f;
    public float groundCheckDistance = 0.3f;

    [Header("Beteende")]
    public float directionChangeInterval = 2f;
    public float randomTurnAngle = 90f;

    private float directionChangeTimer;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;  // vi styr själva via MovePosition
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        directionChangeTimer = Random.Range(0, directionChangeInterval);
    }

    void FixedUpdate()
    {
        // --- 1. Anpassa till underlaget ---
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, groundCheckDistance))
        {
            Quaternion alignToSurface = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, alignToSurface, Time.fixedDeltaTime * surfaceStickForce);

            // Håll den precis ovanför ytan så den inte hamnar inuti
            transform.position = hit.point + hit.normal * 0.02f;
        }

        // --- 2. Rörelse framåt ---
        Vector3 moveDelta = transform.forward * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveDelta);

        // --- 3. Slumpmässiga riktningsbyten ---
        directionChangeTimer -= Time.fixedDeltaTime;
        if (directionChangeTimer <= 0f)
        {
            float angle = Random.Range(-randomTurnAngle, randomTurnAngle);
            transform.Rotate(Vector3.up, angle, Space.Self);
            directionChangeTimer = directionChangeInterval + Random.Range(-0.5f, 0.5f);
        }

        // --- 4. Kantkontroll & klättra ---
        Vector3 forward = transform.forward;
        Vector3 down = -transform.up;

        if (!Physics.Raycast(transform.position + forward * 0.2f, down, groundCheckDistance))
        {
            if (Physics.Raycast(transform.position + forward * 0.2f, forward, out RaycastHit wallHit, 0.5f))
            {
                // Align till väggen
                Quaternion wallAlign = Quaternion.FromToRotation(transform.up, wallHit.normal) * transform.rotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, wallAlign, Time.fixedDeltaTime * surfaceStickForce);
                transform.position = wallHit.point + wallHit.normal * 0.02f;
            }
            else
            {
                // Vänd om om ingen vägg hittas
                transform.Rotate(Vector3.up, 180f, Space.Self);
            }
        }
    }
}