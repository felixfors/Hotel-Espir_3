using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class Doors : MonoBehaviour
{
	public bool debug;
	public int priorityLevel;
	private string id; // used for storing data/saved files
	
	[HideInInspector]
	public RoomCulling roomCulling;

	//[HideInInspector]
	public bool doorOpen;
	public bool locked;
	public Transform keyHole;
	public InspectManager inspectManager;

	public bool unlocking; // used for unlocking animation
	public ItemData unlockItem;

	//[HideInInspector]
	public bool canInteract;
	//[HideInInspector]
	public bool interacting;
	private bool inRange;
	public float weight = 1;
	public Transform hinge;
	public Transform frame;
	public Transform door;
	public bool orientationUp;
	public Vector3 startOrientation;
	public Transform handle;
	public Transform dragReference;
	private Transform playerReference;
	public float dragValue;
	public float rotationDrag; // det som läggs till när vi drar musen
	float tempR;
	private float mouseDirection;
	private string mouseOrientation;
	private float mouseOrientationValue;
	private bool rotationSwap;
	private float tempRotation;
	public bool inside;
	public bool reverseInside;
	public bool scaleInverted;
	
	public float rotation;
	public Vector2 doorClamp;
	public Vector2 autoCloseRange = new Vector2(8,8);
	private float mouseValue;

	public float directionClamp;
	public float closeDuration;
	private float lerpedValue;
	public AnimationCurve animCurve;




	//Audio variables
	[Space(20)]
	public AudioSource doorCreakAudio;
	public AudioSource doorAudio;
	public AudioSource doorCloseAudio;
	public AudioSource doorHandleAudio;
	public AudioClip doorCreak;
	public AudioClip doorClose;
	public AudioClip[] doorhandle;
	public AudioClip[] lockedDoorSound;
	public AudioClip unlockSound;
	public AudioClip doorSlam;
	private bool doorClampPlay;

	// Player moving variables
	[Space(50)]
	private float playerDistance;
	private float lastDistance;
	private float movingDireciton; // WIP
	private float pushForce; // WIP

	//Door Locked variables
	public bool rattling;
	public float rattleTimer;
	public float maxRattleRot = 2;
	public float rattleAmplitude;
	public float rattleSpeed;
	[HideInInspector]
	public bool AIoverride;
	[HideInInspector]
	public NavMeshObstacle navmeshObstacle;
	public float mouseMovement;

	#region Ljud till Ghost
	private float currentEuler;
	private float previousEuler;
	private float yDegreesPerSecond;
	private float rotationSpeed;
	private float soundValue;
	private float doorLoudnessSensetivity = 1;
	private bool playerInteractedDoor;

	public AudioClip[] aiOpenSound;
	public AudioClip[] aiCloseSound;

	#endregion

	private void Awake()
    {
		startOrientation = door.transform.localEulerAngles;// detta är så att man kan placera dörrar som är öppnade i editorn utan att dem stängs till 0 rotation vid play
		float rotationValue = orientationUp ? startOrientation.y : startOrientation.x;
		if (rotationValue > 180) // inspektorn kan inte läsa value som är negativt så måste omvandla
			rotationValue -=360;
		rotation = rotationValue;

		navmeshObstacle = GetComponent<NavMeshObstacle>();
		previousEuler = Vector3.Angle(transform.right, Vector3.right); // för soundImpact, annars så är start value för högt
		mouseOrientation = orientationUp ? "Mouse X" : "Mouse Y";
	}
    private void Start()
    {
		if(PlayerController.instance !=null)
		playerReference = PlayerController.instance.transform;
	}
    private void Update()
    {
		if (debug)
			Debug.Log(door.localEulerAngles.y);
		
		if (!locked)
        {
			
			bool movingMouse = false;
			if (interacting)
				movingMouse = PlayerController.instance.lookInput.action.ReadValue<Vector2>().x != 0 | PlayerController.instance.lookInput.action.ReadValue<Vector2>().y != 0 | PlayerController.instance.isMoving;
			else
				movingMouse = false;
		
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			dragReference.position = Vector3.MoveTowards(dragReference.position, movingMouse ? ray.GetPoint(Vector3.Distance(door.position, playerReference.position)) : door.position, dragValue * Time.deltaTime);
			rotationDrag = Vector3.Distance(dragReference.position, door.position);
			Mathf.Abs(mouseOrientationValue);
			rotation += (rotationDrag * mouseDirection + mouseOrientationValue) * directionClamp;

			if (mouseOrientationValue != 0)
				mouseOrientationValue = Mathf.Lerp(mouseOrientationValue, 0, Time.deltaTime * weight);
			if (mouseOrientationValue < 0) mouseOrientationValue = 0;

			rotation = Mathf.Clamp(rotation, doorClamp.x, doorClamp.y);

			// Beräkna målvinkel baserat på orientationUp
			Vector3 targetEuler = new Vector3(!orientationUp ? rotation : 0, orientationUp ? rotation : 0, 0);

			// Konvertera euler-vinklarna till en rotation
			Quaternion targetRotation = Quaternion.Euler(targetEuler);

			// Smooth interpolation med exponential decay (liknar Ease.OutSine)
			float t = 1 - Mathf.Exp(-weight * Time.deltaTime);

			// Använd Lerp för att rotera dörren smidigt mot målvinkeln
			door.localRotation = Quaternion.Lerp(door.localRotation, targetRotation, t);


			if(rotationDrag >0)
            {
				if(tempRotation > mouseDirection && !rotationSwap) // vi svingar åt vänster
                {
					dragReference.position = door.position;
					mouseOrientationValue = 0;
					rotationSwap = true;
                }
				else if(tempRotation < mouseDirection && rotationSwap) // vi svingar åt höger
				{
					dragReference.position = door.position;
					mouseOrientationValue = 0;
					rotationSwap = false;
				}
				if (door.eulerAngles.y == doorClamp.x || door.eulerAngles.y == doorClamp.y)
				{
					if (!doorClampPlay)
                    {
						if (rotation != 0)
						{
							doorAudio.PlayOneShot(doorSlam);
							doorClampPlay = true;
						}
						else
						{
							doorAudio.PlayOneShot(doorSlam);
							doorClampPlay = true;
						}
					}										
				}
				else
                {
					if(doorClampPlay)
						doorClampPlay = false;
				}
					
			}
			if(roomCulling)
            {
				if (door.localEulerAngles.y >-0.45f  && door.localEulerAngles.y < 0.45f) // vi stängde dörren
				{
					if(doorOpen)
                    {
						roomCulling.RoomCull(false);
						doorOpen = false;
					}				
				}
				else if (door.localEulerAngles.y <-0.45f || door.localEulerAngles.y > 0.45f) // vi öppnade dörren
				{
					if(!doorOpen)
                    {
						if (interacting)
							roomCulling.RoomCull(true);
						doorOpen = true;
					}
				}
			}
			CalculateRotationSpeed(); // för soundimpact

			if (rotationDrag <= 0.01f && !interacting)
				playerInteractedDoor = false;
		}
		if(PlayerController.instance !=null)
        {
			inRange = Vector3.Distance(PlayerController.instance.transform.position + new Vector3(0, 1, 0), door.position) < InteractManager.instance.interactDistance + 1f;
			PlayerPosition();
		}		

		//DoorAudio();
		if(handle)
			DoorHandleMovement();
		if (interacting)
        {
			UpdateDoorRotationTWO();			
		}
			
		if (rattling)
        {
			if(unlocking)
				rattling = false;
			Rattle();
		}
			

		//Start dragging
		if (canInteract && PlayerController.instance.doorInteractInput.action.WasPressedThisFrame() && inRange)
        {
			Interacting_Status(true);
			if (inspectManager && locked)
            {
				inspectManager.StartInspecting();
				return;
            }
			if(!locked) // the door is unlocked
            {
				DoorHandleAudio(0);
				interacting = true;
				playerInteractedDoor = true;
				SetDoorDirection();
				if(rotation !=0)
				StopCoroutine("CloseDoor");
				PlayerController.instance.lookSpeedMultiplier = GeneralSettings.instance.cameraFollowDrag? 0 : 0.2f;
			}
			else if(locked && !rattling && !unlocking) // the door is locked and we are not trying to open it
            {
				Debug.Log("rattle");
				doorAudio.PlayOneShot(lockedDoorSound[Random.Range(0,lockedDoorSound.Length)]);
				doorAudio.pitch = Random.Range(0.9f,1.1f);
				rattling = true;
            }
			
        }
					
		// Stop dragging
		if(interacting && PlayerController.instance.doorInteractInput.action.WasReleasedThisFrame())
        {
			Interacting_Status(false);
			PlayerController.instance.lookSpeedMultiplier = 1f;
			InteractManager.instance.lastHit = null;
			DoorHandleAudio(1);
			interacting = false;
			TryAndCloseDoor();
			SwingDoor();			
		}
		// Disable Interaction by distance
		if (!inRange)
        {
			if(interacting)
            {
				DoorHandleAudio(1);
				TryAndCloseDoor();
				SwingDoor();
				PlayerController.instance.lookSpeedMultiplier = 1f;
				interacting = false;

			}
        }
		if (AIoverride && Vector3.Distance(transform.position, EnemyController.instance.transform.position) > 1.7f)// om AIn är långt borta och ain har kontroll över dörren så återställ
		{
			AIoverride = false;
		}						
	}
	private void CalculateRotationSpeed()
    {
		currentEuler = Vector3.Angle(transform.right, Vector3.right);
		yDegreesPerSecond = Mathf.Abs(currentEuler - previousEuler);
		if (yDegreesPerSecond < 0.01f && doorCreakAudio.volume != 0)// Om dörren inte rör på sig så ska vi sluta kalkulera
		{
			doorCreakAudio.volume = 0;
			doorCreakAudio.Stop();
			return;
		}
		yDegreesPerSecond = Mathf.Clamp01(yDegreesPerSecond);
		doorCreakAudio.volume = yDegreesPerSecond/4;
		
		if (!doorCreakAudio.isPlaying)
			doorCreakAudio.PlayOneShot(doorCreak);

		
		if (playerInteractedDoor)
		{
			Debug.Log(soundValue);

			float loudness = (yDegreesPerSecond) * 25;
			EnemyController.instance.SoundImpact(loudness, PlayerController.instance.transform);
			PlayerAudioDetection.instance.SoundImpact(loudness);
		}

		previousEuler = currentEuler;		
	}
	private void UpdateDoorRotationTWO()
    {
		if (!dragReference)
			return;
		if (PlayerController.instance.lookInput.action.ReadValue<Vector2>().x > 0 || PlayerController.instance.lookInput.action.ReadValue<Vector2>().y > 0)
			mouseDirection = 1;
		else if (PlayerController.instance.lookInput.action.ReadValue<Vector2>().x < 0 || PlayerController.instance.lookInput.action.ReadValue<Vector2>().y < 0)
			mouseDirection = -1;
		playerReference.position = PlayerController.instance.transform.position;

		float YDirection;// används för att pusha dörren med Mouse Y i rätt riktning
		Vector3 directionToDoor = (PlayerController.instance.transform.position - door.transform.position).normalized;
		float dotProduct = Vector3.Dot(door.transform.forward, directionToDoor);
		if (dotProduct >0)
        {
			YDirection = inside? -1 : 1;
		}		
		else
        {
			YDirection = inside ? 1 : -1;
		}
			
		float mouseY = (PlayerController.instance.lookInput.action.ReadValue<Vector2>().y * DetectController.instance.currentIntensitivity) * YDirection;
		mouseOrientationValue = (PlayerController.instance.lookInput.action.ReadValue<Vector2>().x * DetectController.instance.currentIntensitivity) + mouseY;
		mouseOrientationValue /= 40;
		//mouseOrientationValue = Input.GetAxis(mouseOrientation);
	}
	private void SwingDoor()
    {
		float swingAmount = rotation += (rotationDrag * mouseDirection) + Input.GetAxis(mouseOrientation);
		float swingDistance = swingAmount - rotation;
		StartCoroutine(CloseDoor(rotation, swingAmount, swingDistance));
	}
	public void PushDoor(float strength, Transform source)
    {
		int tempDirection;
		if (Vector3.Dot((door.position - source.position).normalized, door.forward) > 0)
			tempDirection = -1;
		else
			tempDirection = 1;
		directionClamp = tempDirection;
			mouseOrientationValue += strength;
    }

	private void DoorHandleAudio(int soundID)
	{
		
		doorAudio.Stop();
		if (doorhandle.Length-1 <soundID)
			return;
		doorHandleAudio.PlayOneShot(doorhandle[soundID]);
		doorHandleAudio.pitch = Random.Range(0.9f, 1.2f);
	}
	
	private void SetDoorDirection()
    {

		Vector3 _forward = frame.TransformDirection(Vector3.back);
		Vector3 _toOther = Vector3.Normalize(PlayerController.instance.transform.position - frame.position);
		inside = Vector3.Dot(_forward, _toOther) >0;

		if(orientationUp)
        {
			if (rotation < -0.01f)
			{
				if (inside)
					directionClamp = 1;
				else
					directionClamp = -1;
			}
			else if (rotation > 0.01f)
			{
				if (inside)
					directionClamp = -1;
				else
					directionClamp = 1;
			}
			else
			{
				if (inside)
					directionClamp = reverseInside? directionClamp = -1 : directionClamp = 1;
				else
					directionClamp = reverseInside ? directionClamp = 1 : directionClamp = -1;
			}
		}
		else
        {
			directionClamp = 1;
        }
		if (scaleInverted)
		{
			directionClamp *= -1;
		}
	}
	public void AIDoorInteractionDoor(bool _openDoor, float _speed)
    {
		float _doorRotation = 0;
		if(_openDoor)
        {
			doorAudio.PlayOneShot(aiOpenSound[Random.Range(0, aiOpenSound.Length)]);
			if (doorClamp.x == 0 && doorClamp.y != 0 || doorClamp.x != 0 && doorClamp.y == 0) // Dörren öppnas bara åt ett håll
			{

				if (doorClamp.x != 0)
					_doorRotation = doorClamp.x;
				else if (doorClamp.y != 0)
					_doorRotation = doorClamp.y;
			}
			else if (doorClamp.x != 0 && doorClamp.y != 0) // Dörren öppnas båda hållen
			{
				Vector3 _forward = frame.TransformDirection(Vector3.back);
				Vector3 _toOther = Vector3.Normalize(EnemyController.instance.transform.position - frame.position);
				inside = Vector3.Dot(_forward, _toOther) > 0;
				_doorRotation = inside ? doorClamp.x : doorClamp.y;
			}
		}
		else
        {
			doorAudio.PlayOneShot(aiCloseSound[Random.Range(0, aiCloseSound.Length)]);
		}
		
		StartCoroutine(CloseDoor(rotation, _doorRotation, _speed));
	}
	private void TryAndCloseDoor()
    {
		
		if(rotation > -autoCloseRange.x && rotation <autoCloseRange.y && rotation !=0)
        {
			StartCoroutine(CloseDoor(rotation,0,closeDuration));
        }
    }
	public void squeakDoor(float amount)
    {
		//rotation + 20, 1.75f
		StartCoroutine(CloseDoor(rotation, rotation+amount, 1.75f));
	}
	IEnumerator CloseDoor(float start, float end, float speed)
    {
		float timeElapsed = 0;
		while(timeElapsed < speed)
        {
			float t = timeElapsed / speed;
			t = animCurve.Evaluate(t);

			lerpedValue = Mathf.Lerp(start, end, t);
			rotation = lerpedValue;
			timeElapsed += Time.deltaTime;
			if (interacting)
				break;
			yield return null;			
        }
		doorCloseAudio.volume = AIoverride ? 0.7f : 0.2f; // om spelaren stänger så ska det inte låta lika mycket som när spöket smäller
		
		if (end == 0 && !AIoverride)
        {
			if(doorCloseAudio != null)
			doorCloseAudio.PlayOneShot(doorClose);			
			rotation = 0;
		}		
		lerpedValue = end;
    }
	private void DoorHandleMovement()
	{
		bool handleDown = interacting | rattling;

			float handleRotation = handleDown ? 45 : 0;
			if (handle.localEulerAngles.z != handleRotation)
			{
				AdjustController(handleRotation);
			}
	}
	private void AdjustController(float rotation)
	{
		Vector3 handleRotation = new Vector3(0,0, rotation);
		handle.localEulerAngles = Vector3.LerpUnclamped(handle.localEulerAngles, handleRotation, 10 * Time.deltaTime);
	}
	private void PlayerPosition()
    {
		playerDistance = Vector3.Distance(PlayerController.instance.transform.position, door.position);

		if(lastDistance != playerDistance && PlayerController.instance.isMoving)
        {
			if(playerDistance > lastDistance)
            {
				movingDireciton = pushForce/10;
				lastDistance = playerDistance;
            }
			else if(playerDistance < lastDistance)
            {
				movingDireciton = -pushForce/10;
				lastDistance = playerDistance;
			}
			else
            {
				movingDireciton = 0;
				lastDistance = playerDistance;
			}
        }
		else if(!PlayerController.instance.isMoving)
        {
			movingDireciton = 0;
        }
    }

	//Rattling
	public void Rattle()
    {
		if(unlocking) return;
		var localrot = transform.localEulerAngles;
		float duration = 1;

		rattleTimer += Time.deltaTime;
		float t = rattleTimer / duration;

		if (t < duration)
        {
			if(orientationUp)
            {
				localrot.y += Mathf.Sin(Time.time * rattleSpeed) * rattleAmplitude;
				localrot.y = Mathf.Clamp(localrot.y, 0, maxRattleRot);
				transform.localEulerAngles = Vector3.Lerp(localrot, transform.localEulerAngles, t);
			}
			else
            {
				localrot.x += Mathf.Sin(Time.time * rattleSpeed) * rattleAmplitude;
				localrot.x = Mathf.Clamp(localrot.x, 0, maxRattleRot);
				transform.localEulerAngles = Vector3.Lerp(localrot, transform.localEulerAngles, t);
			}			
        }
		else
        {
			if (transform.localEulerAngles != localrot)
            {
				if(orientationUp)
					localrot.y = 0;
				else
					localrot.x = 0;
				transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, localrot, t);
            }
			else
            {
				rattleTimer = 0;
				doorAudio.pitch = 1;
				rattling = false;
            }
        }
    }
	public void UnlockDoorDirect()
    {
		locked = false;
		doorHandleAudio.PlayOneShot(unlockSound);
		if(door.GetComponent<Animator>() !=null)
        {
			door.GetComponent<Animator>().SetTrigger("Play");
        }
    }

	public IEnumerator UnlockDoor(Item key)
	{
		unlocking = true;
		key.transform.parent = keyHole;
		key.transform.position = keyHole.position;
		key.transform.rotation = keyHole.rotation;
		key.GetComponent<Animator>().SetTrigger("Play");
		yield return new WaitForSeconds(0.3f);
		locked = false;
		unlocking = false;
	}
	public void StartInteract()
    {
		canInteract = true;
    }
	public void EndInteract()
    {
		canInteract = false;
    }
	public void Interacting_Status(bool interacting)
    {
		InteractManager.instance.currentlyInteracting = interacting;
		if(interacting)
			InteractManager.instance.PriorityLevel(priorityLevel);
		else
			InteractManager.instance.PriorityLevel(0);
	}
	public void SetDoorData(string ID)
	{
		string doorData = PlayerPrefs.GetString(ID);

		string[] _splitData = doorData.Split(char.Parse("_"));
		
		rotation = float.Parse(_splitData[0]);
		Vector3 targetEuler = new Vector3(!orientationUp ? rotation : 0, orientationUp ? rotation : 0, 0);
		Quaternion targetRotation = Quaternion.Euler(targetEuler);

		door.localRotation = targetRotation;

		if (_splitData[1] == "unlocked")
			locked = false;
		else
			locked = true;
			
	}
	private void GenerateDoorTransform(string ID)
    {
		//set rotation
		string doorData = "";
		doorData += rotation+"_";
		doorData += locked ? "locked_" : "unlocked";
		PlayerPrefs.SetString(ID, doorData);
    }
}
