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
    public float minDistance = 0.2f;
    public float minStopDuration = 1f;
    public float maxStopDuration = 3f;

    private bool isWaiting = false;
    private int currentWaypoint = 0;
    private float waitTimer = 0f;
    private float stopDuration = 0f;

    void Start()
    {
    }

    void Update()
    {
        if (waypoints.Length == 0)
            return;

        transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypoint].position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, waypoints[currentWaypoint].position) <= minDistance)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
            Thread.Sleep(1000);
        }

        RotateTowardsWaypoint();
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
