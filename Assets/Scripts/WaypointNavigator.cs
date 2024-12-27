using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointNavigator : MonoBehaviour
{
    NPCController controller;
    public Waypoint currentWaypoint;

    private void Awake()
    {
        controller = GetComponent<NPCController>();
    }

    private void Start()
    {
        controller.SetDestination(currentWaypoint.GetPosition());
    }

    private void Update()
    {
        if(controller.reachedDestination == true)
        {
            currentWaypoint = currentWaypoint.nextWaypoint;
            controller.SetDestination(currentWaypoint.GetPosition());
        }
    }
}
