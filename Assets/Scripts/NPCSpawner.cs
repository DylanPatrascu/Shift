using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public GameObject[] carPrefabs = new GameObject[5];
    public int carsPerLane;
    public GameObject[] carLanes = new GameObject[4];
    public GameObject trafficParent; // Parent object for all spawned cars

    void Start()
    {
        StartCoroutine(Spawn());
    }

    IEnumerator Spawn()
    {
        // Loop through each lane
        foreach (GameObject lane in carLanes)
        {
            int count = 0;
            // Collect all waypoints from the current lane
            Transform[] waypoints = lane.GetComponentsInChildren<Transform>();
            List<Transform> availableWaypoints = new List<Transform>(waypoints);

            // Remove the parent lane transform from waypoints
            availableWaypoints.Remove(lane.transform);

            // Ensure there are enough waypoints
            if (carsPerLane > availableWaypoints.Count)
            {
                Debug.LogWarning($"Not enough waypoints in lane {lane.name} for the requested number of cars.");
                yield break;
            }

            while (count < carsPerLane)
            {
                // Randomly select a car prefab
                GameObject carPrefab = carPrefabs[Random.Range(0, carPrefabs.Length)];

                // Randomly select a waypoint
                int waypointIndex = Random.Range(0, availableWaypoints.Count);
                Transform selectedWaypoint = availableWaypoints[waypointIndex];
                availableWaypoints.RemoveAt(waypointIndex); // Ensure uniqueness

                // Adjust spawn position to be 2 units above the waypoint
                Vector3 spawnPosition = selectedWaypoint.position + new Vector3(0, 1, 0);
                Quaternion spawnRotation = selectedWaypoint.rotation; // Use waypoint's rotation

                // Instantiate car at the adjusted position and rotation
                GameObject car = Instantiate(carPrefab, spawnPosition, spawnRotation);

                // Assign the car to the trafficParent as its parent
                if (trafficParent != null)
                {
                    car.transform.SetParent(trafficParent.transform);
                }
                else
                {
                    Debug.LogWarning("TrafficParent is not assigned. Cars will not be parented.");
                }

                // Assign the waypoint to the car's WaypointNavigator
                WaypointNavigator navigator = car.GetComponent<WaypointNavigator>();
                if (navigator != null)
                {
                    navigator.currentWaypoint = selectedWaypoint.GetComponent<Waypoint>();
                }

                count++;
                yield return null; // Wait for next frame before spawning the next car
            }
        }
    }
}
