using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatsWaypointController : MonoBehaviour
{
    public static RatsWaypointController instance;
    public Transform waypointParent;
    public List<Waypoint> allWaypoints;
    public List<Waypoint> availableWaypoints;

    // Start is called before the first frame update
    private void Awake()
    {
        instance = this;
        GetWaypoints(); // hitta alla waypoints som finns i spelet
        UpdateAvailableWaypoints(); // gör alla waypoint aktiva eller ej
        GetActiveWaypoints(); // hämta alla aktiva waypoints till en lista
    }
    void Start()
    {
        
    }
    
    private void GetWaypoints()
    {
        for (int i = 0; i < waypointParent.childCount; i++)
        {
            waypointParent.GetChild(i).gameObject.AddComponent<Waypoint>();
            allWaypoints.Add(waypointParent.GetChild(i).gameObject.GetComponent<Waypoint>());

        }
    }

    private void UpdateAvailableWaypoints()
    {
        int activeFloor = 1; // den våningen eller sektionen av kartan vi befinner oss på
        for (int i = 0; i < allWaypoints.Count; i ++)
        {
            if (activeFloor == 1)
                allWaypoints[i].available = true;
        }
    }
    private void GetActiveWaypoints()
    {
        availableWaypoints.Clear();
        for (int i = 0; i < allWaypoints.Count; i++)
        {
            if (allWaypoints[i].available)
                availableWaypoints.Add(allWaypoints[i]);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
