using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class RatAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Coroutine rotateCoroutine;
    private Transform target;
    private Transform previousTarget;

    #region CorpseDetection
    private float corpseRadius = 5;
    private Transform corpsePosition;
    private bool moveToCorpse;
    #endregion
    #region PlayerDetection
    public float visibleFieldRange = 5;
    public LayerMask detectionLayer;
    private bool scared;
    private float scareCoolDown = 8;
    #endregion
    #region idleState
    private float idleTimer;
    private Vector2 idleTimerMinMax = new Vector2(5, 10);
    #endregion
    #region States
    private bool startState;
    public States ratStates;
    public enum States
    {
        idle, findTarget, corpse,
    }
    #endregion
    #region RatSpeed
    [Header("Rotation Settings")]
    public float rotateSpeed = 360f;    // grader per sekund
    public float angleThreshold = 5f;   // hur nära vi måste vara för att börja gå
    private bool rotating;
    [Header("Speed Settings")]
    public float normalSpeed = 3.5f;
    public float turnSpeed = 1.5f;
    public float turnAngleThreshold = 30f; // hur skarp sväng som räknas
    public float slowDownDistance = 1.5f;  // börja bromsa innan hörnet
    public float speedLerp = 5f;           // hur snabbt hastigheten ändras
    #endregion
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false; // vi roterar manuellt först
        SwitchState(States.idle);
        
        GetTarget();
        agent.Warp(target.position);
    }

    void Update()
    {


        StateManager();
        if(!scared)
        PlayerDetection();


        
        if (ratStates == States.findTarget)
        {
            if(!rotating)// kör bromslogik bara när vi faktiskt är på väg
                SlowDownInTurns();
            
            if(!scared)
            CorpseDetection(); // leta bara efter lik om vi är påväg mot target
        }
    }
    private void PlayerDetection()
    {       
        float playerDistance = Vector3.Distance(PlayerController.instance.transform.position, transform.position);
        if(playerDistance <= visibleFieldRange)
        {
            RaycastHit hit;
            Vector3 direction = transform.position - PlayerController.instance.transform.position;
            if(Physics.Raycast(transform.position, direction,out hit, visibleFieldRange,detectionLayer))
            {
                ScaredRat();
            }
        }
    }
    private void CorpseDetection()
    {
        Collider [] Spherehits = Physics.OverlapSphere(transform.position, corpseRadius, detectionLayer);
        foreach(Collider Spherehit in Spherehits)
        {
            if(Spherehit.gameObject.layer == 21) // 21 = Corpse layer vi har en corpse innuti våran radie
            {
                Vector3 direction = transform.position - Spherehit.transform.position;
                if(Physics.Raycast(transform.position, direction, corpseRadius+1, detectionLayer)) // vi kan se en corpse
                {
                    corpsePosition = Spherehit.transform;                    
                    moveToCorpse = true;
                    SwitchState(States.corpse);

                }
            }
        }

    }
    public void ScaredRat()
    {

        SwitchState(States.findTarget);
        scared = true;
        Invoke("ScaredCoolDown", scareCoolDown);
    }
    private void ScaredCoolDown()
    {
        scared = false;
    }
    private void IdleState()
    {
        if (!startState)
        {
            idleTimer = Random.Range(idleTimerMinMax.x, idleTimerMinMax.y);
            startState = true;
        }

        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0)
        {
            SwitchState(States.findTarget);
        }
    }
    private void GetTarget()
    {
        // Hämta alla waypoints
        var allwaypoints = RatsWaypointController.instance.availableWaypoints;

        // Filtrera bort förra target
        List<Transform> possibleTargets = new List<Transform>();
        foreach (var wp in allwaypoints)
        {
            if (wp.transform != previousTarget)
                possibleTargets.Add(wp.transform);
        }

        // Om det finns några kvar att välja på
        if (possibleTargets.Count > 0)
        {
            target = possibleTargets[Random.Range(0, possibleTargets.Count)];
            previousTarget = target; // spara för att jämföra nästa gång
        }
    }
    private void FindTarget()
    {
        if (!startState)
        {
            //int maxDestinations = RatsWaypointController.instance.availableWaypoints.Count;
            
            GetTarget();
            rotateCoroutine = StartCoroutine(RotateThenMove());

            startState = true;
        }

        if (target != null && !rotating)
        {
            float distanceLeft = Vector3.Distance(transform.position, target.position);
            if (distanceLeft <= 0.2f) // framme
            {
                SwitchState(States.idle);
            }
        }
    }
    private void CorpseState()
    {
        if(!startState)
        {
            moveToCorpse = true;
            target = corpsePosition;
            rotateCoroutine = StartCoroutine(RotateThenMove());

            startState = true;
        }
        if (!rotating)// kör bromslogik bara när vi faktiskt är på väg
            SlowDownInTurns();
    }

    private IEnumerator RotateThenMove()
    {
        rotating = true;
        agent.isStopped = true;

        // Räkna ut path innan vi börjar gå
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(target.position, path);

        // Hörn[0] = vår position, hörn[1] = första riktiga waypoint
        Vector3 targetPoint = path.corners.Length > 1 ? path.corners[1] : target.position;

        Vector3 dir = (targetPoint - transform.position).normalized;
        dir.y = 0;


        while (Vector3.Angle(transform.forward, dir) > angleThreshold)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRot, rotateSpeed * Time.deltaTime);

            yield return null;
        }

        // Rotation klar → börja gå
        agent.isStopped = false;
        agent.updateRotation = true;
        agent.SetDestination(target.position);

        rotating = false;
    }

    private void SlowDownInTurns()
    {
        if (agent.path == null || agent.path.corners.Length < 3)
        {
            // inga hörn → håll normal fart
            agent.speed = Mathf.Lerp(agent.speed, normalSpeed, Time.deltaTime * speedLerp);
            return;
        }

        Vector3 currentDir = (agent.path.corners[1] - transform.position).normalized;
        Vector3 nextDir = (agent.path.corners[2] - agent.path.corners[1]).normalized;

        float angle = Vector3.Angle(currentDir, nextDir);
        float distToCorner = Vector3.Distance(transform.position, agent.path.corners[1]);

        bool sharpTurn = angle > turnAngleThreshold && distToCorner < slowDownDistance;
        float targetSpeed = sharpTurn ? turnSpeed : normalSpeed;

        agent.speed = Mathf.Lerp(agent.speed, targetSpeed, Time.deltaTime * speedLerp);
    }

    private void StateManager()
    {
        if (ratStates == States.idle)
        {
            IdleState();
        }
        else if (ratStates == States.findTarget)
        {
            FindTarget();
        }
        else if (ratStates == States.corpse)
        {
            CorpseState();
        }
    }

    public void SwitchState(States states)
    {
        moveToCorpse = false; // reset

        startState = false;
        ratStates = states;

        // Stoppa eventuell pågående rotation
        if (rotateCoroutine != null)
        {
            StopCoroutine(rotateCoroutine);
            rotateCoroutine = null;
            rotating = false; // säkerställ att vi inte "fastnar" i rotate-läget
        }
    }
}
