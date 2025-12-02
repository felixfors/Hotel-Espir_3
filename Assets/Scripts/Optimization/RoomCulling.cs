using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomCulling : MonoBehaviour
{
    public List<Doors> doors = new();
    private Dictionary<Doors, bool> doorVisibility = new Dictionary<Doors, bool>();
    public GameObject roomObjects;

    public bool playerInside;

    private bool roomActive;
    public bool debug;
    public float fieldOfView;

    // Temp-lista för att hålla bools för en dörrs raycasts
    private readonly List<bool> rayHits = new();

    void Start()
    {
        if (doors != null)
        {
            foreach (Doors door in doors)
            {
                if (door != null)
                    door.roomCulling = this;
            }
        }

        RoomCull(false);
    }

    void Update()
    {
        if (playerInside)
            return;

        int wallLayer = 1 << 16; // byt gärna till LayerMask.GetMask("Walls")
        Transform cam = PlayerController.instance.playerCamera;

        Vector3 playerPos = cam.position;
        Vector3 playerForward = cam.forward;

        foreach (Doors door in doors)
        {
            if (door == null || door.frame == null)
                continue;

            // töm listan för denna dörr
            rayHits.Clear();

            // Basposition i mitten av dörren (lite uppåt)
            Vector3 center = door.frame.position + door.frame.up * 1.5f;

            // Offsets åt sidorna
            float invertRaypos = playerInside ? -0.2f : 0.2f;
            Vector3 rightPoint = center + door.frame.right * 0.5f + door.frame.forward * invertRaypos;
            Vector3 leftPoint = center - door.frame.right * 0.5f + door.frame.forward * invertRaypos;

            // Kör raycasts och lägg resultat i listan
            rayHits.Add(CheckRayVisible(playerPos, playerForward, center, wallLayer));
            rayHits.Add(CheckRayVisible(playerPos, playerForward, rightPoint, wallLayer));
            rayHits.Add(CheckRayVisible(playerPos, playerForward, leftPoint, wallLayer));

            // Dörren anses synlig om minst en ray lyckas
            bool visible = rayHits.Any(hit => hit);

            doorVisibility[door] = visible;

            if (debug)
            {
                Color lineColor = visible ? Color.green : Color.red;
                Debug.DrawLine(playerPos, center, lineColor);
                Debug.DrawLine(playerPos, rightPoint, lineColor);
                Debug.DrawLine(playerPos, leftPoint, lineColor);
            }
        }

        // --- NY LOGIK BASERAD PÅ ROTATION ---

        // Lista på dörrar som är synliga
        var visibleDoors = doors.Where(door =>
            doorVisibility.TryGetValue(door, out bool visible) && visible).ToList();

        // Om inga dörrar är synliga → culla rummet
        if (!visibleDoors.Any())
        {
            if (roomActive)
            {
                if (debug)
                    Debug.Log("1 - Cull Room (no visible doors)");
                RoomCull(false);
            }
            return;
        }

        // Om alla synliga dörrar är stängda → culla rummet
        if (visibleDoors.All(door => !door.doorOpen))
        {
            if (roomActive)
            {
                if (debug)
                    Debug.Log("2 - Cull Room (all visible doors closed)");
                RoomCull(false);
            }
            return;
        }

        // Om minst en synlig dörr är öppen → visa rummet
        if (visibleDoors.Any(door => door.doorOpen) && !roomActive)
        {
            if (debug)
                Debug.Log("3 - Reveal Room (visible door open)");
            RoomCull(true);
        }
    }

    private bool CheckRayVisible(Vector3 playerPos, Vector3 playerForward, Vector3 targetPoint, int wallLayer)
    {
        Vector3 dir = (targetPoint - playerPos).normalized;

        float dot = Vector3.Dot(playerForward, dir);
        if (dot <= fieldOfView)
            return false;

        if (Physics.Linecast(playerPos, targetPoint, out RaycastHit hit, wallLayer))
            return false;

        return true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !playerInside)
        {
            RoomCull(true);
            playerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && playerInside)
        {
            playerInside = false;
        }
    }

    public void RoomCull(bool _state)
    {
        // Om vi är inne i rummet ska vi aldrig stänga av det
        if (_state == false && playerInside)
            return;

        roomActive = _state;
        roomObjects.SetActive(_state);
    }
}
