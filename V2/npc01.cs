using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class npc01 : MonoBehaviour
{
    public Transform[] waypoints;
    public float moveSpeed = 3f;
    public float minDistance = 0.4f;
    public float minStopDuration = 1f;
    public float maxStopDuration = 3f;

    private int currentWaypoint = 0;
    private bool isWaiting = false;
    private float waitTimer = 0f;
    private float stopDuration = 0f;

    void Start()
    {
        MoveToWaypoint();
    }

    void Update()
    {
        if (waypoints.Length == 0)
            return;

        if (isWaiting)
        {
            WaitAtWaypoint();
        }
        else
        {
            MoveToWaypoint();
        }
        RotateTowardsWaypoint();
    }

    void MoveToWaypoint()
    {
        // This will move the npc towards the set waypoint
        transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypoint].position, moveSpeed * Time.deltaTime);

        // This will check if the npc is close to the waypoint
        if (Vector3.Distance(transform.position, waypoints[currentWaypoint].position) <= minDistance)
        {
            StopAtWaypoint();
        }
    }

    void StopAtWaypoint()
    {
        isWaiting = true;
        stopDuration = Random.Range(minStopDuration, maxStopDuration); 
        waitTimer = 0f;
    }

    void WaitAtWaypoint()
    {
        waitTimer += Time.deltaTime;

        if (waitTimer >= stopDuration)
        {
            isWaiting = false;
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        }
    }

    void RotateTowardsWaypoint()
    {
        Vector3 direction = (waypoints[currentWaypoint].position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        Vector3 euler = lookRotation.eulerAngles;
        euler.x = 0f;
        euler.z = 0f;
        Quaternion finalRot = Quaternion.Euler(euler);
        transform.rotation = Quaternion.Slerp(transform.rotation, finalRot, Time.deltaTime * 5f);
    }
}
