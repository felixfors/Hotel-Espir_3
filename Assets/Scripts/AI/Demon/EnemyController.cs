using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public static EnemyController instance;
    #region Allmäna variablar
    //Navmesh Variables
    public Transform target;
    public NavMeshAgent agent;
    private Transform player;
    private Transform playerCamera;
    [HideInInspector]
    public Animator animator;
    public float walkSpeed; // AI hastighet
    public bool freezeGhost;
    public int hearingDistance;
    
    private float speed;
    private float targetDistance;
    private bool inRange; // vi står bredvid vårat mål
    #endregion
    #region States
    private bool startState;
    public States enemyState;
    public enum States
    {
        idle, patrol, search, chase
    }
    #endregion
    #region Idle variablar
    private Vector2 idleSetTimer = new Vector2(1,15); // vänta 1-15 sekunder
    [HideInInspector]public float idleTimer;
    #endregion
    #region Patrol variablar
    public List<Transform> wayPoints = new List<Transform>();
    public Transform wayPointParent;
    #endregion
    #region Search variablar
    private Vector2 searchSetTimer = new Vector2(8,15);
    private float searchTimer;
    private bool targetReached;
    #endregion
    #region Chase variablar
    public bool playerIsCaught;
    #endregion

    #region Door variablar
    private float lastDoorDistance;
    private Doors doorInteraction;
    #endregion
    #region LocalSearch
    private bool localSearch;
    private float localSearchRadius = 12;
    private Vector2 localSearchSetTimer = new Vector2(1, 4);
    private float localSearchTimer;
    public List<Transform> waypoint = new List<Transform>();
    #endregion
    #region Vision
    public float visionRange = 5;    
    private bool ghostLOS;

    private bool lookingAtGhost;
    private bool previouslookingAtGhost;

    private bool playerInVisionRange;
    private bool cameraInVisionRange;
    private float visionAngle = 0.1f;
    private bool withinVisionAngle;
    [HideInInspector]
    public bool lostVision;
    private bool canSeePlayer;

    #endregion

    #region Banish
    public Transform banishPosition;
    [HideInInspector]
    public bool banishActive;
    public float banishDistance;
    private float banishCooldown = 10f;
    private float BanishTransition = 2.5f;
    public AudioClip banishScreamSound;
    #endregion
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!animator) animator = GetComponent<Animator>();
        agent.updatePosition = false;
        if(GameObject.FindGameObjectWithTag("Player"))
            player = GameObject.FindGameObjectWithTag("Player").transform;
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
        GetWayPoints();
        SwitchState(States.idle);
    }
    private void GetWayPoints()
    {
        for(int i = 0; i < wayPointParent.childCount; i ++)
        {
            wayPoints.Add(wayPointParent.GetChild(i));
        }
    }


    // Update is called once per frame
    void Update()
    {
        if(target)
            agent.SetDestination(target.position);
        AIMovement();
        StateManager();
        if(!playerIsCaught && player !=null) // om vi är tagna så ska vi inte leta efter spelaren förens vi har startat om
            Vision();
        if(agent.hasPath)
            GetDoorPath();
        MusicScared();
    }
    public void FreezeGhost(bool _freeze)
    {
        freezeGhost = _freeze;
        idleTimer = _freeze ? 0 : idleTimer;
        SwitchState(_freeze ? States.idle : States.patrol);
    }
    public void SoundImpact(float loudness, Transform newTarget)
    {
        
        int humanoidID = NavMesh.GetSettingsByIndex(0).agentTypeID;
        NavMeshQueryFilter filter = new NavMeshQueryFilter
        {
            agentTypeID = agent.agentTypeID,areaMask = agent.areaMask
        };

        NavMeshPath newPath = new NavMeshPath();

        if(NavMesh.CalculatePath(transform.position, newTarget.position, filter, newPath))
        {
            for (var i = 0; i < newPath.corners.Length - 1; i++) // Kolla om vi kolliderar med dörrar på den nya stigen
            {
                Debug.DrawLine(newPath.corners[i], newPath.corners[i + 1], Color.red);
                RaycastHit hit;
                if (Physics.Linecast(newPath.corners[i], newPath.corners[i + 1], out hit))
                {
                    if(hit.transform.gameObject.layer == 11) // Door layer
                    {                        
                        Doors hitDoor = hit.transform.GetComponentInChildren<Doors>();
                        if (hitDoor.rotation == 0) // dörren är stängd
                        {
                            loudness = loudness / 2; // dämpa ljudet om vi träffar en dörr
                            break;
                        }
                    }
                }
            }

            float distance = Vector3.Distance(transform.position, newPath.corners[0]);
            for(int j = 1; j < newPath.corners.Length; j ++)
            {
                distance += Vector3.Distance(newPath.corners[j - 1], newPath.corners[j]);
            }
            if(distance <= hearingDistance) // objektet är innanför hörselzonen
            {
                int soundZone = hearingDistance / hearingDistance; // vi har 25st hörsel zoner
                float currentZone = soundZone * loudness;
                //Debug.Log("ljudet hörs " + currentZone+"m" + " detta är loudness " + loudness);
                if(distance < currentZone) // vi är innanför ljudnivån
                {                    
                    target.position = newTarget.position;
                    SwitchState(States.search);
                }               
            }
        }
    }
    public void TryBanish()
    {
        RaycastHit rayHit;
        int wallLayer = 1 << 13;
        int doorLayer = 1 << 11;
        int layerMask = wallLayer | doorLayer;
        if (!Physics.Linecast(transform.position + new Vector3(0, 1, 0), player.position, out rayHit, layerMask) && Vector3.Distance(transform.position, player.position) <= banishDistance) // det är ingenting emellan spöket och spelaren
        {
            StartCoroutine("Banish");
        }
    }
    private IEnumerator Banish()
    {
        animator.SetBool("Banish", true);
        EnemyAttack.instance.attackAudio.PlayOneShot(banishScreamSound);
        banishActive = true;
        yield return new WaitForSeconds(BanishTransition);
        agent.Warp(banishPosition.position);
        target.position = banishPosition.position;
        yield return new WaitForSeconds(banishCooldown);
        animator.SetBool("Banish", false);
        banishActive = false;
        EnemyAttack.instance.RepositionGhost();
        SwitchState(States.idle);

    }
    private void GetDoorPath()
    {
        int layerMask = 1 << 11;
        Debug.DrawLine(agent.path.corners[0], agent.path.corners[1], Color.green);
        RaycastHit hit;
        if (Physics.Linecast(transform.position + new Vector3(0,1,0), agent.path.corners[1] + new Vector3(0,1,0), out hit, layerMask))
        {
            if (hit.transform.gameObject.layer == 11) // Door layer
            {
                bool movingTowards = false;
                float distance = Vector3.Distance(hit.transform.position, transform.position);
                if (doorInteraction != hit.transform.GetComponentInChildren<Doors>())
                    doorInteraction = hit.transform.GetComponentInChildren<Doors>();
                if (lastDoorDistance != distance)
                {
                    if (lastDoorDistance > distance) // vi går närmare dörren
                    {
                        movingTowards = true;
                        lastDoorDistance = distance;
                    }
                    else // vi går längre ifrån dörren
                    {
                        movingTowards = false;
                        lastDoorDistance = distance;
                    }

                } // kollar om vi går emot eller ifrån dörren
                if (distance < 1.5f && movingTowards)
                {
                    if (!doorInteraction.AIoverride) // om vi inte redan har rört dörren och dörren inte är låst
                    {
                        doorInteraction.AIDoorInteractionDoor(true, 0.5f);
                        doorInteraction.AIoverride = true;
                    }
                    else
                    if (doorInteraction.locked)
                    {
                        SwitchState(States.idle);
                    }
                }
            }
        }
        else
        {
            if (doorInteraction)
            {
                float distance = Vector3.Distance(transform.position, doorInteraction.transform.position);
                if (distance > 1.5f)
                {
                    doorInteraction.AIDoorInteractionDoor(false, 0.5f);
                    doorInteraction = null;
                }
            }
        }
    }
    private void MusicScared()
    {
        if (!MusicController.instance || player == null)
            return;
        int layerDoor = 1 << 11;
        int layerWall = 1 << 16;
        int layerMask = layerWall | layerDoor;

        Vector3 center = transform.position + Vector3.up * 1.0f; // mittpunkt i höjd (typ brösthöjd)

        // Manuella checkpoints (offset i X/Z runt mitten)
        Vector3[] checkPoints = new Vector3[]
        {
        center,                                       // mitten
        center + Vector3.right * 0.5f,                // höger
        center - Vector3.right * 0.5f,                // vänster
        center + Vector3.forward * 0.5f,              // framåt
        center - Vector3.forward * 0.5f               // bakåt
        };

        bool visible = false;

        foreach (var point in checkPoints)
        {
            Vector3 dir = (point - playerCamera.position).normalized;
            float dist = Vector3.Distance(playerCamera.position, point);

            if (!Physics.Raycast(playerCamera.position, dir, dist, layerMask))
            {
                Debug.DrawLine(playerCamera.position, point, Color.green);
                visible = true; // minst en punkt synlig
            }
            else
            {
                Debug.DrawLine(playerCamera.position, point, Color.red);
            }
        }

        ghostLOS = visible;

        // === VINKEL mot spelaren ===
        Vector3 playerForward = player.forward;
        Vector3 dirToGhost = (transform.position - player.position).normalized;

        float visionDot = Vector3.Dot(playerForward, dirToGhost);
        bool facingGhost = visionDot > 0.65f;

        lookingAtGhost = facingGhost && ghostLOS;

        // === MUSIKSTATE ===
        if (lookingAtGhost != previouslookingAtGhost)
        {
            if (lookingAtGhost && !canSeePlayer)
            {
                Debug.Log("Nu ser vi spöket");
                MusicController.instance.SwitchTrackState(TrackState.scared);
            }
            else if (!lookingAtGhost && !canSeePlayer)
            {
                Debug.Log("Nu ser vi inte spöket");
                MusicController.instance.currentBiom.resetMusicToBiom();
            }
            previouslookingAtGhost = lookingAtGhost;
        }
    }



    private void Vision()
    {
        Vector3 _enemyForward = transform.TransformDirection(Vector3.forward);
        Vector3 _playerDirection = Vector3.Normalize(player.position - transform.position);
        withinVisionAngle = Vector3.Dot(_enemyForward, _playerDirection) > visionAngle; // räkna ut om vi står bakom eller framför
        if(withinVisionAngle)
        {
            playerInVisionRange =! Physics.Linecast(transform.position + new Vector3(0, 1.5f, 0), player.position + new Vector3(0, 1, 0));
            float distance = Vector3.Distance(transform.position, player.position);
            int layerPlayer = 1 << 3;
            int layerMask = layerPlayer;
            layerMask = ~layerMask;
            if (!Physics.Linecast(transform.position + new Vector3(0, 1.5f, 0), player.position + new Vector3(0, 1.5f, 0), layerMask)) // kolla om vi ser spelaren
                playerInVisionRange = true;
            else
                playerInVisionRange = false;

                

            if (!Physics.Linecast(transform.position + new Vector3(0, 1.5f, 0), playerCamera.position, layerMask)) // kolla om vi ser kameran
                cameraInVisionRange = true;
            else
                cameraInVisionRange = false;

            if (!playerInVisionRange || !cameraInVisionRange && !playerInVisionRange) // vi syns inte längre
            {
                if(canSeePlayer)
                {
                    if (MusicController.instance.currentBiom)
                        MusicController.instance.currentBiom.resetMusicToBiom();
                    else
                        MusicController.instance.SwitchTrackState(TrackState.idle);

                    lostVision = true;
                    canSeePlayer = false;
                }
                    
            }
            if (enemyState != States.chase && enemyState == States.search)
            {
                if (playerInVisionRange && distance < visionRange) // Här syns spelaren
                {
                    canSeePlayer = true;
                    MusicController.instance.SwitchTrackState(TrackState.chased);
                    SwitchState(States.chase);
                }
                else if (!playerInVisionRange && cameraInVisionRange && distance < visionRange / 2) // här syns bara huvudet
                {
                    canSeePlayer = true;
                    MusicController.instance.SwitchTrackState(TrackState.chased);
                    SwitchState(States.chase);
                }
            }
            
        }
        else
        {
            playerInVisionRange = false;
            cameraInVisionRange = false;
        }
        if(!playerInVisionRange || !cameraInVisionRange && !playerInVisionRange)
        {
            if (EnemyAttack.instance.jumpScareActive)
            {
                playerInVisionRange = false;
                cameraInVisionRange = false;
                lostVision = false;
                return;
            }
                lostVision = false;
            if (lostVision)
            {
                if(!EnemyAttack.instance.jumpScareActive)
                {
                    Debug.Log("1");
                    SwitchState(States.search);
                }                   
                lostVision = false;
            }
                      
            playerInVisionRange = false;
            cameraInVisionRange = false;
        }
    }
    private void StateManager()
    {
        if (enemyState == States.idle)
        {            
            IdleState();
        }
        else if(enemyState == States.patrol)
        {
            PatrolState();
        }
        else if(enemyState == States.search)
        {
            SearchState();
        }
        else if(enemyState == States.chase)
        {
            ChaseState();
        }
    }
    public void SwitchState(States states)
    {
        startState = false;
        enemyState = states;

    }
    private void IdleState()
    {
        if(freezeGhost) // gör så ghosten står helt stilla fram tills den är unfreezed
        {
            walkSpeed = 0; // vi ska stå stilla
            return;
        }
        if(!startState)
        {
            walkSpeed = 0; // vi ska stå stilla
            idleTimer = Random.Range(idleSetTimer.x, idleSetTimer.y); // hur länge ska vi stå stilla
            startState = true;
        }
        else
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0) // vi har stått färdigt, börja patrolera
                SwitchState(States.patrol);
        }
    }
    private void PatrolState()
    {
        if (!startState)
        {           
            target.position = wayPoints[Random.Range(0, wayPoints.Count)].position; // välj ett nytt mål
            walkSpeed = 1; // börja gå
            startState = true;
        }
        else
        {
            if(inRange)
            {
                SwitchState(States.idle);
            }
        }
    }
    private void SearchState()
    {
        if (!startState)
        {
            walkSpeed = 2;
            EnemySound.instance.SearchSound();
            searchTimer = Random.Range(searchSetTimer.x, searchSetTimer.y);
            targetReached = false;
            startState = true;
        }
        else
        {
            if(inRange)
            {
                if(!targetReached)// räkna bara ner efter vi har nått första målet
                {
                    localSearchTimer = Random.Range(localSearchSetTimer.x, localSearchSetTimer.y);
                    targetReached = true;
                }
                                

                if (!localSearch)
                {
                    localSearchTimer -= Time.deltaTime;
                    if(localSearchTimer <=0)
                    {
                        localSearchTimer = Random.Range(localSearchSetTimer.x, localSearchSetTimer.y);
                        waypoint.Clear();
                        LocalSearch(); // Om vi är framme och fortfarande har vänt tid så ska vi gå runt i rummet  
                    }                                    
                }                            
            }

            if(targetReached) // räkna bara ner efter vi har nått första målet
                searchTimer -= Time.deltaTime;
            if (searchTimer <= 0)
                SwitchState(States.patrol);
        }
    }
    private void LocalSearch()
    {
        localSearch = true;
        walkSpeed = 1.5f;
        int layerWaypoint = 1 << 12; // we only want to hit spawnpoint layers
        Collider[] waypoints = Physics.OverlapSphere(transform.position, localSearchRadius, layerWaypoint);
        foreach(Collider hit in waypoints)
        {
            if(hit.transform.position != target.position) //We dont want to add our current spawnpoint
            {
                RaycastHit rayHit;
                int wallLayer = 1 << 13;
                int doorLayer = 1 << 11;
                int layerMask = wallLayer | doorLayer;
                if(!Physics.Linecast(transform.position+new Vector3(0,1,0),hit.transform.position, out rayHit, layerMask))
                {
                    waypoint.Add(hit.transform);
                }
            }                   
        }
        if(waypoint.Count >0)
        {
            target.position = waypoint[Random.Range(0, waypoint.Count)].position; // ändra target position
            Debug.Log("we have found a new localposition");            
        }
        localSearch = false;

    }
    private void ChaseState()
    {
        if (!startState)
        {
            
            EnemySound.instance.ChaseSound();
            walkSpeed = 2;
            startState = true;
        }
        else
        {
            target.position = player.position;
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= 1 && !EnemyAttack.instance.jumpScareActive && !banishActive)// om vi är nära och inte har påbörjar jumpscare 
            {
                lostVision = false;
                SwitchState(enemyState = States.idle);
                EnemyAttack.instance.StartJumpScare();
            }                
        }
    }

    private void OnAnimatorMove()
    {
        Vector3 position = animator.rootPosition;
        position.y = agent.nextPosition.y;
        transform.position = position;
        agent.nextPosition = transform.position;
    }
    private void AIMovement()
    {
        var dir = (agent.steeringTarget - transform.position).normalized;
        var animDir = transform.InverseTransformDirection(dir);
        var isFacingMoveDirection = Vector3.Dot(dir, transform.forward) > .5f;
        bool moving = animDir.x > 0.1f | animDir.z > 0.1f;

        animator.SetFloat("WalkSpeed", speed); // ändra hastigheten på zombin
        
        if(target)
        {
            targetDistance = Vector3.Distance(transform.position, target.position);
            inRange = targetDistance < 1;

            if (!inRange) // så zombin inte går in i spelaren, stannar 1 unit ifrån
            {
                if (moving && isFacingMoveDirection)
                {
                    speed = Mathf.Lerp(speed, walkSpeed, Time.deltaTime * 2);
                }
                else
                {
                    speed = Mathf.Lerp(speed, 0, Time.deltaTime * 2);
                    if (speed <= 0.1f)
                        speed = 0;
                }
            }
            else // zombin är tillräckligt nära för att stå stilla
            {
                speed = Mathf.Lerp(speed, 0, speed >1 ? Time.deltaTime * 5 : Time.deltaTime * 2); // bromsa in snabbare om vi springer
                if (speed <= 0.1f)
                    speed = 0;
            }
        }       
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), 180 * Time.deltaTime);
    }



    private void OnDrawGizmos()
    {
        if (agent.hasPath)
        {
            for (var i = 0; i < agent.path.corners.Length - 1; i++)
            {
                Debug.DrawLine(agent.path.corners[i], agent.path.corners[i + 1], Color.blue);
            }
        }
    }
}
