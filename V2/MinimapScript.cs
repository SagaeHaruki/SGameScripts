using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapScript : MonoBehaviour
{
    public Transform player; // Reference to the player's transform
    public float heightOffset = 30f; // The default height offset of the minimap
    public float climbOffset = 5f; // The additional height offset when climbing
    public float movementSpeed = 5f; // Speed of camera movement

    private Vector3 originalPosition; // Store the original position of the minimap camera

    private void Start()
    {
        originalPosition = transform.position;
    }

    private void Update()
    {
        // Calculate the target position with height offset
        Vector3 targetPosition = player.position + Vector3.up * heightOffset;

        // Smoothly move the camera towards the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * movementSpeed);

        StartCoroutine(MoveCamera(targetPosition));
    }
    private System.Collections.IEnumerator MoveCamera(Vector3 targetPosition)
    {
        float elapsedTime = 0f;
        Vector3 startingPosition = transform.position;

        while (elapsedTime < 1f)
        {
            // Interpolate the camera position towards the target over time
            transform.position = Vector3.Lerp(startingPosition, targetPosition, elapsedTime);
            elapsedTime += Time.deltaTime * movementSpeed;

            yield return null;
        }

        // Ensure the camera reaches the exact target position
        transform.position = targetPosition;
    }
}
